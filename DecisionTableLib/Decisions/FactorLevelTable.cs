using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    /// <summary>
    /// 因子水準表
    /// </summary>
    public class FactorLevelTable
    {
        public static FactorLevelTable EmptyTable { get => new FactorLevelTable(new TreeNode("root")); }

        private readonly List<Factor> _table = new ();


        public FactorLevelTable(TreeNode root)
        {
            var factorNodes = root.Children;
            foreach (var factorNode in factorNodes)
            {
                var levels = factorNode.Children.Select(level => new Level(level.Name)).ToList();
                _table.Add(new Factor(factorNode.Name, levels));
            }
        }

        public List<Factor> Factors { get => new List<Factor>(_table); }
    }
}
