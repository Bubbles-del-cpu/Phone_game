from __future__ import annotations

import os
import sys
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
PYTHON_PACKAGES = PROJECT / "Library" / "CodexTools" / "python"
MODEL_DIR = PROJECT / "Library" / "CodexTools" / "argos-packages"

sys.path.insert(0, str(PYTHON_PACKAGES))
os.environ["ARGOS_PACKAGES_DIR"] = str(MODEL_DIR)
os.environ["ARGOS_DEVICE_TYPE"] = "cpu"

import argostranslate.package


MODEL_DIR.mkdir(parents=True, exist_ok=True)
argostranslate.package.update_package_index()
packages = argostranslate.package.get_available_packages()
package = next(
    item for item in packages if item.from_code == "en" and item.to_code == "ja"
)
downloaded = package.download()
argostranslate.package.install_from_path(downloaded)
print(f"Installed en-ja model into {MODEL_DIR}")
