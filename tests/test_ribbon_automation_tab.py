import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class RibbonInstallTests(unittest.TestCase):
    def test_ribbon_xml_adds_automation_tab_with_export_button(self):
        tree = ET.parse(ROOT / "ribbon" / "ExportRgbColorsRibbon.xml")
        root = tree.getroot()
        self.assertEqual(root.tag, "RibbonItems")

        collections = [el.get("Name") for el in root.findall("./TabCollections/TabCollection")]
        self.assertIn("Mstn.Drawing", collections)

        tab_names = [el.get("Name") for el in root.findall("./Tabs/Tab")]
        self.assertIn("ExportRgbColors.Automation", tab_names)

        labels = [el.text for el in root.findall("./Tabs/Tab/Label")]
        self.assertEqual(["Automation"], labels)

        group_labels = [el.text for el in root.findall("./Groups/Group/Label")]
        self.assertIn("RGB Colors", group_labels)

        button_labels = [el.text for el in root.findall("./Buttons/Button/Label")]
        self.assertIn("Export RGB Colors", button_labels)

        keyins = [el.text for el in root.findall(".//Keyin")]
        self.assertIn("[RgbCsvExport]RGBCSV DIALOG", keyins)

    def test_named_command_loads_addin_and_dialog(self):
        tree = ET.parse(ROOT / "ribbon" / "ExportRgbColorsNamedCommands.xml")
        keyin = tree.find("./UserNamedCommand/Keyin").text
        self.assertEqual("[RgbCsvExport]RGBCSV DIALOG", keyin)

    def test_cfg_autoloads_addin_and_ribbon_once(self):
        text = (ROOT / "config" / "ExportRgbColors.cfg").read_text(encoding="utf-8")
        self.assertEqual(1, text.count("MS_DGNAPPS > ExportRgbColors"))
        self.assertIn("MS_RIBBONXML", text)
        self.assertNotIn("# MS_DGNAPPS", text)


if __name__ == "__main__":
    unittest.main()
