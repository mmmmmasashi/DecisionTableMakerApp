using Reactive.Bindings;
using System;
using System.Reactive.Linq;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MainWindowViewModel
    {
        public ReactiveCommand SampleCommand { get; }

        public MainWindowViewModel()
        {
            // コマンドの初期化
            SampleCommand = new ReactiveCommand();
            SampleCommand.Subscribe(_ => ExecuteSampleCommand());
        }

        private void ExecuteSampleCommand()
        {
            // コマンド実行時の処理
            System.Windows.MessageBox.Show("ReactiveCommand executed!");
        }
    }
}
