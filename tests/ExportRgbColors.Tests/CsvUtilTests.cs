using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ExportRgbColors.Tests
{
    public sealed class CsvUtilTests
    {
        [Fact]
        public void Header_Is_ColorIndex_Layer_RGB()
        {
            Assert.Equal(new[] { "ColorIndex", "Layer", "R", "G", "B" }, CsvUtil.Header);
        }

        [Fact]
        public void ColorTableRow_Leaves_Layer_Blank()
        {
            string line = CsvUtil.FormatRow(new ColorCsvRow
            {
                ColorIndex = "3",
                Layer = "",
                R = 0,
                G = 0,
                B = 255
            });

            Assert.Equal("3,,0,0,255", line);
        }

        [Fact]
        public void ByLevelRow_Includes_Layer_Name()
        {
            string line = CsvUtil.FormatRow(new ColorCsvRow
            {
                ColorIndex = "4",
                Layer = "EQPM",
                R = 0,
                G = 0,
                B = 255
            });

            Assert.Equal("4,EQPM,0,0,255", line);
        }

        [Fact]
        public void Layer_With_Comma_Is_Quoted()
        {
            string line = CsvUtil.FormatRow(new ColorCsvRow
            {
                ColorIndex = "1",
                Layer = "R-LITE, EQPM",
                R = 255,
                G = 0,
                B = 0
            });

            Assert.Equal("1,\"R-LITE, EQPM\",255,0,0", line);
        }

        [Fact]
        public void Write_Produces_Bom_Utf8_Csv()
        {
            string path = Path.Combine(Path.GetTempPath(), "export-rgb-colors-test.csv");
            try
            {
                var rows = new List<ColorCsvRow>
                {
                    new ColorCsvRow { ColorIndex = "0", Layer = "", R = 0, G = 0, B = 0 },
                    new ColorCsvRow { ColorIndex = "1", Layer = "Default", R = 0, G = 0, B = 255 }
                };

                CsvUtil.Write(path, rows);

                byte[] bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length >= 3);
                Assert.Equal(0xEF, bytes[0]);
                Assert.Equal(0xBB, bytes[1]);
                Assert.Equal(0xBF, bytes[2]);

                string text = File.ReadAllText(path, new UTF8Encoding(true));
                string[] lines = text.Replace("\r\n", "\n").TrimEnd().Split('\n');
                Assert.Equal("ColorIndex,Layer,R,G,B", lines[0]);
                Assert.Equal("0,,0,0,0", lines[1]);
                Assert.Equal("1,Default,0,0,255", lines[2]);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
