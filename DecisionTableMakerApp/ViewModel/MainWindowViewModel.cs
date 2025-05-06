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
using System.Reactive.Linq;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {
        public ObservableCollection<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public ReactiveCommand ShowOptionSettingCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ImportTableCommand { get; }
        public ReactiveCommand CreateDecisionTableCommand { get; }
        public ReactiveCommand ExportExcelCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<string> FormulaText { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> ParsedResultText { get; set; } = new ReactiveProperty<string>("");

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

            ExportExcelCommand.Subscribe(_ =>
            {
                //検査観点・作成者を入力するダイアログを表示
                var inputWindow = new ExportInfoWindow();
                inputWindow.Owner = System.Windows.Application.Current.MainWindow;
                inputWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? isOk = inputWindow.ShowDialog();
                if (isOk != true) return;

                //出力先となるファイル名をユーザーに入力してもらう
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Save Excel File",
                    FileName = "DecisionTable.xlsx"
                };
                if (saveFileDialog.ShowDialog() != true) return;
                //選択されたファイル名を取得
                string fileName = saveFileDialog.FileName;

                //Excelに出力
                try
                {
                    var tableToExport = RemoveFirstLineForExport(DecisionTable.Value);
                    var property = new ExcelProperty(
                        "Sheet1",
                        inputWindow.TitleText,
                        inputWindow.Author,
                        new Dictionary<string, string>() {
                            { "検査観点", inputWindow.Inspection },
                            { "計算式", FormulaText.Value }
                        });

                    new ExcelFile(tableToExport, property).Export(fileName);
                    //完了メッセージを表示
                    MessageBox.Show("Excelの出力が完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    //失敗メッセージを表示
                    MessageBox.Show("Excelの出力に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ImportTableCommand = new ReactiveCommand();
            ImportTableCommand.Subscribe(_ => ImportFactorAndLevelTableData());

            CreateDecisionTableCommand = new ReactiveCommand();
            CreateDecisionTableCommand.Subscribe(_ => CreateDecisionTable());

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

        /// <summary>
        /// セルをコピーしたときに範囲に入れるために、列名を1行目に埋め込んでいた。
        /// これはExcel出力する際は邪魔になるので1行目を削除する。その上で上に詰める。
        /// もとのDataTableは変更しない。
        /// </summary>
        private DataTable RemoveFirstLineForExport(DataTable value)
        {
            //出力用DataTbleをコピーして作成
            var exportTable = value.Copy();

            if (exportTable.Rows.Count <= 0) throw new InvalidDataException("ディシジョンテーブルが空です");
            
            //1行目を削除
            exportTable.Rows.RemoveAt(0);

            return exportTable;
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

        private void UpdateTable()
        {
            if (FactorAndLevelTreeItems == null) return;

            var rootNode = FactorAndLevelTreeItems.FirstOrDefault();

            DecisionTableMaker decisionTableMaker = new DecisionTableMaker(new FactorLevelTable(rootNode), PlusMode.FillEven, _isIgnoreWhiteSpace);

            var text = FormulaText.Value;
            try
            {
                var decisionTable = decisionTableMaker.CreateFrom(text);
                ParsedResultText.Value = decisionTable.ToString();
                DecisionTable.Value = new DecisionTableFormatter(decisionTable).ToDataTable()
                    .FormatToAppView(_additionalRowSettings);
                SaveAll();
            }
            catch (Exception ex)
            {
                //異常な文字列を入力した場合は、解析エラーであることだけ通知
                ParsedResultText.Value = "解析エラー" + Environment.NewLine + ex.Message;
            }
        }

        private void CreateDecisionTable()
        {

            if (FactorAndLevelTreeItems.Count() == 0)
            {
                MessageBox.Show("決定表を作成するための因子と水準の情報がありません。");
                return;
            }

            var text = Clipboard.GetText();
            var factorAndLevelRootNode = FactorAndLevelTreeItems.First();

            var factorLevelTable = new FactorLevelTable(factorAndLevelRootNode);
            var maker = new DecisionTableMaker(factorLevelTable);
            DecisionTable decisionTable = maker.CreateFrom(FormulaText.Value);

            string tsvDesitionTable = decisionTable.ToTsv();
            Clipboard.SetText(tsvDesitionTable);

            //完了メッセージ
            MessageBox.Show("決定表を作成しました。クリップボードにコピーしています", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportFactorAndLevelTableData()
        {
            var text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("クリップボードにテキストがありません。Excelのセルを範囲指定してコピーしてからクリックしてください。");
                return;
            }

            if (!(text.Contains("\t") && text.Contains("\r\n")))
            {
                MessageBox.Show("クリップボードのテキストが正しい形式ではありません。Excelのセルを範囲指定してコピーしてからクリックしてください。");
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
