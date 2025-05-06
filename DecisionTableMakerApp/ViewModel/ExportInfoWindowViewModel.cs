using DecisionTableMakerApp.View;
using Reactive.Bindings;
using System;
using System.Windows.Input;

namespace DecisionTableMakerApp.ViewModel
{
    public class ExportInfoWindowViewModel
    {
        // 検査観点と作成者のプロパティ
        public ReactiveProperty<string> TitleText { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> InspectionText { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> AuthorText { get; } = new ReactiveProperty<string>();

        // OKとキャンセルのコマンド
        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();
        public ReactiveCommand CancelCommand { get; } = new ReactiveCommand();

        // ダイアログの結果
        public bool? DialogResult { get; private set; }

        public ExportInfoWindowViewModel()
        {
            LoadSettings();

            // OKボタンの動作
            OKCommand.Subscribe(_ =>
            {
                DialogResult = true; // OKを選択
                CloseThisWindow(true);
            });

            // キャンセルボタンの動作
            CancelCommand.Subscribe(_ =>
            {
                DialogResult = false; // キャンセルを選択
                CloseThisWindow(false);
            });
        }

        private void LoadSettings()
        {
            TitleText.Value = Properties.Settings.Default.LastExcelTitle ?? string.Empty;
            InspectionText.Value = Properties.Settings.Default.LastInspection ?? string.Empty;
            AuthorText.Value = Properties.Settings.Default.LastAuthor ?? string.Empty;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.LastExcelTitle = TitleText.Value;
            Properties.Settings.Default.LastInspection = InspectionText.Value;
            Properties.Settings.Default.LastAuthor = AuthorText.Value;
            Properties.Settings.Default.Save();
        }

        private void CloseThisWindow(bool isOk)
        {
            if (isOk) SaveSettings();

            var window = System.Windows.Application.Current.Windows.OfType<ExportInfoWindow>().FirstOrDefault();
            if (window != null)
            {
                window.DialogResult = isOk;
                window.Close();
            }
        }
    }
}