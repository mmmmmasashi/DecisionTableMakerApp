using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public record Level
    {
        private readonly static string DontCareName = "Don't Care";
        public static Level DontCare { get => new Level(DontCareName); }

        public string Name { get; }
        public Level(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Name = DontCareName;
            }
            else
            {
                Name = name;
            }
        }
    }
}
