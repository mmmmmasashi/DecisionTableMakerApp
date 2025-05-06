using DecisionTableLib;
using DecisionTableLib.Decisions;
using DecisionTableLib.Excel;
using DecisionTableLib.Format;
using DecisionTableLib.FormulaAnalyzer;
using DecisionTableLib.Trees;
using DecisionTableMakerApp.View;
using ExcelAccessLib;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {
        public ObservableCollection<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public ReactiveCommand ShowOptionSettingCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ImportTableCommand { get; }
        public ReactiveCommand ExportExcelCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ExportMultiSheetExcelCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<string> FormulaText { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> ParsedResultText { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> AuthorText { get; set; } = new ReactiveProperty<string>("");

        public ReactiveProperty<DataTable> DecisionTable { get; set; } = new ReactiveProperty<DataTable>(new DataTable());
        public string Title { get; }
        //設定値
        private IEnumerable<(string, string)> _additionalRowSettings;

        private string _latestFactorLevelTable;
        private readonly bool _isInitialized = false;
        private bool _isIgnoreWhiteSpace = false;

        public MainWindowViewModel()
        {
            //バージョン表示
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Title = $"Decision Table Maker v{version}";
            AuthorText.Value = Properties.Settings.Default.LastAuthor;
            AuthorText.Subscribe(_ => SaveAuthor());

            //設定値を読み込む
            _isIgnoreWhiteSpace = Properties.Settings.Default.LastIsIgnoreWhiteSpace;
            _additionalRowSettings = LoadAdditionalRowSettings();

            ShowOptionSettingCommand.Subscribe(_ =>
            {
                var optionWindow = new OptionSettingWindow();
                optionWindow.Owner = System.Windows.Application.Current.MainWindow;
                optionWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? isChanged = optionWindow.ShowDialog();
                if (isChanged == true)
                {
                    LoadSettings();
                    UpdateTable();
                }
            });

            ExportExcelCommand.Subscribe(ExportPreviewedExcel);
            ExportMultiSheetExcelCommand.Subscribe(ExportMultiSheetExcel);

            ImportTableCommand = new ReactiveCommand();
            ImportTableCommand.Subscribe(_ => ImportFactorAndLevelTableData());

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();
            FactorAndLevelTreeItems.Add(new TreeNode("root"));//これがないと要素観点表無の時に動かない
            FormulaText.Subscribe(text => UpdateTable());

            // 前回保存されたテキストを読み込む
            var lastText = Properties.Settings.Default.LastFactorLevelText;
            if (!string.IsNullOrEmpty(lastText))
            {
                LoadFactorLevelTable(lastText);
            }

            FormulaText.Value = Properties.Settings.Default.LastFormulaText;

            _isInitialized = true;
        }

        private (bool IsOk, string FilePath) ShowInputFilePath(string defaultFileName)
        {

            //出力先となるファイル名をユーザーに入力してもらう
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Save Excel File",
                FileName = $"{defaultFileName}"
            };
            if (saveFileDialog.ShowDialog() != true) return (false, string.Empty);
            return (true, saveFileDialog.FileName);
        }

        /// <summary>
        /// 表示中の決定表をExcelに出力する
        /// </summary>
        private void ExportPreviewedExcel()
        {
            //検査観点・作成者を入力するダイアログを表示
            var inputWindow = new ExportInfoWindow();
            inputWindow.Owner = System.Windows.Application.Current.MainWindow;
            inputWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? isOk = inputWindow.ShowDialog();
            if (isOk != true) return;

            //出力ファイル名を作成
            //{作成時}_検査観点_{検査観点}.xlsx
            //例) 12/24 12:34:56に作成した場合
            //20231224123456_観点_〇〇が××であること.xlsx
            var exportTime = DateTime.Now;
            var defaultFileName = exportTime.ToString("yyyyMMddHHmmss") + "_観点_" + inputWindow.Inspection + ".xlsx";

            (bool success, string fileName) = ShowInputFilePath(defaultFileName);
            if (!success) return;

            //Excelに出力
            try
            {
                var exceptions = ExportExcelSingleSheetFile(AuthorText.Value, inputWindow.Inspection, FormulaText.Value, exportTime, fileName);

                if (exceptions.Count > 0)
                {
                    throw exceptions[0];
                }

                //Excelを開く
                StartExcel(fileName);
            }
            catch (Exception)
            {
                //失敗メッセージを表示
                MessageBox.Show("Excelの出力に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartExcel(string fileName)
        {
            if (!File.Exists(fileName)) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = true
            });
        }

        private List<ExcelSheetCreateException> ExportExcelSingleSheetFile(string author, string inspection, string formula, DateTime exportTime, string fileName)
        {
            var property = new ExcelBookProperty(
                author,
                exportTime
                );

            var sheetPropertyList = new List<ExcelSheetProperty>() { new ExcelSheetProperty(inspection, inspection, formula) };

            return new ExcelFile(CreateNewDecisionTable, property, sheetPropertyList).Export(fileName);
        }


        /// <summary>
        /// 検査観点,計算式を書いたExcelの範囲コピーを読み込んで、決定表を作成する
        /// Excelファイルは1つで、シート数が複数
        /// </summary>
        private void ExportMultiSheetExcel()
        {
            //Excelの範囲をコピーしたテキストかチェック
            var text = Clipboard.GetText();
            var checkResult = ExcelRange.CheckIfExcelCopiedText(text);
            if (!checkResult.IsOk)
            {
                var errMessage = $"{checkResult.ErrorMsg}\n" + "観点と計算式の2列をクリップボードにコピーしてから実行してください。";

                var window = new MessageWithFigWindow(errMessage, "pack://application:,,,/Assets/example_kanten_keisanshiki.png");
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
                return;
            }

            var range = new ExcelRange(text);
            List<(string Inspection, string Formula)> inspectionAndFormulaPairList = range.ToInspectionAndFormulaList();

            var exportTime = DateTime.Now;

            var excelProperty = new ExcelBookProperty(
                AuthorText.Value,
                exportTime
            );

            //出力ファイル名を作成
            var defaultFileName = exportTime.ToString("yyyyMMddHHmmss") + "_ディシジョンテーブル.xlsx";

            (bool success, string fileName) = ShowInputFilePath(defaultFileName);
            if (!success) return;

            try
            {
                int sheetNumber = 1;
                var sheetProperties = inspectionAndFormulaPairList.Select(pair =>
                {
                    var sheetName = $"No.{sheetNumber++}_{pair.Inspection}";
                    return new ExcelSheetProperty(sheetName, pair.Inspection, pair.Formula);
                }).ToList();

                var exceptions = new ExcelFile(CreateNewDecisionTable, excelProperty, sheetProperties).Export(fileName);
                if (exceptions.Count > 0)
                {
                    //エラーがあった場合はその一覧を表示
                    var errorMsg = new StringBuilder();

                    errorMsg.AppendLine("出力時に以下のエラーが発生しました");
                    foreach (var exception in exceptions)
                    {
                        errorMsg.AppendLine($"番号: {exception.SheetNumber} シート名: {exception.SheetName} エラー内容: {exception.Message}");
                    }
                    MessageBox.Show(errorMsg.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                StartExcel(fileName);
            }
            catch (Exception ex)
            {
                //エラー表示
                MessageBox.Show("Excelの出力に失敗しました。" + Environment.NewLine + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAuthor()
        {
            if (AuthorText.Value != Properties.Settings.Default.LastAuthor)
            {
                Properties.Settings.Default.LastAuthor = AuthorText.Value;
                Properties.Settings.Default.Save();
            }
        }

        private IEnumerable<(string, string)> LoadAdditionalRowSettings()
        {
            return new AdditionalRowProperty().FromProperty(Properties.Settings.Default.LastAdditionalSettings);
        }


        private void SaveAll()
        {
            if (!_isInitialized) return;

            //保存する
            Properties.Settings.Default.LastFormulaText = FormulaText.Value;
            Properties.Settings.Default.LastFactorLevelText = _latestFactorLevelTable;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// TODO: 他の設定値も
        /// </summary>
        private void LoadSettings()
        {
            _isIgnoreWhiteSpace = Properties.Settings.Default.LastIsIgnoreWhiteSpace;
            _additionalRowSettings = LoadAdditionalRowSettings();
        }

        DataTable CreateNewDecisionTable(string formula)
        {
            var rootNode = FactorAndLevelTreeItems.FirstOrDefault();

            DecisionTableMaker decisionTableMaker = new DecisionTableMaker(
                new FactorLevelTable(rootNode), PlusMode.FillEven, _isIgnoreWhiteSpace);
            var decisionTable = decisionTableMaker.CreateFrom(formula);
            ParsedResultText.Value = decisionTable.ToString();
            return new DecisionTableFormatter(decisionTable).ToDataTable().FormatToAppView(_additionalRowSettings);

        }

        private void UpdateTable()
        {
            if (FactorAndLevelTreeItems == null) return;

            bool updateSuccess = false;
            try
            {
                var formula = FormulaText.Value;
                DecisionTable.Value = CreateNewDecisionTable(formula);
                updateSuccess = true;
            }
            catch (Exception ex)
            {
                //異常な文字列を入力した場合は、解析エラーであることだけ通知
                ParsedResultText.Value = "解析エラー" + Environment.NewLine + ex.Message;
            }

            if (updateSuccess) SaveAll();
        }

        private void ImportFactorAndLevelTableData()
        {
            var text = Clipboard.GetText();
            var checkResult = ExcelRange.CheckIfExcelCopiedText(text);
            if (!checkResult.IsOk)
            {
                MessageBox.Show(checkResult.ErrorMsg, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadFactorLevelTable(text);
            SaveAll();
        }

        private void LoadFactorLevelTable(string text)
        {
            var excelRange = new ExcelRange(text);

            FactorAndLevelTreeItems.Clear();
            var rootNode = excelRange.ToTree();
            FactorAndLevelTreeItems.Add(rootNode);

            _latestFactorLevelTable = text;
        }
    }

    public static class DataTableExtension
    {
        /// <summary>
        /// 列名を1行目に埋め込む。元々の1行目以降は2行目以降に移動する。
        /// </summary>
        public static DataTable FormatToAppView(this DataTable originalTable, IEnumerable<(string, string)> additionalRowSettings)
        {
            var table = IncludeColumnNameToCell(originalTable);
            AddNewRows(table, additionalRowSettings);
            return table;
        }

        private static void AddNewRows(DataTable table, IEnumerable<(string, string)> additionalRowSettings)
        {
            foreach (var newRowSetting in additionalRowSettings)
            {
                var newRow = table.NewRow();
                newRow[0] = newRowSetting.Item1;
                newRow[1] = newRowSetting.Item2;
                table.Rows.Add(newRow);
            }
        }

        private static DataTable IncludeColumnNameToCell(DataTable originalTable)
        {
            // 新しいDataTable（列名もデータとして含めたい）
            DataTable withHeaderAsData = new DataTable();

            // 列構造を同じにする
            foreach (DataColumn col in originalTable.Columns)
            {
                withHeaderAsData.Columns.Add(col.ColumnName);
            }

            // 列名を1行目のデータとして追加
            DataRow headerRow = withHeaderAsData.NewRow();
            for (int i = 0; i < originalTable.Columns.Count; i++)
            {
                headerRow[i] = originalTable.Columns[i].ColumnName;
            }
            withHeaderAsData.Rows.Add(headerRow);

            // 元のデータも追加
            foreach (DataRow row in originalTable.Rows)
            {
                withHeaderAsData.Rows.Add(row.ItemArray);
            }

            return withHeaderAsData;
        }
    }
}
