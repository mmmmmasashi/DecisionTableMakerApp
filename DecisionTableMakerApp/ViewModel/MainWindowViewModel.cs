using DecisionTableLib.Decisions;
using DecisionTableLib.Excel;
using DecisionTableLib.Format;
using DecisionTableLib.FormulaAnalyzer;
using DecisionTableLib.Trees;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {
        public ObservableCollection<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public ObservableCollection<AdditionalRowSetting> AdditionalRowSettings { get; private set; } = new ObservableCollection<AdditionalRowSetting>();
        public ReactiveCommand ImportTableCommand { get; }
        public ReactiveCommand AddAdditionalRowCommand { get; }
        public ReactiveCommand CreateDecisionTableCommand { get; }

        public ReactiveProperty<string> FormulaText { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> ParsedResultText { get; set; } = new ReactiveProperty<string>("");

        public ReactiveProperty<DataTable> DecisionTable { get; set; } = new ReactiveProperty<DataTable>(new DataTable());

        private DecisionTableMaker _decisionTableMaker;

        private void DeleteAdditionalRowSetting(AdditionalRowSetting targetSetting)
        {
            AdditionalRowSettings.Remove(targetSetting);
        }

        private void AddNewRowSetting(string col1Text, string col2Text)
        {
            var newRowSetting = new AdditionalRowSetting(DeleteAdditionalRowSetting, UpdateTable, "", "");
            AdditionalRowSettings.Add(newRowSetting);
        }

        public MainWindowViewModel()
        {
            AddAdditionalRowCommand = new ReactiveCommand();
            AddAdditionalRowCommand.Subscribe(_ => AddNewRowSetting("", ""));
            AdditionalRowSettings = new ObservableCollection<AdditionalRowSetting>();
            AddNewRowSetting("結果", "実施日");
            AddNewRowSetting("", "実施者");

            AdditionalRowSettings.CollectionChanged += (sender, args) => UpdateTable();

            ImportTableCommand = new ReactiveCommand();
            ImportTableCommand.Subscribe(_ => ExecuteSampleCommand());

            CreateDecisionTableCommand = new ReactiveCommand();
            CreateDecisionTableCommand.Subscribe(_ => CreateDecisionTable());

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();
            _decisionTableMaker = DecisionTableMaker.EmptyTableMaker;
            FormulaText.Subscribe(text => UpdateTable());

            // 前回保存されたテキストを読み込む
            var lastText = Properties.Settings.Default.LastFactorLevelText;
            if (!string.IsNullOrEmpty(lastText))
            {
                LoadFactorLevelTable(lastText);
            }
        }

        private void UpdateTable()
        {
            if (_decisionTableMaker == null) return;
            var text = FormulaText.Value;
            try
            {
                var decisionTable = _decisionTableMaker.CreateFrom(text);
                ParsedResultText.Value = decisionTable.ToString();
                DecisionTable.Value = new DecisionTableFormatter(decisionTable).ToDataTable()
                    .FormatToAppView(AdditionalRowSettings);
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

        private void ExecuteSampleCommand()
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
        }

        private void LoadFactorLevelTable(string text)
        {
            var excelRange = new ExcelRange(text);

            FactorAndLevelTreeItems.Clear();
            var rootNode = excelRange.ToTree();
            FactorAndLevelTreeItems.Add(rootNode);

            if (rootNode == null) return;
            try
            {
                _decisionTableMaker = new DecisionTableMaker(new FactorLevelTable(rootNode), PlusMode.FillEven);

                //次回読み込み用に保存する
                // テキストを保存
                if (Properties.Settings.Default.LastFactorLevelText != text)
                {
                    Properties.Settings.Default.LastFactorLevelText = text;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                _decisionTableMaker = DecisionTableMaker.EmptyTableMaker;
            }
        }
    }

    public static class DataTableExtension
    {
        /// <summary>
        /// 列名を1行目に埋め込む。元々の1行目以降は2行目以降に移動する。
        /// </summary>
        public static DataTable FormatToAppView(this DataTable originalTable, IEnumerable<AdditionalRowSetting> additionalRowSettings)
        {
            var table = IncludeColumnNameToCell(originalTable);
            AddNewRows(table, additionalRowSettings);
            return table;
        }

        private static void AddNewRows(DataTable table, IEnumerable<AdditionalRowSetting> additionalRowSettings)
        {
            foreach (var newRowSetting in additionalRowSettings)
            {
                var newRow = table.NewRow();
                newRow[0] = newRowSetting.Col1Text.Value;
                newRow[1] = newRowSetting.Col2Text.Value;
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
