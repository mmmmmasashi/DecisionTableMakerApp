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

        public IEnumerable<string> FactorNames { get => _factorLevelPairs.Keys; }

        public Level LevelOf(string factorKey)
        {
            if (_factorLevelPairs.ContainsKey(factorKey) == false)
            {
                return Level.DontCare;
            }
            return new Level(_factorLevelPairs[factorKey]);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in _factorLevelPairs)
            {
                sb.Append($"{pair.Key}: {pair.Value}, ");
            }
            return sb.ToString().TrimEnd(',', ' '); // 最後のカンマとスペースを削除
        }

        internal bool HasFactorOf(string factorName)
        {
            return _factorLevelPairs.ContainsKey(factorName);
        }
    }
}
