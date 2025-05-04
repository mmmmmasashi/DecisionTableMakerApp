using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DecisionTableLib.Decisions;

namespace DecisionTableLibTest
{
    public class DecisionTableTest
    {
        private readonly DecisionTable decisionTable;

        public DecisionTableTest()
        {
            decisionTable = new DecisionTable(new List<TestCase>
            {
                new TestCase(new List<(string, string)>() {("OS", "Windows"), ("Language", "Japanese")}),
                new TestCase(new List<(string, string)>() {("OS", "Windows"), ("Language", "English")}),
                new TestCase(new List<(string, string)>() {("OS", "Mac"), ("Language", "Japanese")}),
                new TestCase(new List<(string, string)>() {("OS", "Mac"), ("Language", "English")}),
            });
        }


        [Fact]
        public void DecisionTableは因子と水準が分かる()
        {
            //因子と水準に何があるかは別途取得できる
            var factors = decisionTable.Factors;
            Assert.Equal(2, factors.Count);

            Assert.Equal("OS", factors[0].Name);
            Assert.Equal(new Level("Windows"), factors[0].Levels[0]);
            Assert.Equal(new Level("Mac"), factors[0].Levels[1]);

            Assert.Equal("Language", factors[1].Name);
            Assert.Equal(new Level("Japanese"), factors[1].Levels[0]);
            Assert.Equal(new Level("English"), factors[1].Levels[1]);
        }

        //[Fact]
        //public void DecisionTableはDataTableに変換できる_シンプルな例()
        //{
        //    //因子と水準に何があるかは別途取得できる
        //    var factors = decisionTable.Factors;
        //    Assert.Equal(2, factors.Count);

        //    Assert.Equal("OS", factors[0].Name);
        //    Assert.Equal(new Level("Windows"), factors[0].Levels[0]);
        //    Assert.Equal(new Level("Mac"), factors[0].Levels[1]);

        //    Assert.Equal("Language", factors[1].Name);
        //    Assert.Equal(new Level("Japanese"), factors[1].Levels[0]);
        //    Assert.Equal(new Level("English"), factors[1].Levels[1]);
        //}
    }
}
