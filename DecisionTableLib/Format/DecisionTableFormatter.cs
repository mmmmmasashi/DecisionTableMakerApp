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

            table.Columns.Add("因子", typeof(string));
            table.Columns.Add("水準", typeof(string));

            var countOfCase = decisionTable.Factors.Count;
            for (int i = 0; i < countOfCase; i++)
            {
                table.Columns.Add($"{i + 1}", typeof(string));
            }

            DataRow row = table.NewRow();
            row["因子"] = "OS";
            row["水準"] = "Windows";
            table.Rows.Add(row);


            return table;
        }
    }
}
