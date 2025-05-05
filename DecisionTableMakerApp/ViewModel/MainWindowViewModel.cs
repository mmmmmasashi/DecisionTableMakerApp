using DecisionTableLib;
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
using static System.Net.Mime.MediaTypeNames;

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
        public ReactiveProperty<bool> IsIgnoreWhiteSpace { get; }  

        private string _latestFactorLevelTable;
        private readonly bool _isInitialized = false;

        private void DeleteAdditionalRowSetting(AdditionalRowSetting targetSetting)
        {
            AdditionalRowSettings.Remove(targetSetting);
        }

        private void AddNewRowSetting(string col1Text, string col2Text)
        {
            var newRowSetting = new AdditionalRowSetting(DeleteAdditionalRowSetting, UpdateTable, col1Text, col2Text);
            AdditionalRowSettings.Add(newRowSetting);
        }

        public MainWindowViewModel()
        {
            bool isIgnoreWhiteSpace = Properties.Settings.Default.LastIsIgnoreWhiteSpace;
            IsIgnoreWhiteSpace = new ReactiveProperty<bool>(isIgnoreWhiteSpace);
            IsIgnoreWhiteSpace.Subscribe(_ => UpdateIgnoreWhiteSpace());

            AddAdditionalRowCommand = new ReactiveCommand();
            AddAdditionalRowCommand.Subscribe(_ => AddNewRowSetting("", ""));

            InitializeAdditionalRowSettings();

            AdditionalRowSettings.CollectionChanged += (sender, args) => UpdateTable();

            ImportTableCommand = new ReactiveCommand();
            ImportTableCommand.Subscribe(_ => ImportFactorAndLevelTableData());

            CreateDecisionTableCommand = new ReactiveCommand();
            CreateDecisionTableCommand.Subscribe(_ => CreateDecisionTable());

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();
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

        private void UpdateIgnoreWhiteSpace()
        {
            SaveAll();
            UpdateTable();
        }

        private void InitializeAdditionalRowSettings()
        {
            AdditionalRowSettings = new ObservableCollection<AdditionalRowSetting>();

            string settingStr = Properties.Settings.Default.LastAdditionalSettings;
            settingStr ??= "結果|実施日||実施者||結果"; // デフォルト値

            new PropertyList()
                .FromPropertyString(settingStr)
                .ToList()
                .ForEach(each => AddNewRowSetting(each.Item1, each.Item2));
        }


        private void SaveAll()
        {
            if (!_isInitialized) return;

            //保存する
            var str = AdditionalRowSettings.Select(setting => (setting.Col1Text.Value, setting.Col2Text.Value)).ToList();
            Properties.Settings.Default.LastAdditionalSettings = new PropertyList().ToPropertyString(str);
            Properties.Settings.Default.LastFormulaText = FormulaText.Value;
            Properties.Settings.Default.LastFactorLevelText = _latestFactorLevelTable;
            Properties.Settings.Default.LastIsIgnoreWhiteSpace = IsIgnoreWhiteSpace.Value;

            Properties.Settings.Default.Save();
        }

        private void UpdateTable()
        {
            if (FactorAndLevelTreeItems == null) return;
            if (FactorAndLevelTreeItems.Count() == 0) return;

            var rootNode = FactorAndLevelTreeItems.FirstOrDefault();

            DecisionTableMaker decisionTableMaker = new DecisionTableMaker(new FactorLevelTable(rootNode), PlusMode.FillEven, IsIgnoreWhiteSpace.Value);

            var text = FormulaText.Value;
            try
            {
                var decisionTable = decisionTableMaker.CreateFrom(text);
                ParsedResultText.Value = decisionTable.ToString();
                DecisionTable.Value = new DecisionTableFormatter(decisionTable).ToDataTable()
                    .FormatToAppView(AdditionalRowSettings);
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
