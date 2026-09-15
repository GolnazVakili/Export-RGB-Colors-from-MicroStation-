using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExportRgbColors
{
    /// <summary>
    /// One exported color row. Layer is blank for color-table entries and filled
    /// for ByLevel / level colors.
    /// </summary>
    public sealed class ColorCsvRow
    {
        public string ColorIndex { get; set; }
        public string Layer { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    /// <summary>
    /// CSV formatting for ColorIndex, Layer, R, G, B. Kept free of Bentley types
    /// so it can be unit-tested without MicroStation.
    /// </summary>
    public static class CsvUtil
    {
        public static readonly string[] Header = { "ColorIndex", "Layer", "R", "G", "B" };

        public static string FormatRow(ColorCsvRow row)
        {
            return string.Join(",",
                Escape(row.ColorIndex ?? string.Empty),
                Escape(row.Layer ?? string.Empty),
                row.R.ToString(CultureInfo.InvariantCulture),
                row.G.ToString(CultureInfo.InvariantCulture),
                row.B.ToString(CultureInfo.InvariantCulture));
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
