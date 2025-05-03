using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Trees
{
    public class TreeNode
    {
        public bool IsExpanded { get; set; } = true; // 展開状態
        public string Name { get; }
        public List<TreeNode> Children { get; }

        public TreeNode(string name)
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
