using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAccessLib
{
    public class ExcelSheetProperty
    {
        public string SheetName { get; }
        public string Inspection { get; }
        public string Formula { get; }

        public ExcelSheetProperty(string sheetName, string inspection, string formula)
        {
            this.SheetName = sheetName;
            this.Inspection = inspection;
            this.Formula = formula;
        }
    }
}
