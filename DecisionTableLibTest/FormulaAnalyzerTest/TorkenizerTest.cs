using DecisionTableLib.FormulaAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DecisionTableLibTest.FormulaAnalyzerTest
{
    public class TorkenizerTest
    {
        [Fact]
        public void 二つの式の掛け算の例()
        {
            string formula = "[OS] * [Language]";

            var result = new Tokenizer().Tokenize(formula);

            List<string> expected = new List<string>() { "OS", "*", "Language" };

            //全要素が一致すること
            Assert.Equal(expected.Count, result.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }

        [Fact]
        public void 三つの式の掛け算の例()
        {
            string formula = "[OS] * [Language] * [Version]";

            var result = new Tokenizer().Tokenize(formula);

            List<string> expected = new List<string>() { "OS", "*", "Language", "*", "Version"};

            //全要素が一致すること
            Assert.Equal(expected.Count, result.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }
    }
}
