from __future__ import annotations

import stat
import zipfile
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
VERSION = "0.20.2"
BUILDS = PROJECT / f"{VERSION} builds"
ARCHIVES = (
    BUILDS / "Android" / f"NTS Honeymoon {VERSION}.apk",
    BUILDS / f"NTS Honeymoon {VERSION} Windows.zip",
    BUILDS / f"NTS Honeymoon {VERSION} Linux.zip",
    BUILDS / f"NTS Honeymoon {VERSION} Mac.zip",
)


for archive_path in ARCHIVES:
    if not archive_path.is_file():
        raise FileNotFoundError(archive_path)
    with zipfile.ZipFile(archive_path) as archive:
        broken = archive.testzip()
        if broken is not None:
            raise RuntimeError(f"Corrupt entry in {archive_path.name}: {broken}")
    print(f"Integrity OK: {archive_path.name}", flush=True)

permission_checks = {
    BUILDS / f"NTS Honeymoon {VERSION} Linux.zip": f"NTS Honeymoon {VERSION}.x86_64",
    BUILDS / f"NTS Honeymoon {VERSION} Mac.zip": ".app/Contents/MacOS/NTS_HM",
}
for archive_path, suffix in permission_checks.items():
    with zipfile.ZipFile(archive_path) as archive:
        entry = next(item for item in archive.infolist() if item.filename.endswith(suffix))
        mode = (entry.external_attr >> 16) & 0o777
        if mode != 0o755 or entry.create_system != 3:
            raise RuntimeError(f"Executable permission check failed for {entry.filename}: {oct(mode)}")
    print(f"Permission OK ({oct(mode)}): {archive_path.name}", flush=True)
