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
        public const int ColorTableSize = 256;

        public static ExportResult Export(string csvPath, bool includeColorTable, bool includeLevels)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            if (dgnFile == null || dgnModel == null)
                throw new InvalidOperationException("No active DGN file. Open a design file and try again.");

            var rows = new List<ColorCsvRow>();
            int skipped = 0;

            if (includeColorTable)
                skipped += AppendColorTable(dgnFile, rows);

            if (includeLevels)
                skipped += AppendLevelColors(dgnFile, dgnModel, rows);

            string directory = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            CsvUtil.Write(csvPath, rows);

            return new ExportResult
            {
                Path = csvPath,
                ColorTableRows = includeColorTable ? ColorTableSize : 0,
                LevelRows = rows.Count - (includeColorTable ? ColorTableSize : 0),
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
            return Path.Combine(folder, name + "-colors.csv");
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
            int skipped = 0;
            for (uint i = 0; i < ColorTableSize; i++)
            {
                int colorIndex;
                byte r, g, b;
                if (!ColorResolver.TryResolve(dgnFile, i, out colorIndex, out r, out g, out b, out _))
                {
                    skipped++;
                    rows.Add(new ColorCsvRow
                    {
                        ColorIndex = i.ToString(),
                        Layer = string.Empty,
                        R = 0,
                        G = 0,
                        B = 0
                    });
                    continue;
                }

                rows.Add(new ColorCsvRow
                {
                    ColorIndex = colorIndex.ToString(),
                    Layer = string.Empty,
                    R = r,
                    G = g,
                    B = b
                });
            }

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
                LevelDefinitionColor levelColor = handle.GetByLevelColor();
                uint colorId = ColorResolver.GetLevelColorId(levelColor);

                int colorIndex;
                byte r, g, b;
                if (!ColorResolver.TryResolve(dgnFile, colorId, out colorIndex, out r, out g, out b, out _))
                {
                    skipped++;
                    continue;
                }

                rows.Add(new ColorCsvRow
                {
                    ColorIndex = colorIndex.ToString(),
                    Layer = layerName,
                    R = r,
                    G = g,
                    B = b
                });
            }

            return skipped;
        }
    }
}
