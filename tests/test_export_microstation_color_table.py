import csv
import importlib.util
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "export_microstation_color_table.py"

spec = importlib.util.spec_from_file_location("export_microstation_color_table", SCRIPT)
mod = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = mod
spec.loader.exec_module(mod)


class ExportColorTableScriptTests(unittest.TestCase):
    def test_unpack_colorref(self):
        self.assertEqual(mod.unpack_colorref(255), (255, 0, 0))
        self.assertEqual(mod.unpack_colorref(65280), (0, 255, 0))
        self.assertEqual(mod.unpack_colorref(16711680), (0, 0, 255))

    def test_csv_contains_all_256_codes_with_layer_and_description(self):
        packed = [0] * 256
        packed[1] = 16711680  # blue
        packed[4] = 255  # red
        path = Path("/tmp/all-color-codes-script-test.csv")
        mod.write_csv(
            path,
            packed,
            level_rows=[
                (1, "EQPM", "Equipment"),
                (1, "R-LITE", "Road lighting"),
            ],
        )

        with path.open(encoding="utf-8-sig", newline="") as handle:
            rows = list(csv.reader(handle))

        self.assertEqual(rows[0], ["ColorIndex", "RGB", "R", "G", "B", "Layer", "Description"])
        # Color 1 is duplicated once per level; unused colors stay listed.
        self.assertEqual(len(rows), 258)
        self.assertEqual(rows[1], ["0", "0, 0, 0", "0", "0", "0", "", ""])
        self.assertEqual(rows[2], ["1", "0, 0, 255", "0", "0", "255", "EQPM", "Equipment"])
        self.assertEqual(rows[3], ["1", "0, 0, 255", "0", "0", "255", "R-LITE", "Road lighting"])
        self.assertEqual(rows[6], ["4", "255, 0, 0", "255", "0", "0", "", ""])
        self.assertEqual(rows[257], ["255", "0, 0, 0", "0", "0", "0", "", ""])

    def test_unused_color_keeps_blank_layer_and_description(self):
        packed = [0] * 256
        path = Path("/tmp/all-color-codes-unused-test.csv")
        mod.write_csv(path, packed, level_rows=[])

        with path.open(encoding="utf-8-sig", newline="") as handle:
            rows = list(csv.reader(handle))

        self.assertEqual(len(rows), 257)
        self.assertEqual(rows[1], ["0", "0, 0, 0", "0", "0", "0", "", ""])
        self.assertEqual(rows[256], ["255", "0, 0, 0", "0", "0", "0", "", ""])


if __name__ == "__main__":
    unittest.main()
