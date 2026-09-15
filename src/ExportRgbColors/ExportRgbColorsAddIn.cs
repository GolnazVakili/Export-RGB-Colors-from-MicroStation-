using System;
using Bentley.MstnPlatformNET;

namespace ExportRgbColors
{
    /// <summary>
    /// MicroStation CONNECT / 2023+ add-in that exports the color table and
    /// level ByLevel colors to CSV.
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
            return 0;
        }
    }
}
