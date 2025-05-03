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
        private TreeNode _factorAndLevelRootNode;

        public DecisionTableMaker(TreeNode factorAndLevelRootNode)
        {
            //TODO:深さ3以上は例外にすること
            this._factorAndLevelRootNode = factorAndLevelRootNode;
        }

        public DecisionTable CreateFrom(string formulaText)
        {
            //TODO:string解析は後回し
            //Language * OSだと分かったとして

            var factorList = new string[] { "Language", "OS" };

            var list = _factorAndLevelRootNode.Children//因子のリスト
                .Select(factorNode => factorNode.Children.Select(level => (factorNode.Name, level.Name)).ToList());

            var combinations = CartesianProduct(list).ToList();

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
