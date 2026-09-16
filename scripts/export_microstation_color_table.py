"""Export every MicroStation color-table code (0-255) with RGB to CSV.

Requires a running MicroStation session with a DGN open. Uses COM
ExtractColorTable.GetColors so unused table slots are included, not only
colors assigned to a layer.

    python export_microstation_color_table.py [output.csv]
"""
from __future__ import annotations

import csv
import sys
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


def write_csv(path: Path, packed: list[int], level_rows: list[tuple[str, int, int, int]]) -> None:
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(["ColorIndex", "R", "G", "B", "Layer"])
        for index in range(COLOR_TABLE_SIZE):
            r, g, b = unpack_colorref(packed[index] if index < len(packed) else 0)
            writer.writerow([index, r, g, b, ""])
        for name, r, g, b in level_rows:
            writer.writerow([-1, r, g, b, name])


def main(argv: list[str]) -> int:
    try:
        import win32com.client  # type: ignore
    except ImportError as exc:
        raise SystemExit(
            "pywin32 is required on Windows (pip install pywin32). "
            "Or use vba/ExportAllColorCodes.bas inside MicroStation."
        ) from exc

    app = win32com.client.GetActiveObject("MicroStationDGN.Application")
    design = app.ActiveDesignFile
    table = design.ExtractColorTable()
    packed = packed_by_index(table.GetColors())

    out = Path(argv[1]) if len(argv) > 1 else Path(design.FullName + "-all-color-codes.csv")
    write_csv(out, packed, level_rows=[])
    print(f"Wrote all {COLOR_TABLE_SIZE} color-table codes with RGB to {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
