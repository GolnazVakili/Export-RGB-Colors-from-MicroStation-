using System;
using Xunit;

namespace ExportRgbColors.Tests
{
    public sealed class ColorRefTests
    {
        [Theory]
        [InlineData(255, 255, 0, 0)]
        [InlineData(65280, 0, 255, 0)]
        [InlineData(16711680, 0, 0, 255)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(16777215, 255, 255, 255)]
        public void Unpack_Reads_Windows_ColorRef_Bytes(int packed, int r, int g, int b)
        {
            ColorRef.Unpack(packed, out byte red, out byte green, out byte blue);
            Assert.Equal(r, red);
            Assert.Equal(g, green);
            Assert.Equal(b, blue);
        }

        [Fact]
        public void Pack_RoundTrips_Rgb()
        {
            int packed = ColorRef.Pack(10, 20, 30);
            ColorRef.Unpack(packed, out byte r, out byte g, out byte b);
            Assert.Equal(10, r);
            Assert.Equal(20, g);
            Assert.Equal(30, b);
        }

        [Fact]
        public void FromComArray_Uses_Color_Index_As_Slot_For_Zero_Based_Table()
        {
            var source = new int[256];
            source[1] = ColorRef.Pack(0, 0, 255);
            source[4] = ColorRef.Pack(255, 0, 0);

            int[] packed = ColorRef.FromComArray(source, 256);

            Assert.Equal(256, packed.Length);
            Assert.Equal(source[1], packed[1]);
            Assert.Equal(source[4], packed[4]);
            Assert.Equal(0, packed[0]);
            Assert.Equal(0, packed[255]);
        }

        [Fact]
        public void FromComArray_Shifts_One_Based_256_Table_To_Color_Codes_0_To_255()
        {
            Array source = Array.CreateInstance(typeof(int), new[] { 256 }, new[] { 1 });
            source.SetValue(ColorRef.Pack(255, 0, 0), 1);
            source.SetValue(ColorRef.Pack(0, 255, 0), 256);

            int[] packed = ColorRef.FromComArray(source, 256);

            Assert.Equal(256, packed.Length);
            Assert.Equal(ColorRef.Pack(255, 0, 0), packed[0]);
            Assert.Equal(ColorRef.Pack(0, 255, 0), packed[255]);
        }
    }
}
