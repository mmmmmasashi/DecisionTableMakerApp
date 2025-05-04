using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class FactorLevelTable
    {
        private readonly List<Factor> _table = new ();


        public FactorLevelTable(TreeNode root)
        {
            var factorNodes = root.Children;
            foreach (var factorNode in factorNodes)
            {
                var levels = factorNode.Children.Select(level => level.Name).ToList();
                _table.Add(new Factor(factorNode.Name, levels));
            }
        }

        public IEnumerable<Factor> Factors { get => new List<Factor>(_table); }
    }
}
