using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Data;
namespace ExcelAccessLib
{
    //シート作成時の例外
    public class ExcelSheetCreateException : System.Exception
    {
        public int SheetNumber { get; }
        public string SheetName { get; }
        
        public ExcelSheetCreateException(int sheetNumber, string sheetName, Exception ex) : base(ex.Message, ex)
        {
            SheetNumber = sheetNumber;
            SheetName = sheetName;
        }
    }

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

        /// <summary>
        /// Excelファイルを出力する
        /// </summary>
        /// <returns>変換時に起きた各シートの例外のリスト</returns>
        public List<ExcelSheetCreateException> Export(string targetFilePath)
        {
            //親フォルダがなければ例外
            var parentDir = System.IO.Path.GetDirectoryName(targetFilePath);
            if (string.IsNullOrEmpty(parentDir) || !System.IO.Directory.Exists(parentDir))
            {
                throw new System.IO.DirectoryNotFoundException($"親フォルダが存在しません: {parentDir}");
            }

            (XLWorkbook workbook, List<ExcelSheetCreateException> sheetExceptions) = CreateWorkbook();

            // ファイルを保存
            workbook.SaveAs(targetFilePath);

            return sheetExceptions;
        }

        private (XLWorkbook, List<ExcelSheetCreateException>) CreateWorkbook()
        {
            var sheetExceptions = new List<ExcelSheetCreateException>();
            var sheetSummaries = new Dictionary<int, SheetSummary>();
            // 新しいExcelワークブックを作成
            var workbook = new XLWorkbook();

            for (int sheetIdx = 0; sheetIdx < _sheetPropertyList.Count; sheetIdx++)
            {
                SheetSummary sheetSummary;
                int sheetNumber = sheetIdx + 1;
                try
                {
                    int caseNum = AddWorksheet(workbook, sheetIdx);
                    sheetSummary = SheetSummary.Success(caseNum);
                }
                catch (ExcelSheetCreateException excelException)
                {
                    sheetExceptions.Add(excelException);
                    sheetSummary = SheetSummary.Failure(excelException);
                }
                catch(Exception ex)
                {
                    var wrappedException = new ExcelSheetCreateException(sheetIdx + 1, "unknown", ex);
                    sheetExceptions.Add(wrappedException);
                    sheetSummary = SheetSummary.Failure(wrappedException);
                }

                sheetSummaries.Add(sheetNumber, sheetSummary);
            }

            // サマリーシートを先頭に追加
            AddSummarySheet(workbook, sheetSummaries);

            return (workbook, sheetExceptions);
        }

        private void AddSummarySheet(XLWorkbook workbook, Dictionary<int, SheetSummary> sheetSummaries)
        {
            // 新しい「サマリー」シートを作成
            var summarySheet = workbook.Worksheets.Add("全体", 1); // 先頭に追加

            int rowIdx = 1;

            // 全体情報の記載
            summarySheet.Cell(rowIdx++, 1).Value = "全体情報";

            summarySheet.Cell(rowIdx, 1).Value = "作成者";
            summarySheet.Cell(rowIdx++, 2).Value = $"{_excelProperty.Author}";

            summarySheet.Cell(rowIdx, 1).Value = $"作成日時";
            summarySheet.Cell(rowIdx++, 2).Value = $"{_excelProperty.ExportTime:yyyy/MM/dd HH:mm:ss}";

            //全体情報(解析成功シート数 {successNum}/{totalNum}
            summarySheet.Cell(rowIdx, 1).Value = $"解析成功観点数";
            int successNum = sheetSummaries.Values.Count(summary => summary.IsSuccess);
            int totalNum = sheetSummaries.Values.Count;
            summarySheet.Cell(rowIdx++, 2).Value = $"{successNum}/{totalNum}";

            //全体情報 総ケース数
            summarySheet.Cell(rowIdx, 1).Value = $"総ケース数";
            int totalCaseNum = sheetSummaries.Values.Where(summary => summary.IsSuccess).Sum(summary => summary.CaseNum);
            summarySheet.Cell(rowIdx++, 2).Value = $"{totalCaseNum}";

            //1行開ける
            rowIdx++;
            const int TableColLeftIdx = 2;

            // 各シートの情報を記載
            summarySheet.Cell(rowIdx++, 1).Value = "シート一覧";

            //ヘッダーの設定
            int colIdx = TableColLeftIdx;
            summarySheet.Cell(rowIdx, colIdx++).Value = "No.";//No.
            summarySheet.Cell(rowIdx, colIdx++).Value = "シート名";//シート名
            summarySheet.Cell(rowIdx, colIdx++).Value = "検査観点";//検査観点
            summarySheet.Cell(rowIdx, colIdx++).Value = "計算式";//計算式
            summarySheet.Cell(rowIdx, colIdx++).Value = "ケース数";//ケース数
            summarySheet.Cell(rowIdx, colIdx++).Value = "エラーメッセージ";//エラーメッセージ
            rowIdx++;//1行あける

            //各シートの情報を記載
            for (int i = 0; i < _sheetPropertyList.Count; i++)
            {
                colIdx = TableColLeftIdx;//初期化

                var sheetProperty = _sheetPropertyList[i];
                int sheetNumber = i + 1;
                var sheetSummary = sheetSummaries[sheetNumber];
                summarySheet.Cell(rowIdx, colIdx++).Value = $"{i + 1}";//No.
                summarySheet.Cell(rowIdx, colIdx++).Value = sheetProperty.SheetName;//シート名
                summarySheet.Cell(rowIdx, colIdx++).Value = sheetProperty.Inspection;//検査観点
                summarySheet.Cell(rowIdx, colIdx++).Value = sheetProperty.Formula;//計算式
                summarySheet.Cell(rowIdx, colIdx++).Value = (sheetSummary.IsSuccess)? sheetSummary.CaseNum : "ERROR";//ケース数

                // 修正箇所: null 参照の可能性があるものの逆参照を防ぐため、null 条件演算子を使用  
                summarySheet.Cell(rowIdx, colIdx++).Value = (sheetSummary.IsSuccess) ? "" : sheetSummary.SheetCreateException?.Message ?? "エラー情報なし"; // エラーメッセージ  
                rowIdx++;//改行
            }

            // サマリーシートのスタイル設定（例: 太字や罫線）
            var headerRange = summarySheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        /// <summary>
        /// Worksheet追加
        /// </summary>
        /// <returns>ケース数</returns>
        private int AddWorksheet(XLWorkbook workbook, int sheetIdx)
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

            (rowIdx, int caseNum) = WriteDecitionTable(sheetProperty, worksheet, rowIdx, sheetIdx);
            return caseNum;
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

        private (int rowIdx, int caseNum) WriteDecitionTable(ExcelSheetProperty sheetProperty, IXLWorksheet worksheet, int rowIdx, int sheetIdx)
        {
            //DataTable全体を追加する
            int leftTopRowIdx = rowIdx;
            int leftTopColIdx = ToDecisionTableColIdx(0);

            DataTable decisionTable;
            int caseNum = 0;
            try
            {
                decisionTable = _decisionTableCreator(sheetProperty.Formula);
                caseNum = decisionTable.Columns.Count - 2;//最初の2列は因子名と水準名 //FIXME: 1行目の末尾の数字を取る方がロバスト
            }
            catch (Exception ex)
            {
                //Excelにもエラー内容を書き込む
                worksheet.Cell(rowIdx, ToDecisionTableColIdx(0)).Value = $"エラー";
                worksheet.Cell(rowIdx++, ToDecisionTableColIdx(1)).Value = $"{ex.Message}";

                //worksheet.Cell(rowIdx++, ToDecisionTableColIdx(0)).Value = $"スタックトレース";
                //worksheet.Cell(rowIdx++, ToDecisionTableColIdx(1)).Value = $"{ex.StackTrace}";

                //その上で終了
                throw new ExcelSheetCreateException(sheetIdx + 1, sheetProperty.SheetName, ex);
            }

            if (decisionTable != null)
            {
                // DataTableのデータを追加
                foreach (DataRow row in decisionTable.Rows)
                {
                    for (int colIdx = 0; colIdx < decisionTable.Columns.Count; colIdx++)
                    {
                        worksheet.Cell(rowIdx, ToDecisionTableColIdx(colIdx)).Value = row[colIdx]?.ToString();
                    }
                    rowIdx++;
                }
            }
            int rightBottomRowIdx = rowIdx - 1;
            int rightBottomColIdx = ToDecisionTableColIdx(decisionTable.Columns.Count - 1);

            var tableRange = worksheet.Range(leftTopRowIdx, leftTopColIdx, rightBottomRowIdx, rightBottomColIdx);

            //格子状の罫線を引く
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;  // 外枠
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;   // 内部線
            return (rowIdx, caseNum);
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
