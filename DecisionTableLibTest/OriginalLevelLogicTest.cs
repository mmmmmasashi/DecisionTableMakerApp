using DecisionTableLib.Decisions;
using DecisionTableLib.FormulaAnalyzer;
using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DecisionTableLibTest
{
    public class OriginalLevelLogicTest
    {
        private readonly ITestOutputHelper _output;

        public OriginalLevelLogicTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void 計算式で独自水準の定義ができる()
        {
            //var formulaText = "[OS] * [Language(Japanese,English)] * [Version]";

            //逆ポーランド記法になったトークン集
            List<string> rpn = new List<string>() { "OS", "Language(Japanese,English)", "*", "Version", "*" };

            var logic = new OriginalLevelLogic();
            var result = logic.OverwriteByOriginalLevels(rpn);

            //カッコは除去される
            var rpnNew = result.rpnNew;
            int i = 0;
            Assert.Equal("OS", rpnNew[i++]);
            Assert.Equal("Language", rpnNew[i++]);
            Assert.Equal("*", rpnNew[i++]);
            Assert.Equal("Version", rpnNew[i++]);
            Assert.Equal("*", rpnNew[i++]);

            var newFactorList = result.factors;
            var expectedNewFactor = new Factor("Language", new List<Level>() { new Level("Japanese"), new Level("English") });

            Assert.Single(newFactorList);
            Assert.Equal(expectedNewFactor, newFactorList[0]);
        }

        [Fact]
        public void 計算式で独自水準の定義ができる_2つ独自のケース()
        {
            //var formulaText = "[OS] * [Language(Japanese,English)] * [Version]";

            //逆ポーランド記法になったトークン集
            List<string> rpn = new List<string>() { "OS", "Language(Japanese,English)", "*", "Version(X,Y)", "*" };

            var logic = new OriginalLevelLogic();
            var result = logic.OverwriteByOriginalLevels(rpn);

            //カッコは除去される
            var rpnNew = result.rpnNew;
            int i = 0;
            Assert.Equal("OS", rpnNew[i++]);
            Assert.Equal("Language", rpnNew[i++]);
            Assert.Equal("*", rpnNew[i++]);
            Assert.Equal("Version", rpnNew[i++]);
            Assert.Equal("*", rpnNew[i++]);

            var newFactorList = result.factors;
            var expectedNewFactor1 = new Factor("Language", new List<Level>() { new Level("Japanese"), new Level("English") });
            var expectedNewFactor2 = new Factor("Version", new List<Level>() { new Level("X"), new Level("Y") });

            Assert.Equal(2, newFactorList.Count());
            Assert.Equal(expectedNewFactor1, newFactorList[0]);
            Assert.Equal(expectedNewFactor2, newFactorList[1]);
        }

        [Fact]
        public void カッコが左しか無ければエラー()
        {
            List<string> rpn = new List<string>() { "OS", "Language(Japanese,English", "*", "Version", "*" };
            var logic = new OriginalLevelLogic();
            Assert.Throws<InvalidDataException>(() => logic.OverwriteByOriginalLevels(rpn));
        }

        [Fact]
        public void カッコが右しか無ければエラー()
        {
            List<string> rpn = new List<string>() { "OS", "Language Japanese,English)", "*", "Version", "*" };
            var logic = new OriginalLevelLogic();
            Assert.Throws<InvalidDataException>(() => logic.OverwriteByOriginalLevels(rpn));
        }

    }
}
