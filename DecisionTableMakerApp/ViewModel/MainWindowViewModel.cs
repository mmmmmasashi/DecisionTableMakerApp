using DecisionTableLib.Excel;
using DecisionTableLib.Trees;
using Reactive.Bindings;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {
        public List<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public ReactiveCommand SampleCommand { get; }

        public MainWindowViewModel()
        {
            // コマンドの初期化
            SampleCommand = new ReactiveCommand();
            SampleCommand.Subscribe(_ => ExecuteSampleCommand());

            string sampleText = "OS\tWindows\r\n\tMac\r\n\tLinux\r\nLanguage\tJapanese\r\n\tEnglish\r\n\tChinese";
            var excelRange = new ExcelRange(sampleText);
            FactorAndLevelTreeItems = new List<TreeNode>() { excelRange.ToTree()};

        }

        private void ExecuteSampleCommand()
        {
            // クリップボードにデータがあるか確認
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();

                // タブと改行でセルと行を分割
                string[] rows = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string row in rows)
                {
                    string[] cells = row.Split('\t');
                    Trace.WriteLine(string.Join(" | ", cells));
                }
            }
            else
            {
                Trace.WriteLine("テキスト形式のクリップボードデータが見つかりません。");
            }
        }
    }
}
