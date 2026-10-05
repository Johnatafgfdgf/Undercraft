#!/usr/bin/env python3
"""Detect and extract legacy TranslaTale-style UNDERTALE data.win changes.

The tool never redistributes game assets. It reads a user-owned data.win and can
export only structural metadata / logical string and font descriptions needed by
Undercraft's compatibility pipeline.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import struct
from pathlib import Path


def chunks(raw: bytes) -> dict[str, tuple[int, int, int, int]]:
    if raw[:4] != b"FORM":
        raise ValueError("not a GameMaker FORM archive")
    result: dict[str, tuple[int, int, int, int]] = {}
    pos = 8
    while pos + 8 <= len(raw):
        tag = raw[pos:pos + 4].decode("ascii", "replace")
        length = struct.unpack_from("<I", raw, pos + 4)[0]
        end = pos + 8 + length
        if end > len(raw):
            raise ValueError(f"chunk {tag} extends past EOF")
        result[tag] = (pos, pos + 8, length, end)
        pos = end
    return result


def read_standard_string(raw: bytes, pos: int) -> tuple[str, int, bytes]:
    length = struct.unpack_from("<I", raw, pos)[0]
    end = pos + 4 + length
    if end >= len(raw) or raw[end] != 0:
        raise ValueError(f"invalid standard string at 0x{pos:X}")
    data = raw[pos + 4:end]
    return data.decode("utf-8", "replace"), end + 1, data


def read_legacy_string(raw: bytes, pos: int) -> tuple[str, int, bytes, int]:
    """Read the variant written by old gmktool/TranslaTale.

    Some translated strings store a Unicode character count while the payload is
    UTF-8, so byte count and declared count can differ when accents are present.
    """
    declared = struct.unpack_from("<I", raw, pos)[0]
    limit = min(len(raw), pos + 4 + max(4096, declared * 8 + 128))
    end = raw.find(b"\0", pos + 4, limit)
    if end < 0:
        raise ValueError(f"legacy string at 0x{pos:X} has no terminator")
    data = raw[pos + 4:end]
    return data.decode("utf-8", "replace"), end + 1, data, declared


def texture_page_item(raw: bytes, pos: int) -> dict[str, int]:
    values = struct.unpack_from("<HHHHHHHHHHh", raw, pos)
    names = (
        "source_x", "source_y", "source_w", "source_h",
        "target_x", "target_y", "target_w", "target_h",
        "bounding_w", "bounding_h", "texture_index",
    )
    return dict(zip(names, values))


def parse_font(raw: bytes, pos: int, tp_address_to_index: dict[int, int],
               string_reader) -> dict:
    name_ptr, display_ptr = struct.unpack_from("<II", raw, pos)
    em_size, bold, italic = struct.unpack_from("<III", raw, pos + 8)
    range_start = struct.unpack_from("<H", raw, pos + 20)[0]
    charset = raw[pos + 22]
    antialias = raw[pos + 23]
    range_end, texture_ptr = struct.unpack_from("<II", raw, pos + 24)
    scale_x, scale_y = struct.unpack_from("<ff", raw, pos + 32)
    glyph_count = struct.unpack_from("<I", raw, pos + 40)[0]
    glyph_ptrs = (
        struct.unpack_from("<" + ("I" * glyph_count), raw, pos + 44)
        if glyph_count else ()
    )

    glyphs = []
    for glyph_ptr in glyph_ptrs:
        character, sx, sy, sw, sh, shift, offset = struct.unpack_from(
            "<HHHHHhh", raw, glyph_ptr
        )
        kern_count = struct.unpack_from("<H", raw, glyph_ptr + 14)[0]
        kernings = [
            list(struct.unpack_from("<hh", raw, glyph_ptr + 16 + (i * 4)))
            for i in range(kern_count)
        ]
        glyphs.append({
            "character": character,
            "source_x": sx,
            "source_y": sy,
            "source_w": sw,
            "source_h": sh,
            "shift": shift,
            "offset": offset,
            "kerning": kernings,
        })

    return {
        "name": string_reader(name_ptr),
        "display_name": string_reader(display_ptr),
        "em_size": em_size,
        "bold": bool(bold),
        "italic": bool(italic),
        "range_start": range_start,
        "charset": charset,
        "anti_aliasing": antialias,
        "range_end": range_end,
        "scale_x": scale_x,
        "scale_y": scale_y,
        "texture_pointer": texture_ptr,
        "texture_existing_index": tp_address_to_index.get(texture_ptr),
        "texture": texture_page_item(raw, texture_ptr),
        "glyph_count": glyph_count,
        "glyphs": glyphs,
    }


def inspect(path: Path, include_payload: bool) -> dict:
    raw = path.read_bytes()
    table = chunks(raw)
    if "STRG" not in table or "FONT" not in table:
        raise ValueError("required UNDERTALE chunks not found")

    _, strg_data, _, strg_end = table["STRG"]
    string_count = struct.unpack_from("<I", raw, strg_data)[0]
    logical_ptrs = list(struct.unpack_from(
        "<" + ("I" * string_count), raw, strg_data + 4
    ))

    sequential_ptrs: list[int] = []
    pos = strg_data + 4 + string_count * 4
    base_strings: list[str] = []
    for _ in range(string_count):
        sequential_ptrs.append(pos)
        text, pos, _ = read_standard_string(raw, pos)
        base_strings.append(text)
    base_end = pos

    logical_strings: list[dict] = []
    repointed = 0
    for i, (logical_ptr, base_ptr) in enumerate(zip(logical_ptrs, sequential_ptrs)):
        if sequential_ptrs[0] <= logical_ptr < base_end:
            text, _, payload = read_standard_string(raw, logical_ptr)
            declared = len(payload)
        else:
            text, _, payload, declared = read_legacy_string(raw, logical_ptr)

        changed = logical_ptr != base_ptr or text != base_strings[i]
        repointed += int(changed)
        entry = {
            "index": i,
            "changed": changed,
            "declared_length": declared,
            "utf8_length": len(payload),
        }
        if include_payload:
            entry["text"] = text
        logical_strings.append(entry)

    # Texture page pointer table lets us detect fonts whose TPAG record was
    # appended outside the canonical TPAG chunk.
    _, tpag_data, _, tpag_end = table["TPAG"]
    tpag_count = struct.unpack_from("<I", raw, tpag_data)[0]
    tpag_ptrs = list(struct.unpack_from(
        "<" + ("I" * tpag_count), raw, tpag_data + 4
    ))
    tpag_lookup = {ptr: i for i, ptr in enumerate(tpag_ptrs)}

    _, font_data, _, font_end = table["FONT"]
    font_count = struct.unpack_from("<I", raw, font_data)[0]
    font_ptrs = list(struct.unpack_from(
        "<" + ("I" * font_count), raw, font_data + 4
    ))
    external_font_ptrs = [
        ptr for ptr in font_ptrs if not (font_data <= ptr < font_end)
    ]

    def any_string(pos_: int) -> str:
        # Standard strings can exist both in the canonical sequence and in the
        # TranslaTale tail. NUL scanning handles both representations.
        declared = struct.unpack_from("<I", raw, pos_)[0]
        exact = pos_ + 4 + declared
        if exact < len(raw) and raw[exact] == 0:
            return raw[pos_ + 4:exact].decode("utf-8", "replace")
        return read_legacy_string(raw, pos_)[0]

    fonts = [
        parse_font(raw, ptr, tpag_lookup, any_string)
        for ptr in font_ptrs
    ] if include_payload else []

    return {
        "sha256": hashlib.sha256(raw).hexdigest(),
        "size": len(raw),
        "chunks": {
            name: {
                "header": header,
                "data": data,
                "length": length,
                "end": end,
            }
            for name, (header, data, length, end) in table.items()
        },
        "legacy_translatale": {
            "detected": bool(external_font_ptrs or repointed),
            "string_count": string_count,
            "repointed_or_changed_strings": repointed,
            "canonical_string_data_end": base_end,
            "strg_declared_end": strg_end,
            "font_count": font_count,
            "external_font_pointer_count": len(external_font_ptrs),
            "external_font_pointers": external_font_ptrs,
            "tpag_count": tpag_count,
        },
        "strings": logical_strings if include_payload else [],
        "fonts": fonts,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("data_win", type=Path)
    parser.add_argument("--export", type=Path,
                        help="write recoverable string/font metadata as JSON")
    args = parser.parse_args()

    payload = inspect(args.data_win, include_payload=args.export is not None)
    summary = payload["legacy_translatale"]
    print(json.dumps({
        "sha256": payload["sha256"],
        "size": payload["size"],
        **summary,
    }, indent=2))

    if args.export:
        args.export.write_text(
            json.dumps(payload, ensure_ascii=False),
            encoding="utf-8",
        )
        print(f"recovery metadata written to {args.export}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
