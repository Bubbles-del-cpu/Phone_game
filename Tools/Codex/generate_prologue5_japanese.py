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
SOURCE = (
    PROJECT
    / "Assets"
    / "Entities"
    / "Dialogue"
    / "prologue part 5"
    / "NTS HM PROLOGUE PART 5 - THE NIGHT.txt"
)
EXISTING = PROJECT / "Library" / "CodexTools" / "existing_story_japanese.tsv"
OUTPUT = SOURCE.parent / "Prologue Part 5 Japanese.tsv"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["HF_HOME"] = str(HF_HOME)
os.environ["HF_HUB_DISABLE_TELEMETRY"] = "1"
sys.stdout.reconfigure(encoding="utf-8")

import ctranslate2
from transformers import AutoTokenizer


MODEL = "facebook/nllb-200-distilled-600M"
TIME_MARKER = re.compile(
    r"^\((Late afternoon|Two minutes later|Just after six|A few minutes later|"
    r"Forty minutes later|That evening|A little later|Several minutes later|"
    r"Half past ten|Eleven o'clock|12:42 AM|1:14 AM|1:27 AM|"
    r"Early the next morning)\.\.\.\)$",
    re.IGNORECASE,
)
OVERRIDES = {
    "...": "…",
    "Hihi.": "ふふ。",
    "Hihi": "ふふ",
    "Late afternoon...": "午後遅く…",
    "Two minutes later...": "2分後…",
    "Just after six...": "6時を少し過ぎた頃…",
    "A few minutes later...": "数分後…",
    "Forty minutes later...": "40分後…",
    "That evening...": "その夜…",
    "A little later...": "少し後…",
    "Several minutes later...": "数分後…",
    "Half past ten...": "10時半…",
    "Eleven o'clock...": "11時…",
    "12:42 AM...": "午前0時42分…",
    "1:14 AM...": "午前1時14分…",
    "1:27 AM...": "午前1時27分…",
    "Early the next morning...": "翌朝早く…",
}


def decode(value: str) -> str:
    return base64.b64decode(value).decode("utf-8")


def encode(value: str) -> str:
    return base64.b64encode(value.encode("utf-8")).decode("ascii")


existing: dict[str, str] = {}
if EXISTING.is_file():
    for line in EXISTING.read_text(encoding="utf-8").splitlines():
        if not line.strip() or line.startswith("#"):
            continue
        english64, japanese64 = line.split("\t", 1)
        existing.setdefault(decode(english64), decode(japanese64))

texts: list[str] = []
seen: set[str] = set()
source_lines = SOURCE.read_text(encoding="utf-8-sig").splitlines()
for raw in source_lines[4:628]:
    stripped = raw.strip()
    match = re.match(r"^(?:D|L|I|M):\s*(.*)$", stripped)
    if match:
        text = match.group(1)
        if text.lower().startswith("(sends picture") or text.lower().startswith("(sends video"):
            continue
    elif TIME_MARKER.match(stripped):
        text = stripped.strip("()")
    else:
        continue

    if text not in seen:
        seen.add(text)
        texts.append(text)

results: dict[str, str] = {}
pending: list[str] = []
for english in texts:
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
        ).strip()
        for result in translated
    ]
    for english, japanese in zip(batch, translations):
        if "hihi" in english.lower() and not re.search(r"ふふ|へへ|ヒヒ", japanese):
            japanese += " ふふ"
        if "<3" in english and "<3" not in japanese:
            japanese += " <3"
        if not japanese:
            raise RuntimeError(f"Empty Japanese translation for {english!r}")
        results[english] = japanese
    print(f"Translated {min(offset + len(batch), len(pending))}/{len(pending)} new lines", flush=True)

rows = ["# base64 English<TAB>base64 Japanese"]
for english in texts:
    rows.append(f"{encode(english)}\t{encode(results[english])}")
OUTPUT.write_text("\n".join(rows) + "\n", encoding="utf-8")

latin_only = [
    (english, results[english])
    for english in pending
    if re.search(r"[A-Za-z]", results[english])
    and not re.search(r"[\u3040-\u30ff\u3400-\u9fff]", results[english])
]
print(
    f"Wrote {len(texts)} unique mappings; translated={len(pending)}, "
    f"reused/protected={len(texts) - len(pending)}, latin-only={len(latin_only)}",
    flush=True,
)
for english, japanese in latin_only[:20]:
    print(f"LATIN ONLY: {english!r} -> {japanese!r}", flush=True)
