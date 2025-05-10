using DecisionTableLib;
using DecisionTableLib.Decisions;
using DecisionTableLib.Excel;
using DecisionTableLib.Format;
using DecisionTableLib.FormulaAnalyzer;
using DecisionTableLib.Trees;
using DecisionTableMakerApp.View;
using ExcelAccessLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {

        private Window ThisWindow => System.Windows.Application.Current.MainWindow;
        public ObservableCollection<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public ReactiveCommand RefreshCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ShowOptionSettingCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ImportTableCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ExportExcelCommand { get; } = new ReactiveCommand();
        public ReactiveCommand<Task> ExportMultiSheetExcelCommand { get; } = new ReactiveCommand<Task>();

        public ReactiveProperty<string> UncoveredCountText { get; } = new ReactiveProperty<string>("-");
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
        private int _randomSearchNum;

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
            _randomSearchNum = LoadRandomSearchNum();

            RefreshCommand.Subscribe(UpdateTable);
            ShowOptionSettingCommand.Subscribe(_ =>
            {
                var optionWindow = new OptionSettingWindow();
                optionWindow.Owner = ThisWindow;
                optionWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? isChanged = optionWindow.ShowDialog();
                if (isChanged == true)
                {
                    LoadSettings();
                    UpdateTable();
                }
            });

            ExportExcelCommand.Subscribe(ExportPreviewedExcel);
            ExportMultiSheetExcelCommand.Subscribe(async _ => await ExportMultiSheetExcel());

            ImportTableCommand.Subscribe(ImportFactorAndLevelTableData);

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();
            FactorAndLevelTreeItems.Add(new TreeNode("root"));//これがないと要素観点表無の時に動かない
            FormulaText.Throttle(TimeSpan.FromMilliseconds(300)) // 300ms のデバウンスを適用。期間内の新しいイベント発生時は最後のイベントのみを処理する
                       .ObserveOnUIDispatcher() // UI スレッドで実行
                       .Subscribe(_ => UpdateTable());

            // 前回保存されたテキストを読み込む
            var lastText = Properties.Settings.Default.LastFactorLevelText;
            if (!string.IsNullOrEmpty(lastText))
            {
                LoadFactorLevelTable(lastText);
            }

            FormulaText.Value = Properties.Settings.Default.LastFormulaText;

            _isInitialized = true;
        }

        private int LoadRandomSearchNum()
        {
            const int DefaultVal = 100;
            if (string.IsNullOrEmpty(Properties.Settings.Default.RandomSearchNum)) return DefaultVal;
            if (int.TryParse(Properties.Settings.Default.RandomSearchNum, out int randomSearchNum))
            {
                return randomSearchNum;
            }
            else
            {
                return DefaultVal;
            }
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
                LaunchExcel(fileName);
            }
            catch (Exception)
            {
                //失敗メッセージを表示
                MessageBox.Show("Excelの出力に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchExcel(string fileName)
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

            return new ExcelFile(CreateNewDecisionDataTable, property, sheetPropertyList).Export(fileName);
        }
        DataTable CreateNewDecisionDataTable(string formula)
        {
            var ret = CreateNewDecisionTable(formula);
            return ret.Item1;
        }
        private void ShowMessageBoxWithImage(string msg, string imagePath)
        {
            var window = new MessageWithFigWindow(msg, imagePath);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }

        /// <summary>
        /// 検査観点,計算式を書いたExcelの範囲コピーを読み込んで、決定表を作成する
        /// Excelファイルは1つで、シート数が複数
        /// </summary>
        private async Task ExportMultiSheetExcel()
        {
            var pasteWindow = new ExcelPasteWindow(ThisWindow, "検査観点と計算式を入力してください。Excelで範囲選択しての貼り付けも可能です。", "検査観点", "ケース計算式");
            var pasted = pasteWindow.ShowDialog();
            if (!(pasted ?? false)) return;

            var text = string.Join(Environment.NewLine, pasteWindow.Rows.Select(row => row.ToLine()));

            //必要な情報の取得
            var range = new ExcelRange(text);
            List<(string Inspection, string Formula)> inspectionAndFormulaPairList = range.ToTwoColumnRows();

            //出力ファイル名を作成・ユーザーに保存先を選択してもらう
            var exportTime = DateTime.Now;
            var defaultFileName = exportTime.ToString("yyyyMMddHHmmss") + "_ディシジョンテーブル.xlsx";
            (bool success, string fileName) = ShowInputFilePath(defaultFileName);
            if (!success) return;

            //プログレスダイアログを表示
            var thisWindow = System.Windows.Application.Current.MainWindow;
            var progressWindow = new ProgressWindow(thisWindow, "作成中", "Excelファイルを出力中です。少しお待ちください...");
            progressWindow.Show();

            List<ExcelSheetCreateException> exceptions = new ();

            try
            {
                await Task.Run(() =>
                {
                    exceptions = ExportExcelFile(inspectionAndFormulaPairList, exportTime, fileName);
                });
            }
            catch (Exception ex)
            {
                //エラー表示
                MessageBox.Show("Excelの出力に失敗しました。" + Environment.NewLine + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                //プログレスダイアログを閉じる
                progressWindow.Close();
            }

            if (exceptions.Count > 0)
            {
                //エラーがあった場合はその一覧を表示
                var errMsg = string.Join(Environment.NewLine, exceptions.Select(e => e.OneLineMessage));
                var errorWindow = new MessageAndDetailWindow(thisWindow, "変換時に以下のエラーが発生しました", errMsg);
                errorWindow.ShowDialog();
            }

            //出力したExcelファイルを開きます
            var excelFilePath = new ExcelFilePath(fileName);
            if (excelFilePath.IsExcelFile)
            {
                var excelOpeningWindow = new ProgressWindow(
                    thisWindow, "起動中", "出力したExcelファイルを開いています...");
                excelOpeningWindow.Show();

                try
                {
                    excelFilePath.LaunchExcelWithProcess();

                    //Excelを開くのに時間がかかる場合があるので、少し待つ
                    await Task.Delay(1000);
                }
                finally
                {
                    excelOpeningWindow.Close();
                }
            }
        }

        private List<ExcelSheetCreateException> ExportExcelFile(List<(string Inspection, string Formula)> inspectionAndFormulaPairList, DateTime exportTime, string fileName)
        {
            var sheetProperties = inspectionAndFormulaPairList
                .Select((pair, index) =>
                {
                    var sheetName = $"No.{index + 1}_{pair.Inspection}";
                    return new ExcelSheetProperty(sheetName, pair.Inspection, pair.Formula);
                }).ToList();

            var excelProperty = new ExcelBookProperty(AuthorText.Value, exportTime);
            var exceptions = new ExcelFile(CreateNewDecisionDataTable, excelProperty, sheetProperties).Export(fileName);
            return exceptions;
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

        private void LoadSettings()
        {
            _randomSearchNum = LoadRandomSearchNum();
            _isIgnoreWhiteSpace = Properties.Settings.Default.LastIsIgnoreWhiteSpace;
            _additionalRowSettings = LoadAdditionalRowSettings();
        }

        (DataTable, DecisionTable) CreateNewDecisionTable(string formula)
        {
            var rootNode = FactorAndLevelTreeItems.FirstOrDefault();

            DecisionTableMaker decisionTableMaker = new DecisionTableMaker(
                new FactorLevelTable(rootNode), PlusMode.FillEven, _isIgnoreWhiteSpace, _randomSearchNum);
            var decisionTable = decisionTableMaker.CreateFrom(formula);
            ParsedResultText.Value = decisionTable.ToString();
            return (new DecisionTableFormatter(decisionTable).ToDataTable().FormatToAppView(_additionalRowSettings), decisionTable);
        }

        private void UpdateTable()
        {
            if (FactorAndLevelTreeItems == null) return;

            bool updateSuccess = false;
            try
            {
                var formula = FormulaText.Value;
                (DecisionTable.Value, DecisionTable decitionTable) = CreateNewDecisionTable(formula);

                //未網羅の組み合わせ数を表示
                UncoveredCountText.Value = CalcUncoverdPairNumNoError(decitionTable);

                updateSuccess = true;
            }
            catch (Exception ex)
            {
                //異常な文字列を入力した場合は、解析エラーであることだけ通知
                ParsedResultText.Value = "解析エラー:" + ex.Message;
            }

            if (updateSuccess) SaveAll();
        }

        private string CalcUncoverdPairNumNoError(DecisionTable decitionTable)
        {
            try
            {
                var result = new TestCaseOptimizer().CalcUncoverdPairNum(decitionTable);
                return result.Score.ToString();
            }
            catch (Exception)
            {
                //この補助的な機能では、エラーで死なせない
                return "Error";
            }
        }

        private void ImportFactorAndLevelTableData()
        {
            var window = new ExcelPasteWindow(ThisWindow, "因子と水準を入力してください。", "因子", "水準");
            var result = window.ShowDialog();
            if (result != true) return;

            var text = string.Join(Environment.NewLine, window.Rows.Select(row => row.ToLine()));
            var checkResult = ExcelRange.CheckIfExcelCopiedText(text);
            if (!checkResult.IsOk)
            {
                MessageBox.Show("解釈に失敗しました。因子と水準の2列をクリップボードにコピーしてから実行してください。", "エラー");
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
