using DecisionTableLib.Excel;
using DecisionTableLib.Trees;

namespace DecisionTableLibTest
{
    public class ExcelRangeTest
    {
        [Fact]
        public void ���q�����K�w�����߂���e�X�g()
        {
            TreeNode rootNode = �T���v���̈��q�����\���쐬();
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

        public static TreeNode �T���v���̈��q�����\���쐬()
        {
            string sampleText = "OS\tWindows\r\n\tMac\r\n\tLinux\r\nLanguage\tJapanese\r\n\tEnglish\r\n\tChinese";

            //�ȉ��̂悤�ȊK�w�\���Ƃ��ĔF��������
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