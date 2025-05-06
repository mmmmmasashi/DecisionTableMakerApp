using DecisionTableMakerApp.View;
using Reactive.Bindings;
using System;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DecisionTableMakerApp.ViewModel
{
    public class ExportInfoWindowViewModel
    {
        public ReactiveProperty<string> InspectionText { get; } = new ReactiveProperty<string>();

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
            InspectionText.Value = Properties.Settings.Default.LastInspection ?? string.Empty;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.LastInspection = InspectionText.Value;
            Properties.Settings.Default.Save();
        }

        private void CloseThisWindow(bool isOk)
        {
            if (isOk)
            {
                (bool isValidText, string errorMessage) = CheckTextFormat();
                if (!isValidText)
                {
                    // エラーメッセージを表示
                    System.Windows.MessageBox.Show(errorMessage, "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }
            
            SaveSettings();


            var window = System.Windows.Application.Current.Windows.OfType<ExportInfoWindow>().FirstOrDefault();
            if (window != null)
            {
                window.DialogResult = isOk;
                window.Close();
            }
        }

        /// <summary>
        /// InspectionTextはファイル名に使う。TitleはExcelのA1セルとシート名に使う。
        /// 利用可能な文字列かどうかを判断する
        /// </summary>
        private (bool, string) CheckTextFormat()
        {
            string pattern = @"^[^\\/:*?""<>|]+$"; // ファイル名に使えない文字を除外する正規表現
            if (string.IsNullOrWhiteSpace(InspectionText.Value))
            {
                return (false, "空白のフィールドがあります");
            }
            else if (!Regex.IsMatch(InspectionText.Value, pattern))
            {
                return (false, "検査観点にファイル名に使えない文字が入っています");// 不正な文字が含まれている場合
            }

            return (true, ""); // すべてのフィールドが有効な場合
        }
    }
}