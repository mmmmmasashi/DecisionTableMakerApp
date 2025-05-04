using DecisionTableLib.Decisions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Format
{
    /// <summary>
    /// 出力を見越した整形はここで行う
    /// </summary>
    public class DecisionTableFormatter
    {
        private DecisionTable decisionTable;

        public DecisionTableFormatter(DecisionTable decisionTable)
        {
            this.decisionTable = decisionTable;
        }

        public DataTable ToDataTable()
        {
            var table = new DataTable();

            //TODO:埋め込みになっている
            //列名定義
            table.Columns.Add("因子", typeof(string));
            table.Columns.Add("水準", typeof(string));

            //テストケースは後回し
            //因子と水準を1,2列目に埋める
            var factors = decisionTable.Factors;
            AddFactorAndLevelToDataTable(table, factors);

            //テストケース単位で列を追加
            AddTestCasesToDataTable(table, decisionTable);

            return table;
        }

        /// <summary>
        /// テストケース集をDataTableに追加する。列を1つずつ足していく。
        /// </summary>
        private void AddTestCasesToDataTable(DataTable dataTable, DecisionTable decisionTable)
        {
            int caseNumber = 1;
            var testCases = decisionTable.TestCases;

            //水準名が別の因子の水準と重複したケースは考慮する必要がある
            //そのため、まず因子名のIndexを先に探し、その後水準名を探す
            foreach (var testCase in testCases)
            {
                //テストケースごとに列を追加
                dataTable.Columns.Add(caseNumber++.ToString(), typeof(string));

                //テストケースの水準に点を打つ
                const string Dot = "x";
                foreach (var factorName in testCase.FactorNames)
                {
                    int rowIdxOfTargetFactor = SearchRowIndexByFactorName(dataTable, factorName);
                    var targetLevelName = testCase.LevelOf(factorName).Name;
                    int rowIdxOfTargetLevel = SearchRowIndexByLevelName(dataTable, rowIdxOfTargetFactor, targetLevelName);

                    int lastColIdx = dataTable.Columns.Count - 1;
                    dataTable.Rows[rowIdxOfTargetLevel][lastColIdx] = Dot;
                }
            }
        }

        /// <summary>
        /// 因子名を元にDataTableの行Indexを探す
        /// ただし、rowIdxStart以降の行を探す
        /// </summary>
        private int SearchRowIndexByLevelName(DataTable dataTable, int rowIdxStart, string targetLevelName)
        {
            for (int i = rowIdxStart; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                if (row["水準"].ToString() == targetLevelName)
                {
                    return i;
                }
            }
            throw new Exception($"水準名 {targetLevelName} が見つかりませんでした");
        }

        /// <summary>
        /// 因子名を元にDataTableの行Indexを探す
        /// </summary>
        private int SearchRowIndexByFactorName(DataTable dataTable, string factorName)
        {
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                if (row["因子"].ToString() == factorName)
                {
                    return i;
                }
            }
            throw new Exception($"因子名 {factorName} が見つかりませんでした");
        }

        private void AddFactorAndLevelToDataTable(DataTable table, List<Factor> factors)
        {
            //例
            //OS        Windows
            //          Mac
            //          Linux
            //Language  Japanese
            //          English
            //          Chinese
            foreach (var factor in factors)
            {
                //1行目
                //因子名を追加
                var row = table.NewRow();
                row["因子"] = factor.Name;
                row["水準"] = factor.Levels.FirstOrDefault()?.Name;
                table.Rows.Add(row);

                //水準を追加. これは因子は空欄
                for (int i = 1; i < factor.Levels.Count;i++)
                {
                    var level = factor.Levels[i];
                    //水準名を追加
                    //因子名は空欄
                    var levelRow = table.NewRow();
                    levelRow["因子"] = "";
                    levelRow["水準"] = level.Name;
                    table.Rows.Add(levelRow);
                }
            }
        }
    }
}
