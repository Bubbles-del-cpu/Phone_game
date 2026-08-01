from __future__ import annotations

import base64
import os
import sys
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
PYTHON_PACKAGES = PROJECT / "Library" / "CodexTools" / "python"
MODEL_DIR = PROJECT / "Library" / "CodexTools" / "argos-packages"
SOURCE = PROJECT / "Library" / "CodexTools" / "update020_english.txt"
OUTPUT = (
    PROJECT
    / "Assets"
    / "Entities"
    / "Dialogue"
    / "ep 27 and ep 27.5"
    / "scripts"
    / "Update 0.20 Japanese.tsv"
)

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["ARGOS_PACKAGES_DIR"] = str(MODEL_DIR)
os.environ["ARGOS_DEVICE_TYPE"] = "cpu"

import argostranslate.translate


OVERRIDES = {
    "😂": "😂",
    "Lily will remember": "Lilyは覚えている",
    "Later...": "その後…",
    "2 minutes later....": "2分後…",
    "And he used something": "それに、彼はちゃんと着けてた",
    "Obviously aswell": "もちろん、それも",
    "She's asleep. Went out like a light.": "彼女は眠ったよ。泥のようにぐっすりだ。",
    "Did he text you?": "彼から連絡は来たか？",
    "But two men... I think this was a once in a lifetime experience...":
        "でも二人の男となんて…一生に一度の経験だったと思う…",
}


def decode(value: str) -> str:
    return base64.b64decode(value).decode("utf-8")


def encode(value: str) -> str:
    return base64.b64encode(value.encode("utf-8")).decode("ascii")


english_lines = [decode(line.strip()) for line in SOURCE.read_text(encoding="utf-8").splitlines() if line.strip()]
rows: list[str] = ["# base64 English<TAB>base64 Japanese"]

for index, english in enumerate(english_lines, start=1):
    japanese = OVERRIDES.get(english)
    if japanese is None:
        japanese = argostranslate.translate.translate(english, "en", "ja").strip()
    if not japanese:
        raise RuntimeError(f"Empty Japanese translation for row {index}: {english!r}")
    rows.append(f"{encode(english)}\t{encode(japanese)}")
    if index % 25 == 0 or index == len(english_lines):
        print(f"Translated {index}/{len(english_lines)}", flush=True)

OUTPUT.write_text("\n".join(rows) + "\n", encoding="utf-8")
print(f"Wrote {len(english_lines)} translations to {OUTPUT}", flush=True)
