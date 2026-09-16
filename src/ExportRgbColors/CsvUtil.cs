using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExportRgbColors
{
    /// <summary>
    /// One exported color row. ColorIndex and RGB are always written together
    /// (the Index dialog Color field plus the RGB field). Layer and Description
    /// come from each MicroStation level that uses that color.
    /// </summary>
    public sealed class ColorCsvRow
    {
        public string ColorIndex { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public string Layer { get; set; }
        public string Description { get; set; }

        public string RGB
        {
            get { return CsvUtil.FormatRgb(R, G, B); }
        }
    }

    /// <summary>
    /// CSV formatting for ColorIndex, RGB, R, G, B, Layer, Description. Kept
    /// free of Bentley types so it can be unit-tested without MicroStation.
    /// </summary>
    public static class CsvUtil
    {
        public const int ByLevelIndex = -1;

        public static readonly string[] Header =
        {
            "ColorIndex", "RGB", "R", "G", "B", "Layer", "Description"
        };

        public static string FormatRgb(int r, int g, int b)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", r, g, b);
        }

        public static ColorCsvRow TableRow(int colorIndex, int r, int g, int b)
        {
            return TableRow(colorIndex, r, g, b, string.Empty, string.Empty);
        }

        public static ColorCsvRow TableRow(int colorIndex, int r, int g, int b, string layer, string description)
        {
            return new ColorCsvRow
            {
                ColorIndex = colorIndex.ToString(CultureInfo.InvariantCulture),
                R = r,
                G = g,
                B = b,
                Layer = layer ?? string.Empty,
                Description = description ?? string.Empty
            };
        }

        public static ColorCsvRow ByLevelRow(string layer, int r, int g, int b)
        {
            return ByLevelRow(layer, r, g, b, string.Empty);
        }

        public static ColorCsvRow ByLevelRow(string layer, int r, int g, int b, string description)
        {
            return TableRow(ByLevelIndex, r, g, b, layer, description);
        }

        public static string FormatRow(ColorCsvRow row)
        {
            return string.Join(",",
                Escape(row.ColorIndex ?? string.Empty),
                Escape(row.RGB),
                row.R.ToString(CultureInfo.InvariantCulture),
                row.G.ToString(CultureInfo.InvariantCulture),
                row.B.ToString(CultureInfo.InvariantCulture),
                Escape(row.Layer ?? string.Empty),
                Escape(row.Description ?? string.Empty));
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
