from __future__ import annotations

import os
import sys
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
PYTHON_PACKAGES = PROJECT / "Library" / "CodexTools" / "python"
HF_HOME = PROJECT / "Library" / "CodexTools" / "huggingface"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["HF_HOME"] = str(HF_HOME)
os.environ["HF_HUB_DISABLE_TELEMETRY"] = "1"
sys.stdout.reconfigure(encoding="utf-8")

import torch
from transformers import AutoModelForSeq2SeqLM, AutoTokenizer


MODEL = "facebook/nllb-200-distilled-600M"
SAMPLES = [
    "Boring...",
    "I picked out a few things. Tell me what you think!",
    "Fuck, he's just shoving it in.",
    "I think I can handle a room full of people looking at me hihi",
    "I'll do it. I'll just close my eyes and pretend it's over.",
    "Me, Dave, and Lily... we have unfinished business.",
    "That black tight dress",
    "You're gonna send them to me, Victoria",
]

tokenizer = AutoTokenizer.from_pretrained(MODEL, src_lang="eng_Latn")
model = AutoModelForSeq2SeqLM.from_pretrained(MODEL)
model.eval()
torch.set_num_threads(max(1, min(8, os.cpu_count() or 1)))

inputs = tokenizer(SAMPLES, return_tensors="pt", padding=True, truncation=True, max_length=256)
with torch.inference_mode():
    generated = model.generate(
        **inputs,
        forced_bos_token_id=tokenizer.convert_tokens_to_ids("jpn_Jpan"),
        max_new_tokens=128,
        num_beams=1,
    )
translations = tokenizer.batch_decode(generated, skip_special_tokens=True)
for english, japanese in zip(SAMPLES, translations):
    print(f"{english}\n  -> {japanese}", flush=True)
