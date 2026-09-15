using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ExportRgbColors.Tests
{
    public sealed class CsvUtilTests
    {
        [Fact]
        public void Header_Is_ColorIndex_RGB_Layer()
        {
            Assert.Equal(new[] { "ColorIndex", "R", "G", "B", "Layer" }, CsvUtil.Header);
        }

        [Fact]
        public void ColorTableRow_Translates_Index_To_Rgb_And_Leaves_Layer_Blank()
        {
            string line = CsvUtil.FormatRow(CsvUtil.TableRow(3, 0, 0, 255));
            Assert.Equal("3,0,0,255,", line);
        }

        [Fact]
        public void ByLevelRow_Uses_ColorIndex_Minus_One_And_Layer_Name()
        {
            string line = CsvUtil.FormatRow(CsvUtil.ByLevelRow("EQPM", 0, 0, 255));
            Assert.Equal("-1,0,0,255,EQPM", line);
        }

        [Fact]
        public void Shared_Color_Writes_One_ByLevel_Row_Per_Level()
        {
            var rows = new[]
            {
                CsvUtil.TableRow(1, 0, 0, 255),
                CsvUtil.ByLevelRow("EQPM", 0, 0, 255),
                CsvUtil.ByLevelRow("R-LITE", 0, 0, 255)
            };

            string[] lines = rows.Select(CsvUtil.FormatRow).ToArray();
            Assert.Equal("1,0,0,255,", lines[0]);
            Assert.Equal("-1,0,0,255,EQPM", lines[1]);
            Assert.Equal("-1,0,0,255,R-LITE", lines[2]);
        }

        [Fact]
        public void Color_Table_Does_Not_Emit_Minus_One_Index()
        {
            for (int i = 0; i <= 255; i++)
            {
                ColorCsvRow row = CsvUtil.TableRow(i, 1, 2, 3);
                Assert.Equal(i.ToString(), row.ColorIndex);
                Assert.Equal(string.Empty, row.Layer);
                Assert.NotEqual("-1", row.ColorIndex);
            }
        }

        [Fact]
        public void Layer_With_Comma_Is_Quoted()
        {
            string line = CsvUtil.FormatRow(CsvUtil.ByLevelRow("R-LITE, EQPM", 255, 0, 0));
            Assert.Equal("-1,255,0,0,\"R-LITE, EQPM\"", line);
        }

        [Fact]
        public void Write_Produces_Bom_Utf8_Csv()
        {
            string path = Path.Combine(Path.GetTempPath(), "export-rgb-colors-test.csv");
            try
            {
                var rows = new List<ColorCsvRow>
                {
                    CsvUtil.TableRow(0, 0, 0, 0),
                    CsvUtil.ByLevelRow("Default", 0, 0, 255)
                };

                CsvUtil.Write(path, rows);

                byte[] bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length >= 3);
                Assert.Equal(0xEF, bytes[0]);
                Assert.Equal(0xBB, bytes[1]);
                Assert.Equal(0xBF, bytes[2]);

                string text = File.ReadAllText(path, new UTF8Encoding(true));
                string[] lines = text.Replace("\r\n", "\n").TrimEnd().Split('\n');
                Assert.Equal("ColorIndex,R,G,B,Layer", lines[0]);
                Assert.Equal("0,0,0,0,", lines[1]);
                Assert.Equal("-1,0,0,255,Default", lines[2]);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
