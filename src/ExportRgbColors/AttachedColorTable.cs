using System;
using System.Reflection;

namespace ExportRgbColors
{
    /// <summary>
    /// Reads every entry of the attached DGN color table via COM
    /// ActiveDesignFile.ExtractColorTable / GetColors — the documented way to
    /// obtain all color codes, not only those assigned to a layer.
    /// </summary>
    internal static class AttachedColorTable
    {
        public static bool TryReadPackedColors(out int[] packedByIndex)
        {
            packedByIndex = null;
            try
            {
                object comApp = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                if (comApp == null)
                    return false;

                object designFile = Invoke(comApp, "ActiveDesignFile");
                if (designFile == null)
                    return false;

                object table = Invoke(designFile, "ExtractColorTable");
                if (table == null)
                    return false;

                object raw = Invoke(table, "GetColors");
                var arr = raw as Array;
                if (arr == null || arr.Length == 0)
                    return false;

                packedByIndex = ColorRef.FromComArray(arr, ColorTableCsvBuilder.ColorTableSize);
                return packedByIndex != null && packedByIndex.Length > 0;
            }
            catch
            {
                packedByIndex = null;
                return false;
            }
        }

        private static object Invoke(object target, string name)
        {
            return target.GetType().InvokeMember(
                name,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.InvokeMethod,
                null,
                target,
                null);
        }
    }
}
