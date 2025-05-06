using DecisionTableMakerApp.View;
using Reactive.Bindings;
using System.Windows.Input;

namespace DecisionTableMakerApp.ViewModel
{
    internal class MessageWithFigWindowViewModel
    {
        // メッセージと画像パスのプロパティ
        public ReactiveProperty<string> Message { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> ImagePath { get; } = new ReactiveProperty<string>();

        // OKボタンのコマンド
        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();

        // ダイアログの結果
        public bool? DialogResult { get; private set; }

        public MessageWithFigWindowViewModel()
        {
            // OKボタンの動作
            OKCommand.Subscribe(_ =>
            {
                DialogResult = true;
                CloseThisWIndow();
            });
        }

        // ウィンドウを閉じるためのアクション

        private void CloseThisWIndow()
        {
            var window = System.Windows.Application.Current.Windows.OfType<MessageWithFigWindow>().FirstOrDefault();
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}