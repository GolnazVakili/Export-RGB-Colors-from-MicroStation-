using System.Collections.Generic;
using System.Globalization;

namespace ExportRgbColors
{
    /// <summary>
    /// Builds CSV rows for every MicroStation color-table code (0–255), whether
    /// or not a layer uses that color. Levels that use a table color fill Layer
    /// and Description on that color’s row (one row per level if shared).
    /// True-color / non-table levels are appended after the table.
    /// </summary>
    public static class ColorTableCsvBuilder
    {
        public const int ColorTableSize = 256;

        public static List<ColorCsvRow> BuildAllColorCodes(
            IList<int> packedByIndex,
            IEnumerable<ColorCsvRow> levelRows = null)
        {
            var attached = new List<ColorCsvRow>[ColorTableSize];
            var extras = new List<ColorCsvRow>();

            if (levelRows != null)
            {
                foreach (ColorCsvRow row in levelRows)
                {
                    int index;
                    if (TryGetTableIndex(row, out index))
                    {
                        if (attached[index] == null)
                            attached[index] = new List<ColorCsvRow>();
                        attached[index].Add(row);
                    }
                    else
                    {
                        extras.Add(row);
                    }
                }
            }

            var rows = new List<ColorCsvRow>(ColorTableSize);
            int count = packedByIndex == null ? 0 : packedByIndex.Count;

            for (int i = 0; i < ColorTableSize; i++)
            {
                byte r = 0, g = 0, b = 0;
                if (i < count)
                    ColorRef.Unpack(packedByIndex[i], out r, out g, out b);

                List<ColorCsvRow> levels = attached[i];
                if (levels == null || levels.Count == 0)
                {
                    rows.Add(CsvUtil.TableRow(i, r, g, b));
                    continue;
                }

                foreach (ColorCsvRow level in levels)
                    rows.Add(CsvUtil.TableRow(i, r, g, b, level.Layer, level.Description));
            }

            foreach (ColorCsvRow extra in extras)
                rows.Add(extra);

            return rows;
        }

        private static bool TryGetTableIndex(ColorCsvRow row, out int index)
        {
            index = -1;
            if (row == null || string.IsNullOrEmpty(row.ColorIndex))
                return false;
            if (!int.TryParse(row.ColorIndex, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                return false;
            return index >= 0 && index < ColorTableSize;
        }
    }
}
