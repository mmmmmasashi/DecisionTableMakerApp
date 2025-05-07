using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAccessLib
{
    internal record SheetSummary
    {
        /// <summary>
        /// ケース数
        /// </summary>
        internal int CaseNum { get; }

        /// <summary>
        /// 解析時の例外
        /// </summary>
        internal ExcelSheetCreateException? SheetCreateException { get; }
        internal bool IsSuccess => SheetCreateException == null;

        private SheetSummary(int caseNum, ExcelSheetCreateException? sheetCreateException)
        {
            CaseNum = caseNum;
            SheetCreateException = sheetCreateException;
        }

        internal static SheetSummary Success(int caseNum)
        {
            return new SheetSummary(caseNum, null);
        }

        internal static SheetSummary Failure(ExcelSheetCreateException sheetCreateException)
        {
            return new SheetSummary(0, sheetCreateException);
        }
    }
}
