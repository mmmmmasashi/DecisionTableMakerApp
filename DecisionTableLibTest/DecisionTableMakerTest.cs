using DecisionTableLib.Decisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DecisionTableLibTest
{
    public class DecisionTableMakerTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DecisionTableMaker maker;

        public DecisionTableMakerTest(ITestOutputHelper output)
        {
            _output = output;
            var 因子水準表サンプル = ExcelRangeTest.三因子水準表を作成();
            maker = new DecisionTableMaker(因子水準表サンプル);
        }

        private List<TestCase> CreateCasesHelper(string 計算式)
        {
            DecisionTable decisionTable = maker.CreateFrom(計算式);
            List<TestCase> testCases = decisionTable.TestCases;
            return testCases;
        }

        private void AssertTestCaseLevels(TestCase testCase, string expectedOS, string expectedLanguage)
        {
            Assert.Equal(new Level(expectedOS), testCase.LevelOf("OS"));
            Assert.Equal(new Level(expectedLanguage), testCase.LevelOf("Language"));
            Assert.Equal(Level.DontCare, testCase.LevelOf("Version"));

        }

        private void AssertTestCase3FactorLevels(TestCase testCase, string expectedOS, string expectedLanguage, string expectedVersion)
        {
            Assert.Equal(new Level(expectedOS), testCase.LevelOf("OS"));
            Assert.Equal(new Level(expectedLanguage), testCase.LevelOf("Language"));
            Assert.Equal(new Level(expectedVersion), testCase.LevelOf("Version"));
        }

        [Fact]
        public void 掛け算でディシジョンテーブルをつくるテスト()
        {
            //サンプルの計算式
            List<TestCase> testCases = CreateCasesHelper("[OS] * [Language]");
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
        public void 足し算でディシジョンテーブルをつくるテスト()
        {
            //サンプルの計算式
            List<TestCase> testCases = CreateCasesHelper("[OS] + [Language]");

            Assert.Equal(3, testCases.Count);

            //因子を指定すると水準がとれる
            int i = 0;
            AssertTestCaseLevels(testCases[i++], "Windows", "Japanese");
            AssertTestCaseLevels(testCases[i++], "Mac", "English");
            AssertTestCaseLevels(testCases[i++], "Linux", "Chinese");
        }

        [Fact]
        public void 三因子の掛け合わせのディシジョンテーブルを作るテスト()
        {
            //サンプルの計算式
            List<TestCase> testCases = CreateCasesHelper("[OS] * [Language] * [Version]");
            Assert.Equal(18, testCases.Count);

            foreach (var item in testCases)
            {
                _output.WriteLine(item.ToString());
            }
            //因子を指定すると水準がとれる
            int i = 0;
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Japanese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Japanese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "English", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "English", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Chinese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Chinese", "2.0");
            
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Japanese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Japanese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "English", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "English", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Chinese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Chinese", "2.0");

            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Japanese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Japanese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "English", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "English", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Chinese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Chinese", "2.0");
        }

        [Fact]
        public void 三因子の掛け合わせ_式の記載順に従ってケースの並びが変わる()
        {
            //サンプルの計算式
            List<TestCase> testCases = CreateCasesHelper("[Language] * [Version] * [OS]");
            Assert.Equal(18, testCases.Count);

            foreach (var item in testCases)
            {
                _output.WriteLine(item.ToString());
            }
            //因子を指定すると水準がとれる
            int i = 0;
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Japanese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Japanese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Japanese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Japanese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Japanese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Japanese", "2.0");

            AssertTestCase3FactorLevels(testCases[i++], "Windows", "English", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "English", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "English", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "English", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "English", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "English", "2.0");

            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Chinese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Chinese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Chinese", "1.0");
            AssertTestCase3FactorLevels(testCases[i++], "Windows", "Chinese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Mac", "Chinese", "2.0");
            AssertTestCase3FactorLevels(testCases[i++], "Linux", "Chinese", "2.0");
        }

    }
}
