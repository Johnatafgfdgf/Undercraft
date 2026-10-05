#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 2 ]]; then
  echo "Usage: $0 /path/to/UNDERTALE /path/to/UndertaleModCli"
  exit 2
fi

GAME_DIR="$(cd "$1" && pwd)"
UTMT="$2"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/build/UNDERTALE-Undercraft"
DATA="$GAME_DIR/data.win"

if [[ ! -f "$DATA" ]]; then
  echo "data.win not found in: $GAME_DIR"
  exit 1
fi

python3 "$ROOT/tools/verify_target.py" "$DATA"
rm -rf "$OUT"
mkdir -p "$OUT"

cp "$GAME_DIR/UNDERTALE.exe" "$OUT/" 2>/dev/null || true
cp "$GAME_DIR/options.ini" "$OUT/" 2>/dev/null || true
cp "$GAME_DIR/credits.txt" "$OUT/" 2>/dev/null || true

"$UTMT" load "$DATA" -s "$ROOT/mod/UndercraftBootstrap.csx" -o "$OUT/data.win" --overwrite

echo "Undercraft build created at: $OUT"
