using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ExportRgbColors.Tests
{
    public sealed class ColorTableCsvBuilderTests
    {
        [Fact]
        public void BuildAllColorCodes_Writes_Every_Index_Even_When_Only_Layers_Use_Two_Colors()
        {
            var packed = new int[256];
            packed[1] = ColorRef.Pack(0, 0, 255);
            packed[4] = ColorRef.Pack(255, 0, 0);

            var levels = new[]
            {
                CsvUtil.ByLevelRow("EQPM", 0, 0, 255),
                CsvUtil.ByLevelRow("R-LITE", 0, 0, 255)
            };

            List<ColorCsvRow> rows = ColorTableCsvBuilder.BuildAllColorCodes(packed, levels);

            Assert.Equal(258, rows.Count);

            ColorCsvRow[] table = rows.Take(256).ToArray();
            for (int i = 0; i < 256; i++)
            {
                Assert.Equal(i.ToString(), table[i].ColorIndex);
                Assert.Equal(CsvUtil.FormatRgb(table[i].R, table[i].G, table[i].B), table[i].RGB);
                Assert.Equal(string.Empty, table[i].Layer);
            }

            Assert.Equal(0, table[1].R);
            Assert.Equal(0, table[1].G);
            Assert.Equal(255, table[1].B);
            Assert.Equal(255, table[4].R);
            Assert.Equal(0, table[0].R);
            Assert.Equal(0, table[0].G);
            Assert.Equal(0, table[0].B);
            Assert.Equal(0, table[255].R);

            Assert.Equal("-1", rows[256].ColorIndex);
            Assert.Equal("EQPM", rows[256].Layer);
            Assert.Equal("-1", rows[257].ColorIndex);
            Assert.Equal("R-LITE", rows[257].Layer);
        }

        [Fact]
        public void BuildAllColorCodes_Does_Not_Drop_Unused_Table_Slots()
        {
            var packed = new int[5];
            packed[3] = ColorRef.Pack(12, 34, 56);

            List<ColorCsvRow> rows = ColorTableCsvBuilder.BuildAllColorCodes(packed);

            Assert.Equal(256, rows.Count);
            Assert.DoesNotContain(rows, row => row.ColorIndex == "-1");
            Assert.Equal("3,\"12, 34, 56\",12,34,56,", CsvUtil.FormatRow(rows[3]));
            Assert.Equal("255,\"0, 0, 0\",0,0,0,", CsvUtil.FormatRow(rows[255]));
        }

        [Fact]
        public void Write_Full_Table_Has_Header_And_256_Color_Codes()
        {
            var packed = new int[256];
            packed[0] = ColorRef.Pack(0, 0, 0);
            packed[1] = ColorRef.Pack(0, 0, 255);
            packed[2] = ColorRef.Pack(0, 255, 0);

            List<ColorCsvRow> rows = ColorTableCsvBuilder.BuildAllColorCodes(packed);
            string path = Path.Combine(Path.GetTempPath(), "all-color-codes-test.csv");
            try
            {
                CsvUtil.Write(path, rows);
                string[] lines = File.ReadAllText(path, new UTF8Encoding(true))
                    .Replace("\r\n", "\n")
                    .TrimEnd()
                    .Split('\n');

                Assert.Equal(257, lines.Length);
                Assert.Equal("ColorIndex,RGB,R,G,B,Layer", lines[0]);
                Assert.Equal("0,\"0, 0, 0\",0,0,0,", lines[1]);
                Assert.Equal("1,\"0, 0, 255\",0,0,255,", lines[2]);
                Assert.Equal("2,\"0, 255, 0\",0,255,0,", lines[3]);
                Assert.Equal("255,\"0, 0, 0\",0,0,0,", lines[256]);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
