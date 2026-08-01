from __future__ import annotations

import base64
import os
import re
import sys
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
PYTHON_PACKAGES = PROJECT / "Library" / "CodexTools" / "python"
MODEL_DIR = PROJECT / "Library" / "CodexTools" / "argos-packages"
SOURCE = PROJECT / "Library" / "CodexTools" / "missing_story_japanese_english.txt"
EXISTING = PROJECT / "Library" / "CodexTools" / "existing_story_japanese.tsv"
OUTPUT = PROJECT / "Assets" / "Localization" / "Missing Japanese Dialogue.tsv"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["ARGOS_PACKAGES_DIR"] = str(MODEL_DIR)
os.environ["ARGOS_DEVICE_TYPE"] = "cpu"

import argostranslate.translate


OVERRIDES = {
    "😂": "😂",
    "Lily will remember": "Lilyは覚えている",
    "Later...": "その後…",
    "2 minutes later....": "2分後…",
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
rows = ["# base64 English<TAB>base64 Japanese"]
reused = 0
translated = 0
preserved = 0

for index, english in enumerate(english_lines, start=1):
    japanese = OVERRIDES.get(english)
    if japanese is not None:
        preserved += 1
    elif english in existing:
        japanese = existing[english]
        reused += 1
    elif not re.search(r"[A-Za-z]", english):
        japanese = english
        preserved += 1
    else:
        japanese = argostranslate.translate.translate(english, "en", "ja").strip()
        translated += 1

    if not japanese:
        raise RuntimeError(f"Empty Japanese translation for row {index}: {english!r}")
    rows.append(f"{encode(english)}\t{encode(japanese)}")
    if index % 50 == 0 or index == len(english_lines):
        print(f"Prepared {index}/{len(english_lines)}", flush=True)

OUTPUT.write_text("\n".join(rows) + "\n", encoding="utf-8")
print(
    f"Wrote {len(english_lines)} mappings: reused={reused}, translated={translated}, preserved={preserved}",
    flush=True,
)
