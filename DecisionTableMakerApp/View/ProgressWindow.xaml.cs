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
    /// ProgressWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ReactiveProperty<string> TitleBarText { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Message { get; } = new ReactiveProperty<string>();

        public ProgressWindow(Window owner, string? title = null, string? message = null)
        {
            TitleBarText.Value = title ?? "処理中";
            Message.Value = message ?? "処理中です。少しお待ちください...";

            this.Owner = owner;
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
