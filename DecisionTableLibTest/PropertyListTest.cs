using DecisionTableLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DecisionTableLibTest
{
    public class PropertyListTest
    {
        [Fact]
        public void 二つペアのタプルに変換できること()
        {
            var propertyList = new PropertyList();
            var memories = propertyList.FromPropertyString("あ|いうえ|123|678|123|6789");
            
            Assert.Equal(3, memories.Count());
            Assert.Equal("あ", memories[0].Item1);
            Assert.Equal("いうえ", memories[0].Item2);
            Assert.Equal("123", memories[1].Item1);
            Assert.Equal("678", memories[1].Item2);
            Assert.Equal("123", memories[2].Item1);
            Assert.Equal("6789", memories[2].Item2);
        }

    }
}
