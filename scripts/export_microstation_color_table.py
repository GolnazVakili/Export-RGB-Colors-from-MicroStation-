"""Export every MicroStation color-table code (0-255) with RGB, Layer, and Description.

Requires a running MicroStation session with a DGN open. Uses COM
ExtractColorTable.GetColors so unused table slots are included. Levels that
use a table color fill Layer and Description on that row (one row per level
if several share a color).

    python export_microstation_color_table.py [output.csv]
"""
from __future__ import annotations

import csv
import sys
from collections import defaultdict
from pathlib import Path


COLOR_TABLE_SIZE = 256


def unpack_colorref(packed: int) -> tuple[int, int, int]:
    packed = int(packed)
    return packed & 0xFF, (packed >> 8) & 0xFF, (packed >> 16) & 0xFF


def packed_by_index(values) -> list[int]:
    if values is None:
        return [0] * COLOR_TABLE_SIZE

    try:
        lower = int(values.LBound)
        upper = int(values.UBound)
        get = values.__getitem__
    except Exception:
        seq = list(values)
        result = [0] * max(COLOR_TABLE_SIZE, len(seq))
        for i, value in enumerate(seq):
            result[i] = int(value)
        return result

    result = [0] * max(COLOR_TABLE_SIZE, upper + 1)
    for i in range(lower, upper + 1):
        if i >= 0:
            result[i] = int(get(i))
    if lower == 1 and result[0] == 0 and (1 <= COLOR_TABLE_SIZE <= upper):
        # 1..256 array for codes 0..255
        shifted = [0] * COLOR_TABLE_SIZE
        for i in range(COLOR_TABLE_SIZE):
            src = i + 1
            if src <= upper:
                shifted[i] = int(get(src))
        return shifted
    return result


def collect_level_rows(design) -> list[tuple[int, str, str]]:
    rows: list[tuple[int, str, str]] = []
    try:
        levels = design.Levels
    except Exception:
        return rows

    for lvl in levels:
        try:
            name = str(lvl.Name or "")
        except Exception:
            continue
        if not name:
            continue
        try:
            description = str(getattr(lvl, "Description", None) or "")
        except Exception:
            description = ""
        try:
            color_index = int(lvl.ElementColor)
        except Exception:
            continue
        rows.append((color_index, name, description))
    return rows


def write_csv(
    path: Path,
    packed: list[int],
    level_rows: list[tuple[int, str, str]],
) -> None:
    by_index: dict[int, list[tuple[str, str]]] = defaultdict(list)
    extras: list[tuple[int, str, str]] = []
    for color_index, layer, description in level_rows:
        index = int(color_index)
        if 0 <= index < COLOR_TABLE_SIZE:
            by_index[index].append((layer, description))
        else:
            extras.append((index, layer, description))

    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(["ColorIndex", "RGB", "R", "G", "B", "Layer", "Description"])
        for index in range(COLOR_TABLE_SIZE):
            r, g, b = unpack_colorref(packed[index] if index < len(packed) else 0)
            assigned = by_index.get(index) or []
            if not assigned:
                writer.writerow([index, f"{r}, {g}, {b}", r, g, b, "", ""])
                continue
            for layer, description in assigned:
                writer.writerow([index, f"{r}, {g}, {b}", r, g, b, layer, description])
        for color_index, layer, description in extras:
            packed_value = int(color_index)
            r, g, b = unpack_colorref(packed_value)
            writer.writerow([-1, f"{r}, {g}, {b}", r, g, b, layer, description])


def main(argv: list[str]) -> int:
    try:
        import win32com.client  # type: ignore
    except ImportError as exc:
        raise SystemExit(
            "pywin32 is required on Windows (pip install pywin32). "
            "Prefer the ExportRgbColors add-in: mdl load ExportRgbColors, then RGBCSV DIALOG."
        ) from exc

    app = win32com.client.GetActiveObject("MicroStationDGN.Application")
    design = app.ActiveDesignFile
    table = design.ExtractColorTable()
    packed = packed_by_index(table.GetColors())
    level_rows = collect_level_rows(design)

    out = Path(argv[1]) if len(argv) > 1 else Path(design.FullName + "-all-color-codes.csv")
    write_csv(out, packed, level_rows)
    print(f"Wrote all {COLOR_TABLE_SIZE} color-table codes with RGB, Layer, and Description to {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
