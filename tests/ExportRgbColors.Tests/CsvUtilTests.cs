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
        public void Header_Is_ColorIndex_Rgb_Layer_And_Description()
        {
            Assert.Equal(
                new[] { "ColorIndex", "RGB", "R", "G", "B", "Layer", "Description" },
                CsvUtil.Header);
        }

        [Fact]
        public void ColorTableRow_Writes_ColorIndex_And_RGB_On_The_Same_Line()
        {
            string line = CsvUtil.FormatRow(CsvUtil.TableRow(3, 0, 0, 255));
            Assert.Equal("3,\"0, 0, 255\",0,0,255,,", line);
            Assert.StartsWith("3,", line);
            Assert.Contains("\"0, 0, 255\"", line);
        }

        [Fact]
        public void Assigned_Color_Writes_Layer_And_Description_On_The_Color_Code_Row()
        {
            string line = CsvUtil.FormatRow(CsvUtil.TableRow(1, 0, 0, 255, "EQPM", "Equipment"));
            Assert.Equal("1,\"0, 0, 255\",0,0,255,EQPM,Equipment", line);
        }

        [Fact]
        public void ByLevelRow_Uses_ColorIndex_Minus_One_With_Layer_And_Description()
        {
            string line = CsvUtil.FormatRow(CsvUtil.ByLevelRow("EQPM", 0, 0, 255, "Equipment"));
            Assert.Equal("-1,\"0, 0, 255\",0,0,255,EQPM,Equipment", line);
        }

        [Fact]
        public void Shared_Color_Writes_One_Row_Per_Level_With_The_Same_ColorIndex()
        {
            var rows = new[]
            {
                CsvUtil.TableRow(1, 0, 0, 255, "EQPM", "Equipment"),
                CsvUtil.TableRow(1, 0, 0, 255, "R-LITE", "Road lighting")
            };

            string[] lines = rows.Select(CsvUtil.FormatRow).ToArray();
            Assert.Equal("1,\"0, 0, 255\",0,0,255,EQPM,Equipment", lines[0]);
            Assert.Equal("1,\"0, 0, 255\",0,0,255,R-LITE,Road lighting", lines[1]);
        }

        [Fact]
        public void Unused_Color_Table_Row_Has_Blank_Layer_And_Description()
        {
            for (int i = 0; i <= 255; i++)
            {
                ColorCsvRow row = CsvUtil.TableRow(i, 1, 2, 3);
                Assert.Equal(i.ToString(), row.ColorIndex);
                Assert.Equal("1, 2, 3", row.RGB);
                Assert.Equal(string.Empty, row.Layer);
                Assert.Equal(string.Empty, row.Description);
                Assert.NotEqual("-1", row.ColorIndex);
            }
        }

        [Fact]
        public void Layer_And_Description_With_Comma_Are_Quoted()
        {
            string line = CsvUtil.FormatRow(
                CsvUtil.TableRow(4, 255, 0, 0, "R-LITE, EQPM", "Lights, equipment"));
            Assert.Equal("4,\"255, 0, 0\",255,0,0,\"R-LITE, EQPM\",\"Lights, equipment\"", line);
        }

        [Fact]
        public void Write_Produces_Bom_Utf8_Csv_With_Layer_And_Description()
        {
            string path = Path.Combine(Path.GetTempPath(), "export-rgb-colors-test.csv");
            try
            {
                var rows = new List<ColorCsvRow>
                {
                    CsvUtil.TableRow(0, 0, 0, 0),
                    CsvUtil.TableRow(1, 0, 0, 255, "Default", "Default level")
                };

                CsvUtil.Write(path, rows);

                byte[] bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length >= 3);
                Assert.Equal(0xEF, bytes[0]);
                Assert.Equal(0xBB, bytes[1]);
                Assert.Equal(0xBF, bytes[2]);

                string text = File.ReadAllText(path, new UTF8Encoding(true));
                string[] lines = text.Replace("\r\n", "\n").TrimEnd().Split('\n');
                Assert.Equal("ColorIndex,RGB,R,G,B,Layer,Description", lines[0]);
                Assert.Equal("0,\"0, 0, 0\",0,0,0,,", lines[1]);
                Assert.Equal("1,\"0, 0, 255\",0,0,255,Default,Default level", lines[2]);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
