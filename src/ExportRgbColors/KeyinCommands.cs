using System;
using System.IO;
using System.Windows.Forms;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;

namespace ExportRgbColors
{
    /// <summary>
    /// Key-in handlers registered from Commands.xml.
    ///   RGBCSV EXPORT [path]  — ColorIndex, RGB, Layer, Description for all color codes
    ///   RGBCSV TABLE [path]   — same
    ///   RGBCSV DIALOG         — add-in form showing ColorIndex, RGB, Layer, Description
    /// </summary>
    public static class KeyinCommands
    {
        public static void Export(string unparsed)
        {
            // No path → add-in dialog so ColorIndex and RGB are shown together.
            if (string.IsNullOrEmpty(NormalizePath(unparsed)))
            {
                Dialog(unparsed);
                return;
            }

            ExportToPath(unparsed, includeColorTable: true, includeLevels: true);
        }

        public static void ExportTable(string unparsed)
        {
            ExportToPath(unparsed, includeColorTable: true, includeLevels: true);
        }

        private static void ExportToPath(string unparsed, bool includeColorTable, bool includeLevels)
        {
            try
            {
                DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
                if (dgnFile == null)
                {
                    ShowError("No active DGN file", "Open a design file and try again.");
                    return;
                }

                string path = NormalizePath(unparsed);
                if (string.IsNullOrEmpty(path))
                    path = PromptForCsvPath(ColorCsvExporter.SuggestDefaultPath(dgnFile));

                if (string.IsNullOrEmpty(path))
                    return;

                RunExport(path, includeColorTable, includeLevels);
            }
            catch (Exception ex)
            {
                ShowError("Color CSV export failed", ex.ToString());
            }
        }

        public static void Dialog(string unparsed)
        {
            try
            {
                DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
                if (dgnFile == null)
                {
                    ShowError("No active DGN file", "Open a design file and try again.");
                    return;
                }

                using (var form = new ExportForm(ColorCsvExporter.SuggestDefaultPath(dgnFile)))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                        return;

                    RunExport(form.CsvPath, form.IncludeColorTable, form.IncludeLevels);
                }
            }
            catch (Exception ex)
            {
                ShowError("Color CSV export failed", ex.ToString());
            }
        }

        internal static void RunExport(string path, bool includeColorTable, bool includeLevels)
        {
            if (!includeColorTable && !includeLevels)
            {
                ShowError("Nothing to export", "Select the color table and/or level colors.");
                return;
            }

            ExportResult result = ColorCsvExporter.Export(path, includeColorTable, includeLevels);
            string summary = string.Format(
                "Wrote {0} rows with ColorIndex, RGB, Layer, and Description ({1} color-table rows, {2} with a layer) to {3}",
                result.TotalRows,
                result.ColorTableRows,
                result.LevelRows,
                result.Path);
            if (result.Skipped > 0)
                summary += string.Format(" ({0} unresolved colors skipped or written as 0,0,0)", result.Skipped);
            ShowInfo("Exported colors to CSV", summary);
        }

        private static string PromptForCsvPath(string suggested)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export MicroStation colors";
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = Path.GetFileName(suggested);
                dialog.InitialDirectory = Path.GetDirectoryName(suggested);
                dialog.OverwritePrompt = true;
                dialog.AddExtension = true;
                dialog.DefaultExt = "csv";
                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private static string NormalizePath(string unparsed)
        {
            if (string.IsNullOrWhiteSpace(unparsed))
                return null;

            string path = unparsed.Trim();
            if (path.Length >= 2 &&
                ((path[0] == '"' && path[path.Length - 1] == '"') ||
                 (path[0] == '\'' && path[path.Length - 1] == '\'')))
            {
                path = path.Substring(1, path.Length - 2);
            }

            return path.Trim();
        }

        private static void ShowInfo(string brief, string details)
        {
            MessageCenter.Instance.ShowInfoMessage(brief, details, false);
        }

        private static void ShowError(string brief, string details)
        {
            MessageCenter.Instance.ShowErrorMessage(brief, details, false);
        }
    }
}
