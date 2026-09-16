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

    def test_csv_contains_all_256_codes_not_only_layer_colors(self):
        packed = [0] * 256
        packed[1] = 16711680  # blue
        packed[4] = 255  # red
        path = Path("/tmp/all-color-codes-script-test.csv")
        mod.write_csv(path, packed, level_rows=[("EQPM", 0, 0, 255)])

        with path.open(encoding="utf-8-sig", newline="") as handle:
            rows = list(csv.reader(handle))

        self.assertEqual(rows[0], ["ColorIndex", "R", "G", "B", "Layer"])
        self.assertEqual(len(rows), 258)
        self.assertEqual(rows[1], ["0", "0", "0", "0", ""])
        self.assertEqual(rows[2], ["1", "0", "0", "255", ""])
        self.assertEqual(rows[5], ["4", "255", "0", "0", ""])
        self.assertEqual(rows[256], ["255", "0", "0", "0", ""])
        self.assertEqual(rows[257], ["-1", "0", "0", "255", "EQPM"])


if __name__ == "__main__":
    unittest.main()
