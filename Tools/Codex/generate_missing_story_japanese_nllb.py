from __future__ import annotations

import base64
import os
import re
import sys
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
PYTHON_PACKAGES = PROJECT / "Library" / "CodexTools" / "python"
HF_HOME = PROJECT / "Library" / "CodexTools" / "huggingface"
SNAPSHOT = (
    HF_HOME
    / "hub"
    / "models--facebook--nllb-200-distilled-600M"
    / "snapshots"
    / "f8d333a098d19b4fd9a8b18f94170487ad3f821d"
)
CT2_MODEL = PROJECT / "Library" / "CodexTools" / "nllb-ct2-int8"
SOURCE = PROJECT / "Library" / "CodexTools" / "missing_story_japanese_english.txt"
EXISTING = PROJECT / "Library" / "CodexTools" / "existing_story_japanese.tsv"
OUTPUT = PROJECT / "Assets" / "Localization" / "Missing Japanese Dialogue.tsv"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["HF_HOME"] = str(HF_HOME)
os.environ["HF_HUB_DISABLE_TELEMETRY"] = "1"
sys.stdout.reconfigure(encoding="utf-8")

import ctranslate2
from transformers import AutoTokenizer


MODEL = "facebook/nllb-200-distilled-600M"
OVERRIDES = {
    "😂": "😂",
    "...": "...",
    "Boring...": "退屈…",
    "Skips big part of this Episode": "このエピソードの大部分をスキップします",
    "I picked out a few things. Tell me what you think!": "いくつか選んでみたの。どう思うか教えて！",
    "Fuck, he's just shoving it in.": "くそっ、あいつ、そのまま押し込んでる。",
    "I think I can handle a room full of people looking at me hihi": "大勢の人に見られても、私なら平気だと思う。ふふ",
    "I'll do it. I'll just close my eyes and pretend it's over.": "やるよ。目を閉じて、もう終わったふりをする。",
    "Me, Dave, and Lily... we have unfinished business.": "俺とデイブとリリーには…まだやり残したことがある。",
    "That black thight dress": "あの黒いタイトドレス",
    "That black tight dress": "あの黒いタイトドレス",
    "You're gonna send them to me Victoria": "それ、私に送ってくれるんでしょ、ビクトリア",
    "Kinda killed the mood for the night though 😂": "でも、それで今夜のムードはちょっと台無しになった😂",
}


def decode(value: str) -> str:
    return base64.b64decode(value).decode("utf-8")


def encode(value: str) -> str:
    return base64.b64encode(value.encode("utf-8")).decode("ascii")


existing: dict[str, str] = {}
for line in EXISTING.read_text(encoding="utf-8").splitlines():
    if not line.strip():
        continue
    english64, japanese64 = line.split("\t", 1)
    existing.setdefault(decode(english64), decode(japanese64))

english_lines = [
    decode(line.strip())
    for line in SOURCE.read_text(encoding="utf-8").splitlines()
    if line.strip()
]

results: dict[str, str] = {}
pending: list[str] = []
for english in english_lines:
    if english in OVERRIDES:
        results[english] = OVERRIDES[english]
    elif english in existing:
        results[english] = existing[english]
    elif not re.search(r"[A-Za-z]", english):
        results[english] = english
    else:
        pending.append(english)

tokenizer = AutoTokenizer.from_pretrained(str(SNAPSHOT), src_lang="eng_Latn", local_files_only=True)
translator = ctranslate2.Translator(
    str(CT2_MODEL),
    device="cpu",
    compute_type="int8",
    inter_threads=1,
    intra_threads=max(1, min(8, os.cpu_count() or 1)),
)

pending.sort(key=len)
for offset in range(0, len(pending), 128):
    batch = pending[offset : offset + 128]
    source_tokens = [
        tokenizer.convert_ids_to_tokens(tokenizer.encode(text, truncation=True, max_length=256))
        for text in batch
    ]
    translated = translator.translate_batch(
        source_tokens,
        target_prefix=[["jpn_Jpan"] for _ in batch],
        max_batch_size=32,
        batch_type="examples",
        beam_size=1,
        max_input_length=256,
        max_decoding_length=128,
    )
    translations = [
        tokenizer.decode(
            tokenizer.convert_tokens_to_ids(result.hypotheses[0][1:]),
            skip_special_tokens=True,
        )
        for result in translated
    ]
    for english, japanese in zip(batch, translations):
        japanese = japanese.strip()
        if "hihi" in english.lower() and not re.search(r"ふふ|へへ|ヒヒ", japanese):
            japanese += " ふふ"
        if "😂" in english and "😂" not in japanese:
            japanese += " 😂"
        if not japanese:
            raise RuntimeError(f"Empty Japanese translation for: {english!r}")
        results[english] = japanese
    print(f"Translated {min(offset + len(batch), len(pending))}/{len(pending)} new lines", flush=True)

rows = ["# base64 English<TAB>base64 Japanese"]
for english in english_lines:
    rows.append(f"{encode(english)}\t{encode(results[english])}")
OUTPUT.write_text("\n".join(rows) + "\n", encoding="utf-8")

latin_only = [
    (english, results[english])
    for english in pending
    if re.search(r"[A-Za-z]", results[english])
    and not re.search(r"[\u3040-\u30ff\u3400-\u9fff]", results[english])
]
print(
    f"Wrote {len(english_lines)} mappings; NLLB={len(pending)}, protected={len(english_lines) - len(pending)}, latin-only={len(latin_only)}",
    flush=True,
)
for english, japanese in latin_only[:20]:
    print(f"LATIN ONLY: {english!r} -> {japanese!r}", flush=True)
