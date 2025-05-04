using DecisionTableLib.FormulaAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DecisionTableLibTest.FormulaAnalyzerTest
{
    //逆ポーランド記法(後置記法？)に変換するテスト
    public class RPNTest
    {
        private readonly ITestOutputHelper _output;

        public RPNTest(ITestOutputHelper output)
        {
            _output = output;
        }

        private void AssertRPN(List<string> expected, List<string> inputToken)
        {
            var result = new RPN().ToRPN(inputToken);

            //全要素が一致すること
            Assert.Equal(expected.Count, result.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }

        [Fact]
        public void 並び替えテスト_2要素()
        {
            List<string> inputToken = new List<string>() { "OS", "*", "Language" };
            List<string> expected = new List<string>() { "OS", "Language", "*" };
            AssertRPN(expected, inputToken);
        }

        [Fact]
        public void 並び替えテスト_3要素()
        {
            List<string> inputToken = new List<string>() { "OS", "*", "Language", "*", "Version" };
            List<string> expected = new List<string>() { "OS", "Language", "*", "Version", "*" };
            AssertRPN(expected, inputToken);
        }


        [Fact]
        public void 並び替えテスト_3要素_足し算を含む()
        {
            List<string> inputToken = new List<string>() { "OS", "*", "Language", "+", "Version" };
            List<string> expected = new List<string>() { "OS", "Language", "*", "Version", "+" };

            AssertRPN(expected, inputToken);
        }


        [Fact]
        public void 並び替えテスト_3要素_足し算による順序並び替えを含む()
        {
            List<string> inputToken = new List<string>() { "OS", "+", "Language", "*", "Version" };
            
            //脳内モデルとしてはスタックと考えればよい
            List<string> expected = new List<string>() { "OS", "Language", "Version", "*", "+" };

            AssertRPN(expected, inputToken);
        }
    }
}
