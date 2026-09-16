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
        public void BuildAllColorCodes_Fills_Layer_And_Description_On_Used_Color_Codes()
        {
            var packed = new int[256];
            packed[1] = ColorRef.Pack(0, 0, 255);
            packed[4] = ColorRef.Pack(255, 0, 0);

            var levels = new[]
            {
                CsvUtil.TableRow(1, 0, 0, 255, "EQPM", "Equipment"),
                CsvUtil.TableRow(1, 0, 0, 255, "R-LITE", "Road lighting")
            };

            List<ColorCsvRow> rows = ColorTableCsvBuilder.BuildAllColorCodes(packed, levels);

            // Color 1 is repeated once per level; unused colors stay as a single blank-layer row.
            Assert.Equal(257, rows.Count);

            ColorCsvRow unused = rows.First(row => row.ColorIndex == "0");
            Assert.Equal(string.Empty, unused.Layer);
            Assert.Equal(string.Empty, unused.Description);
            Assert.Equal("0,\"0, 0, 0\",0,0,0,,", CsvUtil.FormatRow(unused));

            ColorCsvRow[] colorOne = rows.Where(row => row.ColorIndex == "1").ToArray();
            Assert.Equal(2, colorOne.Length);
            Assert.Equal("EQPM", colorOne[0].Layer);
            Assert.Equal("Equipment", colorOne[0].Description);
            Assert.Equal(0, colorOne[0].R);
            Assert.Equal(0, colorOne[0].G);
            Assert.Equal(255, colorOne[0].B);
            Assert.Equal("R-LITE", colorOne[1].Layer);
            Assert.Equal("Road lighting", colorOne[1].Description);
            Assert.Equal("1,\"0, 0, 255\",0,0,255,EQPM,Equipment", CsvUtil.FormatRow(colorOne[0]));
            Assert.Equal("1,\"0, 0, 255\",0,0,255,R-LITE,Road lighting", CsvUtil.FormatRow(colorOne[1]));

            ColorCsvRow colorFour = rows.Single(row => row.ColorIndex == "4");
            Assert.Equal(255, colorFour.R);
            Assert.Equal(string.Empty, colorFour.Layer);
            Assert.Equal(string.Empty, colorFour.Description);

            Assert.DoesNotContain(rows, row => row.ColorIndex == "-1");
            Assert.Equal("255", rows[256].ColorIndex);
        }

        [Fact]
        public void BuildAllColorCodes_Does_Not_Drop_Unused_Table_Slots()
        {
            var packed = new int[5];
            packed[3] = ColorRef.Pack(12, 34, 56);

            List<ColorCsvRow> rows = ColorTableCsvBuilder.BuildAllColorCodes(packed);

            Assert.Equal(256, rows.Count);
            Assert.DoesNotContain(rows, row => row.ColorIndex == "-1");
            Assert.Equal("3,\"12, 34, 56\",12,34,56,,", CsvUtil.FormatRow(rows[3]));
            Assert.Equal("255,\"0, 0, 0\",0,0,0,,", CsvUtil.FormatRow(rows[255]));
            Assert.Equal(string.Empty, rows[3].Layer);
            Assert.Equal(string.Empty, rows[3].Description);
        }

        [Fact]
        public void True_Color_Levels_Are_Appended_After_The_Table()
        {
            var packed = new int[256];
            packed[2] = ColorRef.Pack(0, 255, 0);
            var extras = new[]
            {
                CsvUtil.ByLevelRow("TRUE-COLOR", 10, 20, 30, "Not a table index")
            };

            List<ColorCsvRow> rows = ColorTableCsvBuilder.BuildAllColorCodes(packed, extras);

            Assert.Equal(257, rows.Count);
            Assert.Equal("2,\"0, 255, 0\",0,255,0,,", CsvUtil.FormatRow(rows[2]));
            Assert.Equal("-1,\"10, 20, 30\",10,20,30,TRUE-COLOR,Not a table index", CsvUtil.FormatRow(rows[256]));
        }

        [Fact]
        public void Write_Full_Table_Has_Header_Layer_Description_And_256_Color_Codes()
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
                Assert.Equal("ColorIndex,RGB,R,G,B,Layer,Description", lines[0]);
                Assert.Equal("0,\"0, 0, 0\",0,0,0,,", lines[1]);
                Assert.Equal("1,\"0, 0, 255\",0,0,255,,", lines[2]);
                Assert.Equal("2,\"0, 255, 0\",0,255,0,,", lines[3]);
                Assert.Equal("255,\"0, 0, 0\",0,0,0,,", lines[256]);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
