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
    }
}
