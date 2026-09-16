using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;

namespace ExportRgbColors
{
    internal sealed class ExportResult
    {
        public string Path { get; set; }
        public int ColorTableRows { get; set; }
        public int LevelRows { get; set; }
        public int Skipped { get; set; }
        public int TotalRows { get; set; }
    }

    internal sealed class ColorExportData
    {
        public List<ColorCsvRow> Rows { get; set; }
        public int ColorTableRows { get; set; }
        public int LevelRows { get; set; }
        public int Skipped { get; set; }
    }

    internal static class ColorCsvExporter
    {
        public const int ColorTableSize = ColorTableCsvBuilder.ColorTableSize;

        public static ColorExportData Collect(bool includeColorTable, bool includeLevels)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            if (dgnFile == null || dgnModel == null)
                throw new InvalidOperationException("No active DGN file. Open a design file and try again.");

            var levelRows = new List<ColorCsvRow>();
            int skipped = 0;

            if (includeLevels)
                skipped += CollectLevelRows(dgnFile, dgnModel, levelRows);

            var rows = new List<ColorCsvRow>();
            int tableRows = 0;

            if (includeColorTable)
            {
                int[] complete;
                skipped += ReadColorTable(dgnFile, out complete);
                rows.AddRange(ColorTableCsvBuilder.BuildAllColorCodes(complete, levelRows));
                tableRows = CountTableRows(rows);
            }
            else
            {
                rows.AddRange(levelRows);
            }

            return new ColorExportData
            {
                Rows = rows,
                ColorTableRows = tableRows,
                LevelRows = CountAssignedLayers(rows),
                Skipped = skipped
            };
        }

        public static ExportResult Export(string csvPath, bool includeColorTable, bool includeLevels)
        {
            ColorExportData data = Collect(includeColorTable, includeLevels);

            string directory = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            CsvUtil.Write(csvPath, data.Rows);

            return new ExportResult
            {
                Path = csvPath,
                ColorTableRows = data.ColorTableRows,
                LevelRows = data.LevelRows,
                Skipped = data.Skipped,
                TotalRows = data.Rows.Count
            };
        }

        public static string SuggestDefaultPath(DgnFile dgnFile)
        {
            string dgnPath = GetActiveDgnPath(dgnFile);
            string folder = Path.GetDirectoryName(dgnPath);
            string name = Path.GetFileNameWithoutExtension(dgnPath);
            if (string.IsNullOrEmpty(name))
                name = "colors";
            if (string.IsNullOrEmpty(folder) || folder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(folder, name + "-all-color-codes.csv");
        }

        public static string GetActiveDgnPath(DgnFile dgnFile)
        {
            try
            {
                string name = dgnFile.GetFileName();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            catch
            {
                // Fall through to COM.
            }

            try
            {
                return Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp.ActiveDesignFile.FullName;
            }
            catch
            {
                return "colors.csv";
            }
        }

        private static int ReadColorTable(DgnFile dgnFile, out int[] complete)
        {
            int[] packed;
            bool haveTable = AttachedColorTable.TryReadPackedColors(out packed);

            complete = new int[ColorTableSize];
            int skipped = 0;

            for (int i = 0; i < ColorTableSize; i++)
            {
                if (haveTable && packed != null && i < packed.Length)
                {
                    complete[i] = packed[i];
                    continue;
                }

                byte r, g, b;
                if (ColorResolver.TryResolve(dgnFile, (uint)i, out _, out r, out g, out b, out _))
                    complete[i] = ColorRef.Pack(r, g, b);
                else
                    skipped++;
            }

            return skipped;
        }

        private static int CollectLevelRows(DgnFile dgnFile, DgnModel dgnModel, List<ColorCsvRow> rows)
        {
            FileLevelCache cache = dgnModel.GetFileLevelCache();
            if (cache == null)
                return 0;

            LevelHandleCollection handles = cache.GetHandles();
            int skipped = 0;
            foreach (LevelHandle handle in handles)
            {
                if (handle == null || !handle.IsValid)
                    continue;

                string layerName = handle.Name ?? string.Empty;
                if (string.IsNullOrEmpty(layerName))
                    continue;

                string description = GetLevelDescription(handle, layerName);

                LevelDefinitionColor levelColor = handle.GetByLevelColor();
                uint colorId = ColorResolver.GetLevelColorId(levelColor);

                byte r, g, b;
                if (!ColorResolver.TryResolve(dgnFile, colorId, out _, out r, out g, out b, out _))
                {
                    skipped++;
                    continue;
                }

                if (colorId <= 255)
                    rows.Add(CsvUtil.TableRow((int)colorId, r, g, b, layerName, description));
                else
                    rows.Add(CsvUtil.ByLevelRow(layerName, r, g, b, description));
            }

            return skipped;
        }

        private static string GetLevelDescription(LevelHandle handle, string layerName)
        {
            string description = ReadStringProperty(handle, "Description");
            if (!string.IsNullOrEmpty(description))
                return description;
            return TryComLevelDescription(layerName);
        }

        private static string ReadStringProperty(object target, string name)
        {
            if (target == null)
                return string.Empty;

            try
            {
                var prop = target.GetType().GetProperty(name);
                if (prop == null)
                    return string.Empty;
                object value = prop.GetValue(target, null);
                return value == null
                    ? string.Empty
                    : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string TryComLevelDescription(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                return string.Empty;

            try
            {
                var app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                if (app == null || app.ActiveDesignFile == null)
                    return string.Empty;

                var lvl = app.ActiveDesignFile.Levels[layerName];
                if (lvl == null)
                    return string.Empty;

                string desc = lvl.Description;
                return desc ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int CountTableRows(IEnumerable<ColorCsvRow> rows)
        {
            int count = 0;
            foreach (ColorCsvRow row in rows)
            {
                int index;
                if (row != null
                    && int.TryParse(row.ColorIndex, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
                    && index >= 0
                    && index < ColorTableSize)
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountAssignedLayers(IEnumerable<ColorCsvRow> rows)
        {
            int count = 0;
            foreach (ColorCsvRow row in rows)
            {
                if (row != null && !string.IsNullOrEmpty(row.Layer))
                    count++;
            }
            return count;
        }
    }
}
