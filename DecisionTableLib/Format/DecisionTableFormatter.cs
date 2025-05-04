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

            //列名定義
            table.Columns.Add("因子", typeof(string));
            table.Columns.Add("水準", typeof(string));

            //テストケースは後回し
            //因子と水準を1,2列目に埋める
            var factors = decisionTable.Factors;
            AddFactorAndLevelToDataTable(table, factors);

            //DataRow row = table.NewRow();
            //row["因子"] = "OS";
            //row["水準"] = "Windows";
            //table.Rows.Add(row);


            //var countOfCase = decisionTable.Factors.Count;
            //for (int i = 0; i < countOfCase; i++)
            //{
            //    table.Columns.Add($"{i + 1}", typeof(string));
            //}



            return table;
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
