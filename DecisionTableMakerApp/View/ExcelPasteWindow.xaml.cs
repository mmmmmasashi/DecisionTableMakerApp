using DecisionTableLib.Excel;
using DecisionTableMakerApp.ViewModel;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DecisionTableMakerApp.View
{
    /// <summary>
    /// ExcelPasteWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExcelPasteWindow : Window
    {
        public List<ExcelRow> Rows { get => _vm.Rows.ToList(); }
        private ExcelPasteWindowViewModel _vm;

        public ExcelPasteWindow(Window owner, string message, string col1Name, string col2Name)
        {
            this.Owner = owner;
            InitializeComponent();
            _vm = new ExcelPasteWindowViewModel(message, col1Name, col2Name);
            this.DataContext = _vm;
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteExcelDataToDataGrid();
                e.Handled = true;
            }
        }

        private void PasteExcelDataToDataGrid()
        {
            //Excelの範囲をコピーしたテキストかチェック
            var text = Clipboard.GetText();
            var checkResult = ExcelRange.CheckIfExcelCopiedText(text);
            if (!checkResult.IsOk)
            {
                MessageBox.Show("観点と計算式の2列をクリップボードにコピーしてから実行してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //必要な情報の取得
            var range = new ExcelRange(text);
            var newRows = range.ToTwoColumnRows().Select(pair => new ExcelRow(pair.Item1, pair.Item2));
            _vm.SetRows(newRows);
        }
    }
}
