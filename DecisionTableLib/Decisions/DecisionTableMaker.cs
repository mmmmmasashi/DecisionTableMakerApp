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
        private TreeNode factorAndLevelRootNode;

        public DecisionTableMaker(TreeNode factorAndLevelRootNode)
        {
            this.factorAndLevelRootNode = factorAndLevelRootNode;
        }

        public DecisionTable CreateFrom(string value)
        {
            throw new NotImplementedException();
        }
    }
}
