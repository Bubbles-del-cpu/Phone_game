from __future__ import annotations

import os
import sys
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
PYTHON_PACKAGES = PROJECT / "Library" / "CodexTools" / "python"
SNAPSHOTS = (
    PROJECT
    / "Library"
    / "CodexTools"
    / "huggingface"
    / "hub"
    / "models--facebook--nllb-200-distilled-600M"
    / "snapshots"
)
MODEL = SNAPSHOTS / "f8d333a098d19b4fd9a8b18f94170487ad3f821d"
OUTPUT = PROJECT / "Library" / "CodexTools" / "nllb-ct2-int8"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["HF_HUB_OFFLINE"] = "1"
os.environ["TRANSFORMERS_OFFLINE"] = "1"
sys.stdout.reconfigure(encoding="utf-8")

from ctranslate2.converters import TransformersConverter


converter = TransformersConverter(str(MODEL), low_cpu_mem_usage=False)
converted = converter.convert(str(OUTPUT), quantization="int8", force=True)
print(f"Converted NLLB to {converted}", flush=True)
