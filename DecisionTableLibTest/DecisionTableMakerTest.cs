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

        private void AssertTestCase3FactorLevels(TestCase testCase, string expectedOS, string expectedLanguage, string expectedVersion)
        {
            Assert.Equal(expectedOS, testCase.LevelOf("OS"));
            Assert.Equal(expectedLanguage, testCase.LevelOf("Language"));
            Assert.Equal(expectedVersion, testCase.LevelOf("Version"));
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

        [Fact]
        public void 三因子の掛け合わせのディシジョンテーブルを作るテスト()
        {
            //サンプルの因子水準表を取得
            var 因子水準表サンプル = ExcelRangeTest.三因子水準表を作成();

            //サンプルの計算式
            string 計算式 = "[OS] * [Language] * [Version]";

            var maker = new DecisionTableMaker(因子水準表サンプル);
            DecisionTable decisionTable = maker.CreateFrom(計算式);
            List<TestCase> testCases = decisionTable.TestCases;
            Assert.Equal(9, testCases.Count);

            //因子を指定すると水準がとれる
            AssertTestCase3FactorLevels(testCases[0], "Windows", "Japanese", "1.0");
            //AssertTestCase3FactorLevels(testCases[1], "Windows", "English");
            //AssertTestCase3FactorLevels(testCases[2], "Windows", "Chinese");

            //AssertTestCase3FactorLevels(testCases[3], "Mac", "Japanese");
            //AssertTestCase3FactorLevels(testCases[4], "Mac", "English");
            //AssertTestCase3FactorLevels(testCases[5], "Mac", "Chinese");

            //AssertTestCase3FactorLevels(testCases[6], "Linux", "Japanese");
            //AssertTestCase3FactorLevels(testCases[7], "Linux", "English");
            //AssertTestCase3FactorLevels(testCases[8], "Linux", "Chinese");

            
        }
    }
}
