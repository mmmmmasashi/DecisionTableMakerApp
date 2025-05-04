using DecisionTableLib.Decisions;
using DecisionTableLib.Excel;
using DecisionTableLib.Trees;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {
        public ObservableCollection<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public ReactiveCommand ImportTableCommand { get; }
        public ReactiveCommand CreateDecisionTableCommand { get; }

        public ReactiveProperty<string> FormulaText { get; set; } = new ReactiveProperty<string>("");

        public MainWindowViewModel()
        {
            ImportTableCommand = new ReactiveCommand();
            ImportTableCommand.Subscribe(_ => ExecuteSampleCommand());

            CreateDecisionTableCommand = new ReactiveCommand();
            CreateDecisionTableCommand.Subscribe(_ => CreateDecisionTable());

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();

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

            //サンプル
            //string sampleText = "OS\tWindows\r\n\tMac\r\n\tLinux\r\nLanguage\tJapanese\r\n\tEnglish\r\n\tChinese";

            var excelRange = new ExcelRange(text);

            FactorAndLevelTreeItems.Clear();
            FactorAndLevelTreeItems.Add(excelRange.ToTree());
        }
    }
}
