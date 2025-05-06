using ClosedXML.Excel;
using System.Data;
namespace ExcelAccessLib
{
    public class ExcelFile
    {
        private readonly ExcelProperty _excelProperty;
        private readonly DataTable _decisionTable;

        public ExcelFile(DataTable decisionTable, ExcelProperty excelProperty)
        {
            _excelProperty = excelProperty;
            _decisionTable = decisionTable;
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

            // 新しいワークシートを追加
            var worksheet = workbook.Worksheets.Add(_excelProperty.SheetName);

            //タイトル
            int rowIdx = 1;
            worksheet.Cell(rowIdx++, 1).Value = _excelProperty.Title;

            const int RightSideAtrCol = 9;
            const int RightSideValCol = RightSideAtrCol + 2;

            worksheet.Cell(rowIdx, RightSideAtrCol).Value = "作成者";
            worksheet.Cell(rowIdx++, RightSideValCol).Value = _excelProperty.Author;

            worksheet.Cell(rowIdx, RightSideAtrCol).Value = "新規作成日時";
            worksheet.Cell(rowIdx++, RightSideValCol).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            rowIdx++;

            //_excelProperty.Propertiesの内容を追加。Property数は可変であることに注意
            //列はBにkey, Dにvalue
            foreach (var keyValPair in _excelProperty.Properties)
            {
                worksheet.Cell(rowIdx, 2).Value = keyValPair.Key;
                worksheet.Cell(rowIdx, 4).Value = keyValPair.Value;
                rowIdx++;
            }

            //1行あける
            rowIdx++;

            //_decisionTableの内容を追加
            //DataTable全体を追加する
            if (_decisionTable != null)
            {
                // DataTableの列ヘッダーを追加
                int tableStartRow = rowIdx;
                for (int colIdx = 0; colIdx < _decisionTable.Columns.Count; colIdx++)
                {
                    worksheet.Cell(rowIdx, ToDecisionTableColIdx(colIdx)).Value = _decisionTable.Columns[colIdx].ColumnName;
                }
                rowIdx++;

                // DataTableのデータを追加
                foreach (DataRow row in _decisionTable.Rows)
                {
                    for (int colIdx = 0; colIdx < _decisionTable.Columns.Count; colIdx++)
                    {
                        worksheet.Cell(rowIdx, ToDecisionTableColIdx(colIdx)).Value = row[colIdx]?.ToString();
                    }
                    rowIdx++;
                }
            }

            return workbook;
        }

        private int ToDecisionTableColIdx(int colIdx)
        {
            return colIdx + 1 + 1;//1始まりかつB列から始まる
        }
    }
}
