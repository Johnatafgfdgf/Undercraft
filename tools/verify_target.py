#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "mod" / "manifest.json"

def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda: f.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()

def main() -> int:
    if len(sys.argv) != 2:
        print("usage: verify_target.py /path/to/data.win", file=sys.stderr)
        return 2

    path = Path(sys.argv[1])
    if not path.is_file():
        print(f"missing file: {path}", file=sys.stderr)
        return 2

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    size = path.stat().st_size
    digest = sha256(path)

    for build in manifest.get("supported_builds", []):
        if build.get("data_win_sha256") == digest and build.get("data_win_size") == size:
            print(f"supported Undertale build: {digest}")
            return 0

    print("unsupported data.win build", file=sys.stderr)
    print(f"size:   {size}", file=sys.stderr)
    print(f"sha256: {digest}", file=sys.stderr)
    return 1

if __name__ == "__main__":
    raise SystemExit(main())
