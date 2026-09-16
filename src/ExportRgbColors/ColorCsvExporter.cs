using System;
using System.Collections.Generic;
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

        public int TotalRows
        {
            get { return ColorTableRows + LevelRows; }
        }
    }

    internal static class ColorCsvExporter
    {
        public const int ColorTableSize = ColorTableCsvBuilder.ColorTableSize;

        public static ExportResult Export(string csvPath, bool includeColorTable, bool includeLevels)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            if (dgnFile == null || dgnModel == null)
                throw new InvalidOperationException("No active DGN file. Open a design file and try again.");

            var rows = new List<ColorCsvRow>();
            int skipped = 0;
            int tableRows = 0;

            if (includeColorTable)
            {
                int before = rows.Count;
                skipped += AppendColorTable(dgnFile, rows);
                tableRows = rows.Count - before;
            }

            if (includeLevels)
                skipped += AppendLevelColors(dgnFile, dgnModel, rows);

            string directory = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            CsvUtil.Write(csvPath, rows);

            return new ExportResult
            {
                Path = csvPath,
                ColorTableRows = tableRows,
                LevelRows = rows.Count - tableRows,
                Skipped = skipped
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

        private static int AppendColorTable(DgnFile dgnFile, List<ColorCsvRow> rows)
        {
            int[] packed;
            bool haveTable = AttachedColorTable.TryReadPackedColors(out packed);

            var complete = new int[ColorTableSize];
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

            // Always emit indices 0–255 so unused table colors are included.
            rows.AddRange(ColorTableCsvBuilder.BuildAllColorCodes(complete));
            return skipped;
        }

        private static int AppendLevelColors(DgnFile dgnFile, DgnModel dgnModel, List<ColorCsvRow> rows)
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

                LevelDefinitionColor levelColor = handle.GetByLevelColor();
                uint colorId = ColorResolver.GetLevelColorId(levelColor);

                byte r, g, b;
                if (!ColorResolver.TryResolve(dgnFile, colorId, out _, out r, out g, out b, out _))
                {
                    skipped++;
                    continue;
                }

                rows.Add(CsvUtil.ByLevelRow(layerName, r, g, b));
            }

            return skipped;
        }
    }
}
