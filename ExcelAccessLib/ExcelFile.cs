using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Data;
namespace ExcelAccessLib
{
    public class ExcelFile
    {
        private readonly Func<string, DataTable> _decisionTableCreator;
        private readonly ExcelBookProperty _excelProperty;
        private readonly List<ExcelSheetProperty> _sheetPropertyList;

        /// <summary>
        /// Excelファイル出力クラス
        /// </summary>
        /// <param name="decisionTableCreator">計算式を入れるとDataTableを出力するFunc</param>
        public ExcelFile(
            Func<string, DataTable> decisionTableCreator,
            ExcelBookProperty excelProperty,
            List<ExcelSheetProperty> sheetPropertyList)
        {
            _decisionTableCreator = decisionTableCreator;
            _excelProperty = excelProperty;
            _sheetPropertyList = sheetPropertyList;
        }

        public void Export(string targetFilePath)
        {
            //親フォルダがなければ例外
            var parentDir = System.IO.Path.GetDirectoryName(targetFilePath);
            if (string.IsNullOrEmpty(parentDir) || !System.IO.Directory.Exists(parentDir))
            {
                throw new System.IO.DirectoryNotFoundException($"親フォルダが存在しません: {parentDir}");
            }

            XLWorkbook workbook = CreateWorkbook();

            // ファイルを保存
            workbook.SaveAs(targetFilePath);
        }

        private XLWorkbook CreateWorkbook()
        {
            // 新しいExcelワークブックを作成
            var workbook = new XLWorkbook();

            for (int sheetIdx = 0; sheetIdx < _sheetPropertyList.Count; sheetIdx++)
            {
                AddWorksheet(workbook, sheetIdx);
            }

            return workbook;
        }

        private void AddWorksheet(XLWorkbook workbook, int sheetIdx)
        {
            var sheetProperty = _sheetPropertyList[sheetIdx];
            // 新しいワークシートを追加
            var truncatedSheetName = TruncSheetName(sheetProperty.SheetName);
            var worksheet = workbook.Worksheets.Add(truncatedSheetName);

            //タイトル
            int rowIdx = 1;
            rowIdx = WriteHeader(sheetProperty, worksheet, rowIdx);

            //情報追記
            rowIdx = WriteProperties(sheetProperty, worksheet, rowIdx);

            //1行あける
            rowIdx++;

            rowIdx = WriteDecitionTable(sheetProperty, worksheet, rowIdx);
        }

        private static int WriteProperties(ExcelSheetProperty sheetProperty, IXLWorksheet worksheet, int rowIdx)
        {
            worksheet.Cell(rowIdx, 2).Value = "検査観点";
            worksheet.Cell(rowIdx++, 4).Value = sheetProperty.Inspection;

            worksheet.Cell(rowIdx, 2).Value = "計算式";
            worksheet.Cell(rowIdx++, 4).Value = sheetProperty.Formula;
            return rowIdx;
        }

        private int WriteHeader(ExcelSheetProperty sheetProperty, IXLWorksheet worksheet, int rowIdx)
        {
            worksheet.Cell(rowIdx++, 1).Value = sheetProperty.SheetName;

            const int RightSideAtrCol = 9;
            const int RightSideValCol = RightSideAtrCol + 2;

            worksheet.Cell(rowIdx, RightSideAtrCol).Value = "作成者";
            worksheet.Cell(rowIdx++, RightSideValCol).Value = _excelProperty.Author;

            worksheet.Cell(rowIdx, RightSideAtrCol).Value = "新規作成日時";
            worksheet.Cell(rowIdx++, RightSideValCol).Value = _excelProperty.ExportTime.ToString("yyyy/MM/dd HH:mm:ss");

            rowIdx++;
            return rowIdx;
        }

        private int WriteDecitionTable(ExcelSheetProperty sheetProperty, IXLWorksheet worksheet, int rowIdx)
        {
            //DataTable全体を追加する
            int leftTopRowIdx = rowIdx;
            int leftTopColIdx = ToDecisionTableColIdx(0);
            var decitionTable = _decisionTableCreator(sheetProperty.Formula);
            if (decitionTable != null)
            {
                // DataTableのデータを追加
                foreach (DataRow row in decitionTable.Rows)
                {
                    for (int colIdx = 0; colIdx < decitionTable.Columns.Count; colIdx++)
                    {
                        worksheet.Cell(rowIdx, ToDecisionTableColIdx(colIdx)).Value = row[colIdx]?.ToString();
                    }
                    rowIdx++;
                }
            }
            int rightBottomRowIdx = rowIdx - 1;
            int rightBottomColIdx = ToDecisionTableColIdx(decitionTable.Columns.Count - 1);

            var tableRange = worksheet.Range(leftTopRowIdx, leftTopColIdx, rightBottomRowIdx, rightBottomColIdx);

            //格子状の罫線を引く
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;  // 外枠
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;   // 内部線
            return rowIdx;
        }

        /// <summary>
        /// シート名は31文字まで
        /// </summary>
        private string TruncSheetName(string sheetName)
        {
            if (sheetName.Length > 31)
            {
                sheetName = sheetName.Substring(0, 31);
            }
            return sheetName;
        }

        private int ToDecisionTableColIdx(int colIdx)
        {
            return colIdx + 1 + 1;//1始まりかつB列から始まる
        }
    }
}
