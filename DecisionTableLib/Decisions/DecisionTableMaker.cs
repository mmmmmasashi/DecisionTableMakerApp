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

        public DecisionTableMaker(FactorLevelTable factorLevelTable)
        {
            //TODO:深さ3以上は例外にすること
            this._factorLevelTable = factorLevelTable;
        }

        public DecisionTable CreateFrom(string formulaText)
        {
            //TODO:string解析は後回し
            //Language * OSだと分かったとして

            var factorList = new string[] { "OS", "Language", "Version" };

            var list = _factorLevelTable.Factors
                .Where(factor => factorList.Contains(factor.Name))
                .OrderBy(factor => Array.IndexOf(factorList, factor.Name))
                .Select(factor => factor.Levels.Select(level => (factor.Name, level)))
                .ToList();

            //こういう状態
            // (OS, Windows), (OS, Mac), (OS, Linux)
            // (Language, Japanese), (Language, English), (Language, Chinese)
            // (Version, 1.0), (Version, 2.0)

            //直積をとる
            var combinations = CartesianProduct(list).ToList();
            //この時点で以下の状態
            //(OS, Windows), (Language, Japanese)
            //(OS, Windows), (Language, English)
            //:
            var testCases = combinations.Select(factorLevelCombination => new TestCase(factorLevelCombination));
            return new DecisionTable(testCases);
        }

        // 汎用的な直積生成メソッド
        static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };

            foreach (var sequence in sequences)
            {
                result = result.SelectMany(
                    acc => sequence,
                    (acc, item) => acc.Append(item)
                );
            }

            return result;
        }
    }
}
