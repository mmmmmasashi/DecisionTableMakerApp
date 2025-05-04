using DecisionTableLib.FormulaAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DecisionTableLibTest
{
    public class FillerTest
    {
        [Fact]
        public void 要素数10を目指して3種類で埋める()
        {
            var before = new string[] { "OS", "Language", "Version" }.ToList();
            var after = new Filler().Fill<string>(before, 10, PlusMode.FillEven);

            Assert.Equal("OS", after[0]);
            Assert.Equal("OS", after[1]);
            Assert.Equal("OS", after[2]);
            Assert.Equal("Language", after[3]);
            Assert.Equal("Language", after[4]);
            Assert.Equal("Language", after[5]);
            Assert.Equal("Version", after[6]);
            Assert.Equal("Version", after[7]);
            Assert.Equal("Version", after[8]);
            Assert.Equal("Version", after[9]);
            Assert.Equal(10, after.Count);

        }
    }
}
