using System;
using Bentley.DgnPlatformNET;

namespace ExportRgbColors
{
    /// <summary>
    /// Resolves a MicroStation color id (table index or internal color) to
    /// ColorIndex + RGB using DgnColorMap.
    /// </summary>
    internal static class ColorResolver
    {
        public static bool TryResolve(DgnFile dgnFile, uint colorId, out int colorIndex, out byte r, out byte g, out byte b, out string error)
        {
            colorIndex = (int)(colorId & 0xFF);
            r = g = b = 0;
            error = null;

            try
            {
                ColorInformation info = DgnColorMap.ExtractElementColorInfo(colorId, dgnFile);
                ColorDefinition def = info.ColorDefinition;
                r = Convert.ToByte(def.R);
                g = Convert.ToByte(def.G);
                b = Convert.ToByte(def.B);

                // Indexed colors are 0–255. Internal/true colors store the nearest
                // table index in the low byte (same as the Color dialog Index field).
                if (colorId <= 255)
                    colorIndex = (int)colorId;
                else
                    colorIndex = (int)(colorId & 0xFF);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static uint GetLevelColorId(LevelDefinitionColor levelColor)
        {
            // Constructor is LevelDefinitionColor(index, dgnFile); Color is the stored id.
            return unchecked((uint)levelColor.Color);
        }
    }
}
