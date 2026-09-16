using System.Collections.Generic;

namespace ExportRgbColors
{
    /// <summary>
    /// Builds CSV rows for every MicroStation color-table code (0–255), whether
    /// or not a layer uses that color. Optional ByLevel rows are appended after
    /// the table; they never replace table codes.
    /// </summary>
    public static class ColorTableCsvBuilder
    {
        public const int ColorTableSize = 256;

        public static List<ColorCsvRow> BuildAllColorCodes(
            IList<int> packedByIndex,
            IEnumerable<ColorCsvRow> levelRows = null)
        {
            var rows = new List<ColorCsvRow>(ColorTableSize);
            int count = packedByIndex == null ? 0 : packedByIndex.Count;

            for (int i = 0; i < ColorTableSize; i++)
            {
                byte r = 0, g = 0, b = 0;
                if (i < count)
                    ColorRef.Unpack(packedByIndex[i], out r, out g, out b);
                rows.Add(CsvUtil.TableRow(i, r, g, b));
            }

            if (levelRows != null)
            {
                foreach (ColorCsvRow row in levelRows)
                    rows.Add(row);
            }

            return rows;
        }
    }
}
