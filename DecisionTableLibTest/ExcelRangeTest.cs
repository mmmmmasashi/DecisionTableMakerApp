using DecisionTableLib.Excel;
using DecisionTableLib.Trees;

namespace DecisionTableLibTest
{
    public class ExcelRangeTest
    {
        [Fact]
        public void 因子水準階層を解釈するテスト()
        {
            TreeNode rootNode = サンプルの因子水準表を作成();
            Assert.Equal("root", rootNode.Name);

            var children1 = rootNode.Children;
            Assert.Equal(2, children1.Count);

            Assert.Equal("OS", children1[0].Name);
            Assert.Equal("Language", children1[1].Name);

            var children1_1 = children1[0].Children;
            Assert.Equal(3, children1_1.Count);
            Assert.Equal("Windows", children1_1[0].Name);
            Assert.Equal("Mac", children1_1[1].Name);
            Assert.Equal("Linux", children1_1[2].Name);

        }

        public static TreeNode サンプルの因子水準表を作成()
        {
            string sampleText = "OS\tWindows\r\n\tMac\r\n\tLinux\r\nLanguage\tJapanese\r\n\tEnglish\r\n\tChinese";

            //以下のような階層構造として認識したい
            //root
            // L OS
            //   L Windows  
            //   L Mac
            //   L Linux
            // L Language
            //   L Japanese
            //   L English
            //   L Chinese

            var range = new ExcelRange(sampleText);
            TreeNode rootNode = range.ToTree();
            return rootNode;
        }
    }
}