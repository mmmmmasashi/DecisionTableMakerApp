using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    /// <summary>
    /// 因子名と水準名を持つペア
    /// </summary>
    internal record FactorLevelPair
    {
        /// <summary>
        /// 因子名
        /// </summary>
        public string FactorName { get; }
        /// <summary>
        /// 水準名
        /// </summary>
        public string LevelName { get; }
        public FactorLevelPair(string factorName, string levelName)
        {
            FactorName = factorName;
            LevelName = levelName;
        }
        public override string ToString()
        {
            return $"{FactorName}: {LevelName}";
        }
    }
}
