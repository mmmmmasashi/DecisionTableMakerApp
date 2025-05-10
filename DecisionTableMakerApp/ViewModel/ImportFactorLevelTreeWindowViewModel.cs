using DecisionTableLib.Excel;
using DecisionTableLib.Trees;
using DecisionTableMakerApp.View;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    public class ImportFactorLevelTreeWindowViewModel
    {
        public ObservableCollection<TreeNode> FactorAndLevelTreeItems { get; private set; }
        public string Message { get; }
        public string Col1Name { get; }
        public string Col2Name { get; }

        public ReactiveCommand OkCommand { get; } = new ReactiveCommand();
        public ReactiveCommand CancelCommand { get; } = new ReactiveCommand();
        public ObservableCollection<ExcelRow> Rows { get; set; }
        public TreeNode? RootNode { get; private set; }

        public ImportFactorLevelTreeWindowViewModel(string message, string col1Name, string col2Name)
        {
            RootNode = null;
            Message = message;
            Col1Name = col1Name;
            Col2Name = col2Name;

            FactorAndLevelTreeItems = new ObservableCollection<TreeNode>();

            Rows = new ObservableCollection<ExcelRow>();
            for (int i = 0; i < 200; i++)
            {
                Rows.Add(new ExcelRow("", ""));
            }
            OkCommand.Subscribe(_ => CloseWindow(true));
            CancelCommand.Subscribe(_ => CloseWindow(false));

            Rows.CollectionChanged += (_, __) => UpdateTreeView();
        }

        private void CloseWindow(bool isOk)
        {
            if (isOk)
            {
                if (RootNode == null)
                {
                    MessageBox.Show("因子水準表が不正です。正しく入力してから終了してください。");
                    return;
                }
            }

            var window = System.Windows.Application.Current.Windows.OfType<ImportFactorLevelTreeWindow>().FirstOrDefault();
            if (window != null)
            {
                window.DialogResult = isOk;
                window.Close();
            }
        }

        internal void SetRows(IEnumerable<ExcelRow> newRows)
        {
            Rows.Clear();
            foreach (var row in newRows)
            {
                Rows.Add(row);
            }
        }

        internal void UpdateTreeView()
        {
            RootNode = null;
            var text = string.Join(Environment.NewLine, Rows.Select(row => row.ToLine()));
            var checkResult = ExcelRange.CheckIfExcelCopiedText(text);
            if (!checkResult.IsOk)
            {
                FactorAndLevelTreeItems.Clear();
                FactorAndLevelTreeItems.Add(new TreeNode("ERROR"));
                return;
            }

            var excelRange = new ExcelRange(text);

            FactorAndLevelTreeItems.Clear();
            RootNode = excelRange.ToTree();
            FactorAndLevelTreeItems.Add(RootNode);
        }
    }
}
