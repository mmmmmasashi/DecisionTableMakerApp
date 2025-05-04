using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class DecisionTable
    {
        internal List<TestCase> TestCases { get; set; }

        public DecisionTable(IEnumerable<TestCase> testCases)
        {
            TestCases = testCases.ToList();
        }

        public string ToTsv()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var testCase in TestCases)
            {
                sb.AppendLine(testCase.ToString());
            }
            return sb.ToString().TrimEnd('\n', '\r'); // 最後の改行を削除
        }
    }
}
