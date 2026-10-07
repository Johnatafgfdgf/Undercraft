#!/usr/bin/env python3
"""Prepare Minecraft texture sources for Undercraft's 2D-only pipeline.

This tool deliberately keeps upstream assets outside git. It creates a local
staging directory that later UTMT build steps can consume.
"""
from __future__ import annotations

import argparse
import json
import shutil
import urllib.request
import zipfile
from pathlib import Path

DEFAULT_SOURCE = "https://github.com/Mojang/bedrock-samples/archive/refs/heads/main.zip"
FALLBACK_SOURCE = "https://github.com/KygekDev/default-textures/archive/refs/heads/master.zip"

KEEP_DIRS = {
    "blocks",
    "items",
    "ui",
    "gui",
    "particle",
    "environment",
    "entity",
    "models",
}


def download(url: str, target: Path) -> None:
    target.parent.mkdir(parents=True, exist_ok=True)
    with urllib.request.urlopen(url) as response, target.open("wb") as out:
        shutil.copyfileobj(response, out)


def find_texture_root(root: Path) -> Path:
    candidates = list(root.glob("**/resource_pack/textures")) + list(root.glob("**/textures"))
    candidates = [p for p in candidates if p.is_dir()]
    if not candidates:
        raise RuntimeError("No textures directory found in downloaded source")
    candidates.sort(key=lambda p: ("resource_pack" not in str(p), len(p.parts)))
    return candidates[0]


def stage(source: Path, output: Path) -> None:
    texture_root = find_texture_root(source)
    output.mkdir(parents=True, exist_ok=True)
    for name in KEEP_DIRS:
        src = texture_root / name
        if src.exists():
            dst = output / name
            if dst.exists():
                shutil.rmtree(dst)
            shutil.copytree(src, dst)

    meta = {
        "runtime_mode": "2d_only",
        "texture_root": str(texture_root),
        "categories": sorted(p.name for p in output.iterdir() if p.is_dir()),
        "note": "3D models are reference-only; Undercraft runtime uses 2D sprites.",
    }
    (output / "undercraft_asset_stage.json").write_text(
        json.dumps(meta, indent=2), encoding="utf-8"
    )


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--url", default=DEFAULT_SOURCE)
    ap.add_argument("--fallback-url", default=FALLBACK_SOURCE)
    ap.add_argument("--work", default="build/minecraft-source")
    ap.add_argument("--out", default="build/minecraft-2d")
    args = ap.parse_args()

    work = Path(args.work)
    out = Path(args.out)
    archive = work / "source.zip"
    extracted = work / "extracted"

    if work.exists():
        shutil.rmtree(work)
    work.mkdir(parents=True)

    last_error = None
    for url in (args.url, args.fallback_url):
        try:
            download(url, archive)
            with zipfile.ZipFile(archive) as zf:
                zf.extractall(extracted)
            stage(extracted, out)
            print(f"Prepared 2D source assets in {out}")
            return 0
        except Exception as exc:
            last_error = exc
            if extracted.exists():
                shutil.rmtree(extracted)
            if archive.exists():
                archive.unlink()

    raise SystemExit(f"Failed to prepare texture source: {last_error}")


if __name__ == "__main__":
    raise SystemExit(main())
