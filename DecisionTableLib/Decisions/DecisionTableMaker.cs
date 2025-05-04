using DecisionTableLib.FormulaAnalyzer;
using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class DecisionTableMaker
    {
        private readonly FactorLevelTable _factorLevelTable;
        private readonly bool _evenPlusMode;
        private readonly RPN _rpn;

        public DecisionTableMaker(FactorLevelTable factorLevelTable, PlusMode plusMode = PlusMode.FillEnd)
        {
            //TODO:深さ3以上は例外にすること
            this._factorLevelTable = factorLevelTable;
            _rpn = new RPN(plusMode);
        }

        public static DecisionTableMaker EmptyTableMaker { get => new DecisionTableMaker(FactorLevelTable.EmptyTable, PlusMode.FillEven); }

        /// <summary>
        /// 因子水準の計算式から決定表を作成する
        /// </summary>
        /// <param name="formulaText">因子水準の計算式。具体例) [OS] * [Language] + [Version]</param>
        public DecisionTable CreateFrom(string formulaText)
        {
            var tokenList = new Tokenizer().Tokenize(formulaText);//トークンの取り出し

            //逆ポーランド記法になったトークン集
            var rpn = _rpn.ToRPN(tokenList);

            //入力 : 因子のリスト(因子は複数の水準を持つ)
            var factors = _factorLevelTable.Factors;

            //期待する出力 : テストケースの集合体。各テストケースは、(string : 因子, string : 水準)のリストを持つ
            IEnumerable<IEnumerable<(string, string)>> combinations = _rpn.EvaluateRPN(rpn, factors);

            var testCases = combinations.Select(factorLevelCombination => new TestCase(factorLevelCombination));
            return new DecisionTable(testCases);
        }

        // 汎用的な直積生成メソッド
        //static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        //{
        //    IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };

        //    foreach (var sequence in sequences)
        //    {
        //        result = result.SelectMany(
        //            acc => sequence,
        //            (acc, item) => acc.Append(item)
        //        );
        //    }

        //    return result;
        //}


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
