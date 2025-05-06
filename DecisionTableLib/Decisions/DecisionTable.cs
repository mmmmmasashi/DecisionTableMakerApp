using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    /// <summary>
    /// ディシジョンテーブル
    /// </summary>
    public class DecisionTable
    {
        public static DecisionTable Empty { get => new DecisionTable(new List<TestCase>()); }
        /// <summary>
        /// テストケースのリスト
        /// </summary>
        internal List<TestCase> TestCases { get; set; }

        /// <summary>
        /// 因子のリスト
        /// </summary>
        public List<Factor> Factors { get => CreateFactors(); }

        /// <summary>
        /// 決定表のコンストラクタ
        /// </summary>
        public DecisionTable(IEnumerable<TestCase> testCases)
        {
            TestCases = testCases.ToList();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var testCase in TestCases)
            {
                sb.AppendLine(testCase.ToString());
            }
            return sb.ToString().TrimEnd('\n', '\r'); // 最後の改行を削除
        }

        private List<Factor> CreateFactors()
        {
            var factorNames = TestCases.SelectMany(tc => tc.FactorNames).Distinct();

            //因子名のリスト -> (因子 1-- * 水準)のリストに変換してFactorクラスにする
            var factors = factorNames.Select(factorName =>
            {
                var levels = new List<Level>();
                foreach (var testCase in TestCases)
                {
                    if (!testCase.HasFactorOf(factorName)) continue;
                    var level = testCase.LevelOf(factorName);
                    if (levels.Contains(level)) continue;
                    levels.Add(level);
                }
                return new Factor(factorName, levels);
            }).ToList();

            return factors;
        }
    }
}
