using System;
using Bentley.MstnPlatformNET;

namespace ExportRgbColors
{
    /// <summary>
    /// MicroStation CONNECT / 2023+ add-in that exports ColorIndex and RGB
    /// together for every color-table code (0–255).
    /// MdlTaskID must stay at or under 15 characters.
    /// </summary>
    [AddIn(MdlTaskID = "RgbCsvExport")]
    internal sealed class ExportRgbColorsAddIn : AddIn
    {
        internal static ExportRgbColorsAddIn Instance { get; private set; }

        private ExportRgbColorsAddIn(IntPtr mdlDesc)
            : base(mdlDesc)
        {
            Instance = this;
        }

        protected override int Run(string[] commandLine)
        {
            try
            {
                MessageCenter.Instance.ShowInfoMessage(
                    "Export RGB Colors add-in loaded",
                    "Key-in RGBCSV DIALOG to export ColorIndex and RGB together.",
                    false);
            }
            catch
            {
                // Message Center is unavailable during some load paths.
            }

            return 0;
        }
    }
}
