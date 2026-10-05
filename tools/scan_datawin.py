#!/usr/bin/env python3
"""Minimal GameMaker data.win scanner used by Undercraft.

This intentionally does not extract or redistribute game assets. It fingerprints
an owner-provided data.win and prints the top-level GameMaker chunk layout so the
patch pipeline can reject unknown builds before touching them.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import struct
from pathlib import Path


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda: f.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def parse_chunks(path: Path) -> list[dict[str, int | str]]:
    raw = path.read_bytes()
    if len(raw) < 8 or raw[:4] != b"FORM":
        raise ValueError("Not a GameMaker FORM archive")

    declared_size = struct.unpack_from("<I", raw, 4)[0]
    if declared_size + 8 > len(raw):
        raise ValueError("Truncated FORM archive")

    chunks: list[dict[str, int | str]] = []
    pos = 8
    while pos + 8 <= len(raw):
        tag_bytes = raw[pos : pos + 4]
        try:
            tag = tag_bytes.decode("ascii")
        except UnicodeDecodeError as exc:
            raise ValueError(f"Invalid chunk tag at 0x{pos:X}") from exc

        length = struct.unpack_from("<I", raw, pos + 4)[0]
        data_offset = pos + 8
        end = data_offset + length
        if end > len(raw):
            raise ValueError(f"Chunk {tag} extends past end of file")

        chunks.append(
            {
                "tag": tag,
                "header_offset": pos,
                "data_offset": data_offset,
                "length": length,
            }
        )
        pos = end

    return chunks


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("data_win", type=Path)
    parser.add_argument("--json", action="store_true")
    args = parser.parse_args()

    path: Path = args.data_win
    if not path.is_file():
        raise SystemExit(f"File not found: {path}")

    result = {
        "path": str(path),
        "size": path.stat().st_size,
        "sha256": sha256(path),
        "chunks": parse_chunks(path),
    }

    if args.json:
        print(json.dumps(result, indent=2))
    else:
        print(f"data.win: {result['size']} bytes")
        print(f"sha256:   {result['sha256']}")
        print("chunks:")
        for chunk in result["chunks"]:
            print(
                f"  {chunk['tag']:4}  "
                f"offset=0x{chunk['data_offset']:08X}  "
                f"size={chunk['length']}"
            )

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
