using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExportRgbColors
{
    /// <summary>
    /// One exported color row. ColorIndex is always present (0–255 for the color
    /// table, −1 for ByLevel). Layer is blank unless the row belongs to a level.
    /// </summary>
    public sealed class ColorCsvRow
    {
        public string ColorIndex { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public string Layer { get; set; }
    }

    /// <summary>
    /// CSV formatting for ColorIndex, R, G, B, Layer. Kept free of Bentley types
    /// so it can be unit-tested without MicroStation.
    /// </summary>
    public static class CsvUtil
    {
        public const int ByLevelIndex = -1;

        public static readonly string[] Header = { "ColorIndex", "R", "G", "B", "Layer" };

        public static ColorCsvRow TableRow(int colorIndex, int r, int g, int b)
        {
            return new ColorCsvRow
            {
                ColorIndex = colorIndex.ToString(CultureInfo.InvariantCulture),
                R = r,
                G = g,
                B = b,
                Layer = string.Empty
            };
        }

        public static ColorCsvRow ByLevelRow(string layer, int r, int g, int b)
        {
            return new ColorCsvRow
            {
                ColorIndex = ByLevelIndex.ToString(CultureInfo.InvariantCulture),
                R = r,
                G = g,
                B = b,
                Layer = layer ?? string.Empty
            };
        }

        public static string FormatRow(ColorCsvRow row)
        {
            return string.Join(",",
                Escape(row.ColorIndex ?? string.Empty),
                row.R.ToString(CultureInfo.InvariantCulture),
                row.G.ToString(CultureInfo.InvariantCulture),
                row.B.ToString(CultureInfo.InvariantCulture),
                Escape(row.Layer ?? string.Empty));
        }

        public static void Write(string path, IEnumerable<ColorCsvRow> rows)
        {
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                writer.WriteLine(string.Join(",", Header));
                foreach (ColorCsvRow row in rows)
                    writer.WriteLine(FormatRow(row));
            }
        }

        public static string Escape(string value)
        {
            if (value == null)
                return string.Empty;

            bool needsQuotes = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
            if (!needsQuotes)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
