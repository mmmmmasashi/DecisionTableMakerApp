using DecisionTableLib.Decisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DecisionTableLibTest
{
    public class DecisionTableMakerTest
    {
        
        private void AssertTestCaseLevels(TestCase testCase, string expectedOS, string expectedLanguage)
        {
            Assert.Equal(expectedOS, testCase.LevelOf("OS"));
            Assert.Equal(expectedLanguage, testCase.LevelOf("Language"));
        }

        [Fact]
        public void ディシジョンテーブルをつくるテスト()
        {
            //サンプルの因子水準表を取得
            var 因子水準表サンプル = ExcelRangeTest.サンプルの因子水準表を作成();

            //サンプルの計算式
            string 計算式 = "[OS] * [Language]";

            var maker = new DecisionTableMaker(因子水準表サンプル);
            DecisionTable decisionTable = maker.CreateFrom(計算式);
            List<TestCase> testCases = decisionTable.TestCases;
            Assert.Equal(9, testCases.Count);

            //因子を指定すると水準がとれる
            AssertTestCaseLevels(testCases[0], "Windows", "Japanese");
            AssertTestCaseLevels(testCases[1], "Windows", "English");
            AssertTestCaseLevels(testCases[2], "Windows", "Chinese");

            AssertTestCaseLevels(testCases[3], "Mac", "Japanese");
            AssertTestCaseLevels(testCases[4], "Mac", "English");
            AssertTestCaseLevels(testCases[5], "Mac", "Chinese");

            AssertTestCaseLevels(testCases[6], "Linux", "Japanese");
            AssertTestCaseLevels(testCases[7], "Linux", "English");
            AssertTestCaseLevels(testCases[8], "Linux", "Chinese");
        }

    }
}
