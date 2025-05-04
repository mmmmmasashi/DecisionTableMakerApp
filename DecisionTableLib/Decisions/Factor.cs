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
        public List<Level> Levels { get; }
        public Factor(string name, List<Level> levels)
        {
            Name = name;
            Levels = levels;
        }
    }
}
