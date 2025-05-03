using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Trees
{
    internal class TreeNode
    {
        internal string Name { get; }
        internal List<TreeNode> Children { get; }

        internal TreeNode(string name)
        {
            Name = name;
            Children = new List<TreeNode>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
