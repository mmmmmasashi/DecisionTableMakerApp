using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class TestCase
    {
        //因子 - 水準のペアのリスト
        private Dictionary<string, string> _factorLevelPairs = new Dictionary<string, string>();

        public TestCase(IEnumerable<(string, string)> factorLevelCombination)
        {
            foreach (var pair in factorLevelCombination)
            {
                _factorLevelPairs.Add(pair.Item1, pair.Item2);
            }
        }

        public Level LevelOf(string factorKey)
        {
            if (_factorLevelPairs.ContainsKey(factorKey) == false)
            {
                return Level.DontCare;
            }
            return new Level(_factorLevelPairs[factorKey]);
        }
    }
}
