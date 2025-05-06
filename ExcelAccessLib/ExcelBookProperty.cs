using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAccessLib
{
    public record ExcelBookProperty
    {
        public string Author { get; }
        public DateTime ExportTime { get; }

        public ExcelBookProperty(string creatorName, DateTime exportTime)
        {

            ExportTime = exportTime;

            if (string.IsNullOrEmpty(creatorName))
            {
                Author = "Unknown";
            }
            else
            {
                Author = creatorName;
            }

        }
    }
}
