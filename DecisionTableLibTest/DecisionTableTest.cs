using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DecisionTableLib.Decisions;
using System.Data;
using DecisionTableLib.Format;
using System.Diagnostics.Metrics;

namespace DecisionTableLibTest
{
    public class DecisionTableTest_3因子
    {
        private readonly DecisionTable decisionTable;
        private readonly DataTable dataTable;

        public DecisionTableTest_3因子()
        {
            var 因子水準表サンプル = ExcelRangeTest.三因子水準表を作成();
            var maker = new DecisionTableMaker(因子水準表サンプル);
            decisionTable = maker.CreateFrom("[OS] * [Language] * [Version]");

            var formatter = new DecisionTableFormatter(decisionTable);
            dataTable = formatter.ToDataTable();
        }

        [Fact]
        public void DecisionTableはDataTableに変換できる_因子水準部分をテスト()
        {
            Assert.Equal("因子", dataTable.Columns[TableFormatHelper.FactorColmnIdx].ColumnName);
            Assert.Equal("水準", dataTable.Columns[TableFormatHelper.LevelColmnIdx].ColumnName);

            // 行の検証を共通メソッドで行う
            int i = 0;
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "OS", "Windows");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "Mac");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "Linux");

            TableFormatHelper.AssertDataTableRow(dataTable, i++, "Language", "Japanese");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "English");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "Chinese");

            TableFormatHelper.AssertDataTableRow(dataTable, i++, "Version", "1.0");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "2.0");
        }

        [Fact]
        public void テストケース_つまり列を追加し適切に点を打てているかテスト()
        {
            Assert.Equal("x", dataTable.Rows[0]["1"]);
        }

        private int ケース列Idx(int caseIdx) => TableFormatHelper.LevelColmnIdx + caseIdx + 1;

    }

    public class DecisionTableTest_2因子
    {
        private readonly DecisionTable decisionTable;

        public DecisionTableTest_2因子()
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


        [Fact]
        public void DecisionTableはDataTableに変換できる_シンプルな例()
        {
            var formatter = new DecisionTableFormatter(decisionTable);
            DataTable dataTable = formatter.ToDataTable();

            Assert.Equal("因子", dataTable.Columns[TableFormatHelper.FactorColmnIdx].ColumnName);
            Assert.Equal("水準", dataTable.Columns[TableFormatHelper.LevelColmnIdx].ColumnName);

            // 行の検証を共通メソッドで行う
            int i = 0;
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "OS", "Windows");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "Mac");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "Language", "Japanese");
            TableFormatHelper.AssertDataTableRow(dataTable, i++, "", "English");
        }


    }
    

    internal static class TableFormatHelper
    {
        internal static int FactorColmnIdx = 0;
        internal static int LevelColmnIdx = 1;

        /// <summary>
        /// 因子と水準の値を確認する
        /// </summary>
        internal static void AssertDataTableRow(
             DataTable table, int rowIndex, string expectedFactor, string expectedLevel)
        {
            Assert.Equal(expectedFactor, table.Rows[rowIndex]["因子"]);
            Assert.Equal(expectedLevel, table.Rows[rowIndex]["水準"]);
        }
    }

    public class DecisionTableTest_2因子_因子水準の並び順を指定可能
    {
        private readonly DecisionTable decisionTable;

        public DecisionTableTest_2因子_因子水準の並び順を指定可能()
        {
            decisionTable = new DecisionTable(new List<TestCase>
            {
                new TestCase(new List<(string, string)>() {("OS", "Windows"), ("Language", "Japanese")}),
                new TestCase(new List<(string, string)>() {("OS", "Windows"), ("Language", "English")}),
                new TestCase(new List<(string, string)>() {("OS", "Mac"), ("Language", "Japanese")}),
                new TestCase(new List<(string, string)>() {("OS", "Mac"), ("Language", "English")}),
            },
            new List<Factor>() {
                new Factor("Language", new List<Level>() { new Level("English"), new Level("Japanese") }),
                new Factor("OS", new List<Level>() { new Level("Mac"), new Level("Windows") }),
            });
        }

        [Fact]
        public void DecisionTableは因子と水準が分かる()
        {
            var factors = decisionTable.Factors;
            Assert.Equal(2, factors.Count);

            //因子の並び替えはできない点に注意。これは計算式から算出したテストケースの並び順を尊重するため
            Assert.Equal("OS", factors[0].Name);
            Assert.Equal(new Level("Mac"), factors[0].Levels[0]);
            Assert.Equal(new Level("Windows"), factors[0].Levels[1]);

            Assert.Equal("Language", factors[1].Name);
            Assert.Equal(new Level("English"), factors[1].Levels[0]);
            Assert.Equal(new Level("Japanese"), factors[1].Levels[1]);

        }
    }
}
