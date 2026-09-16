using System;
using System.Globalization;

namespace ExportRgbColors
{
    /// <summary>
    /// MicroStation VBA ColorTable.GetColors stores each entry as a Windows
    /// COLORREF: red in the low byte, then green, then blue.
    /// </summary>
    public static class ColorRef
    {
        public static void Unpack(int packed, out byte r, out byte g, out byte b)
        {
            unchecked
            {
                r = (byte)(packed & 0xFF);
                g = (byte)((packed >> 8) & 0xFF);
                b = (byte)((packed >> 16) & 0xFF);
            }
        }

        public static int Pack(int r, int g, int b)
        {
            return (r & 0xFF) | ((g & 0xFF) << 8) | ((b & 0xFF) << 16);
        }

        /// <summary>
        /// Copies a COM/VBA SAFEARRAY of packed colors into a 0-based array
        /// whose index is the MicroStation color code (0–255).
        /// A 1-based array of exactly minLength entries is treated as the
        /// 256 table slots and shifted to 0–255.
        /// </summary>
        public static int[] FromComArray(Array arr, int minLength)
        {
            if (arr == null || arr.Rank != 1 || arr.Length == 0)
                return new int[Math.Max(0, minLength)];

            int lower = arr.GetLowerBound(0);
            int upper = arr.GetUpperBound(0);

            if (lower == 1 && minLength > 0 && arr.Length == minLength)
            {
                var shifted = new int[minLength];
                for (int i = 0; i < minLength; i++)
                {
                    object value = arr.GetValue(i + lower);
                    shifted[i] = value == null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                return shifted;
            }

            int length = Math.Max(minLength, Math.Max(upper + 1, arr.Length));
            var result = new int[length];

            for (int i = lower; i <= upper; i++)
            {
                if (i < 0)
                    continue;

                object value = arr.GetValue(i);
                result[i] = value == null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }

            return result;
        }
    }
}
