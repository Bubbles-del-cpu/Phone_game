from __future__ import annotations

import argparse
import stat
import zipfile
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[2]
VERSION = "0.20.2"
BUILDS = PROJECT / f"{VERSION} builds"
LINUX = BUILDS / "Linux"
MAC = BUILDS / "Mac"
WINDOWS = BUILDS / "Windows"
LINUX_ZIP = BUILDS / f"NTS Honeymoon {VERSION} Linux.zip"
MAC_ZIP = BUILDS / f"NTS Honeymoon {VERSION} Mac.zip"
WINDOWS_ZIP = BUILDS / f"NTS Honeymoon {VERSION} Windows.zip"

MACH_O_MAGICS = {
    b"\xfe\xed\xfa\xce",
    b"\xce\xfa\xed\xfe",
    b"\xfe\xed\xfa\xcf",
    b"\xcf\xfa\xed\xfe",
    b"\xca\xfe\xba\xbe",
    b"\xbe\xba\xfe\xca",
}


def executable_binary(path: Path) -> bool:
    with path.open("rb") as stream:
        magic = stream.read(4)
    return magic == b"\x7fELF" or magic in MACH_O_MAGICS


def archive_directory(source: Path, destination: Path) -> None:
    if destination.exists():
        destination.unlink()

    with zipfile.ZipFile(
        destination,
        mode="w",
        compression=zipfile.ZIP_DEFLATED,
        compresslevel=6,
        allowZip64=True,
    ) as archive:
        for path in sorted(source.rglob("*")):
            relative = path.relative_to(source).as_posix()
            if path.is_dir():
                info = zipfile.ZipInfo(relative.rstrip("/") + "/")
                info.create_system = 3
                info.external_attr = (stat.S_IFDIR | 0o755) << 16
                archive.writestr(info, b"")
                continue

            mode = 0o755 if executable_binary(path) else 0o644
            info = zipfile.ZipInfo.from_file(path, arcname=relative)
            info.create_system = 3
            info.external_attr = (stat.S_IFREG | mode) << 16
            info.compress_type = zipfile.ZIP_DEFLATED
            info._compresslevel = 6
            with path.open("rb") as source_stream, archive.open(info, "w", force_zip64=True) as target_stream:
                while chunk := source_stream.read(1024 * 1024):
                    target_stream.write(chunk)


def verify_permissions(archive_path: Path, expected_suffix: str) -> int:
    with zipfile.ZipFile(archive_path) as archive:
        matches = [item for item in archive.infolist() if item.filename.endswith(expected_suffix)]
        if len(matches) != 1:
            raise RuntimeError(f"Expected one {expected_suffix!r} in {archive_path}, found {len(matches)}")
        mode = (matches[0].external_attr >> 16) & 0o777
        if mode != 0o755:
            raise RuntimeError(f"Wrong executable mode in {archive_path}: {oct(mode)}")
        return mode


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("targets", nargs="*", choices=("linux", "mac", "windows"))
    args = parser.parse_args()
    targets = set(args.targets or ("linux", "mac"))

    if "linux" in targets:
        archive_directory(LINUX, LINUX_ZIP)
        print(f"Created {LINUX_ZIP.name} ({LINUX_ZIP.stat().st_size / 1024 / 1024:.2f} MB)", flush=True)
        linux_mode = verify_permissions(LINUX_ZIP, f"NTS Honeymoon {VERSION}.x86_64")
        print(f"Verified Linux executable mode {oct(linux_mode)}", flush=True)

    if "mac" in targets:
        archive_directory(MAC, MAC_ZIP)
        print(f"Created {MAC_ZIP.name} ({MAC_ZIP.stat().st_size / 1024 / 1024:.2f} MB)", flush=True)
        mac_mode = verify_permissions(MAC_ZIP, ".app/Contents/MacOS/NTS_HM")
        print(f"Verified macOS executable mode {oct(mac_mode)}", flush=True)

    if "windows" in targets:
        archive_directory(WINDOWS, WINDOWS_ZIP)
        print(f"Created {WINDOWS_ZIP.name} ({WINDOWS_ZIP.stat().st_size / 1024 / 1024:.2f} MB)", flush=True)


if __name__ == "__main__":
    main()
