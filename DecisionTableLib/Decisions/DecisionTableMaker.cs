using DecisionTableLib.FormulaAnalyzer;
using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class DecisionTableMaker
    {
        private readonly FactorLevelTable _factorLevelTable;
        private readonly bool _evenPlusMode;
        private readonly RPN _rpn;
        private readonly bool _isIgnoreWhiteSpace;

        public DecisionTableMaker(
            FactorLevelTable factorLevelTable,
            PlusMode plusMode = PlusMode.FillEnd,
            bool isIgnoreWhiteSpace = true)
        {
            //TODO:深さ3以上は例外にすること
            this._factorLevelTable = factorLevelTable;
            _rpn = new RPN(plusMode);
            _isIgnoreWhiteSpace = isIgnoreWhiteSpace;
        }

        /// <summary>
        /// 因子水準の計算式から決定表を作成する
        /// </summary>
        /// <param name="formulaText">因子水準の計算式。具体例) [OS] * [Language] + [Version]</param>
        public DecisionTable CreateFrom(string formulaText)
        {
            if (_isIgnoreWhiteSpace)
            {
                formulaText = RemoveWhiteSpace(formulaText);
            }

            var tokenList = new Tokenizer().Tokenize(formulaText);//トークンの取り出し

            //逆ポーランド記法になったトークン集
            var rpn = _rpn.ToRPN(tokenList);

            //独自定義の水準を持つ因子は、因子名を更新。新因子 - > 水準のリストを抽出
            var logic = new OriginalLevelLogic();
            (rpn, List<Factor> factorOverwriteList) = logic.OverwriteByOriginalLevels(rpn);

            //入力 : 因子のリスト(因子は複数の水準を持つ)
            var factorsTmp = _factorLevelTable.Factors;

            //新定義の因子-水準で上書き。ない場合は新規追加
            var factors = logic.OverwriteFactors(factorsTmp, factorOverwriteList);

            //期待する出力 : テストケースの集合体。各テストケースは、(string : 因子, string : 水準)のリストを持つ

            int bestScore = int.MaxValue;
            DecisionTable bestDecisionTable = DecisionTable.Empty;
            int repeatCount = (rpn.Contains("<")) ? 100 : 1;
            for (int i = 0; i < repeatCount; i++)
            {
                IEnumerable<IEnumerable<(string, string)>> combinations = _rpn.EvaluateRPN(rpn, factors);

                var testCases = combinations.Select(factorLevelCombination => new TestCase(factorLevelCombination));
                var decisionTable = new DecisionTable(testCases);

                var optimizer = new TestCaseOptimizer();
                var optResult = optimizer.CalcUncoverdPairNum(decisionTable);

                if (bestScore > optResult.Score)
                {
                    bestScore = optResult.Score;
                    bestDecisionTable = decisionTable;
                }
            }
            return bestDecisionTable;
        }

        private string RemoveWhiteSpace(string formulaText)
        {
            return Regex.Replace(formulaText, @"\s+", "");
        }
    }

    public static class CartesianProductExtensions
    {
        /// <summary>
        /// すでにある因子の集合体に、新しい因子を掛け合わせる(直積)
        /// </summary>
        public static List<List<(string, string)>> Cartesian
            (this IEnumerable<List<(string, string)>> combinationList, IEnumerable<(string, string)> newFactorAndLevels)
        {
            var result = new List<List<(string, string)>>();

            foreach (var combination in combinationList)
            {
                //combination : (OS, Windows), (Language, Japanese)
                foreach (var factorAndLevel in newFactorAndLevels)
                {
                    var newCombination = new List<(string, string)>();
                    newCombination.AddRange(combination);

                    //combination : (OS, Windows), (Language, Japanese), (Version, 1.0)
                    newCombination.Add(factorAndLevel);
                    result.Add(newCombination);
                }
            }

            return result;
        }
    }
}
