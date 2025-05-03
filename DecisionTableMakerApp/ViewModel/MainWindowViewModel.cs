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
        public ReactiveCommand SampleCommand { get; }

        public MainWindowViewModel()
        {
            SampleCommand = new ReactiveCommand();
            SampleCommand.Subscribe(_ => ExecuteSampleCommand());

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();

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
