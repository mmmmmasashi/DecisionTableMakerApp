using ClosedXML.Excel;
namespace ExcelAccessLib
{
    public class ExcelFile
    {
        public void Export(string targetFilePath)
        {
            //親フォルダがなければ例外
            var parentDir = System.IO.Path.GetDirectoryName(targetFilePath);
            if (string.IsNullOrEmpty(parentDir) || !System.IO.Directory.Exists(parentDir))
            {
                throw new System.IO.DirectoryNotFoundException($"親フォルダが存在しません: {parentDir}");
            }

            // 新しいExcelワークブックを作成
            var workbook = new XLWorkbook();

            // 新しいワークシートを追加
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // セルに値を設定
            worksheet.Cell("A1").Value = "名前";
            worksheet.Cell("B1").Value = "年齢";
            worksheet.Cell("A2").Value = "田中";
            worksheet.Cell("B2").Value = 28;

            // ファイルを保存
            workbook.SaveAs(targetFilePath);
        }
    }
}
