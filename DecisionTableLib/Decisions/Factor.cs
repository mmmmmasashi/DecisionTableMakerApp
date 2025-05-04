using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class Factor
    {
        public string Name { get; }
        public List<string> Levels { get; }
        public Factor(string name, List<string> levels)
        {
            Name = name;
            Levels = levels;
        }
    }
}
