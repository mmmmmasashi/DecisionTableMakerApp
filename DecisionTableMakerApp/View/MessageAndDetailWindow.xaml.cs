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
    /// MessageAndDetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageAndDetailWindow : Window
    {
        public string Message { get; }
        public string Detail { get; }
        public ReactiveCommand OkCommand { get; } = new ReactiveCommand();

        public MessageAndDetailWindow(Window owner, string message, string detail)
        {
            this.Owner = owner;
            this.Message = message;
            this.Detail = detail;

            InitializeComponent();
            this.DataContext = this;

            OkCommand.Subscribe(_ =>
            {
                this.DialogResult = true;
                this.Close();
            });
        }
    }
}
