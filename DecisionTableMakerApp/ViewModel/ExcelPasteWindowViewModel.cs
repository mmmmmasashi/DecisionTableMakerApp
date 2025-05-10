using DecisionTableLib.Excel;
using DecisionTableMakerApp.View;
using DocumentFormat.OpenXml.Spreadsheet;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    public record ExcelRow
    {
        public string Field1 { get; set;}
        public string Field2 { get; set; }
        public ExcelRow(string field1, string field2)
        {
            Field1 = field1;
            Field2 = field2;
        }

        internal string ToLine()
        {
            return $"{Field1}\t{Field2}";
        }
    }

    public class ExcelPasteWindowViewModel
    {
        public string Message { get; }

        public ReactiveCommand OkCommand { get; } = new ReactiveCommand();
        public ReactiveCommand CancelCommand { get; } = new ReactiveCommand();
        public ObservableCollection<ExcelRow> Rows { get; set; }

        public ExcelPasteWindowViewModel(string message)
        {
            Message = message;

            Rows = new ObservableCollection<ExcelRow>();
            for (int i = 0; i < 200; i++)
            {
                Rows.Add(new ExcelRow("", ""));
            }

            OkCommand.Subscribe(_ => CloseWindow(true));
            CancelCommand.Subscribe(_ => CloseWindow(false));
        }

        private void CloseWindow(bool isOk)
        {
            foreach (var row in Rows)
            {
                if (!string.IsNullOrEmpty(row.ToString()))
                {
                    Trace.WriteLine(row.ToString());
                }
            }

            var window = System.Windows.Application.Current.Windows.OfType<ExcelPasteWindow>().FirstOrDefault();
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
    }
}
