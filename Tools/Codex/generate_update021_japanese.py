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
ROOT = PROJECT / "Assets" / "Entities" / "Dialogue" / "ep28 and ep28.5"
SOURCES = [ROOT / "NTS HM EP 28.txt", ROOT / "NTS HM EP 28.5.txt"]
EXISTING = PROJECT / "Library" / "CodexTools" / "existing_story_japanese.tsv"
OUTPUT = ROOT / "Update 0.21 Japanese.tsv"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["HF_HOME"] = str(HF_HOME)
os.environ["HF_HUB_DISABLE_TELEMETRY"] = "1"
sys.stdout.reconfigure(encoding="utf-8")

import ctranslate2
from transformers import AutoTokenizer


TIME_MARKERS = {
    "In the afternoon...",
    "A few minutes later...",
    "Shortly after...",
    "Later that afternoon...",
    "Late that afternoon...",
    "That evening...",
    "Early the next morning...",
    "A little later...",
    "Mid-morning...",
    "Later in the morning...",
    "Late in the morning...",
    "Near midday...",
    "Around lunchtime...",
    "After lunch...",
    "Late in the afternoon...",
    "Later that evening...",
    "Near the end of dinner...",
    "Ten minutes later...",
    "Five minutes later...",
    "Two minutes later...",
    "A minute later...",
    "About forty minutes later...",
}

OVERRIDES = {
    "...": "…",
    "I think he's—": "彼はもう—",
    "How did that happen?": "どうしてそうなったの？",
    "In the afternoon...": "午後…",
    "A few minutes later...": "数分後…",
    "Shortly after...": "その少し後…",
    "Later that afternoon...": "その日の午後遅く…",
    "Late that afternoon...": "その日の午後遅く…",
    "That evening...": "その夜…",
    "Early the next morning...": "翌朝早く…",
    "A little later...": "少し後…",
    "Mid-morning...": "午前半ば…",
    "Later in the morning...": "その日の午前遅く…",
    "Late in the morning...": "午前遅く…",
    "Near midday...": "正午近く…",
    "Around lunchtime...": "昼食の頃…",
    "After lunch...": "昼食後…",
    "Late in the afternoon...": "午後遅く…",
    "Later that evening...": "その夜遅く…",
    "Near the end of dinner...": "夕食が終わる頃…",
    "Ten minutes later...": "10分後…",
    "Five minutes later...": "5分後…",
    "Two minutes later...": "2分後…",
    "A minute later...": "1分後…",
    "About forty minutes later...": "約40分後…",
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
for source in SOURCES:
    for raw in source.read_text(encoding="utf-8-sig").splitlines():
        stripped = raw.strip()
        match = re.match(r"^(?:D|L|LI|M):\s*(.*)$", stripped)
        if match:
            text = match.group(1)
            lowered = text.lower()
            if lowered.startswith("(sends ") or lowered.startswith("(forwards "):
                continue
        elif stripped.startswith("(") and stripped.endswith(")") and stripped[1:-1] in TIME_MARKERS:
            text = stripped[1:-1]
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
    translation_inputs = [text.replace("😂", "").strip() for text in batch]
    source_tokens = [
        tokenizer.convert_ids_to_tokens(tokenizer.encode(text, truncation=True, max_length=256))
        for text in translation_inputs
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
        if "😂" in english and "😂" not in japanese:
            japanese += " 😂"
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
