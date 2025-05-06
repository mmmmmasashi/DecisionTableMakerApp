using DecisionTableLib;
using DecisionTableMakerApp.View;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    public class OptionSettingWindowViewModel
    {
        public ReactiveProperty<bool> IsIgnoreWhiteSpace { get; } = new ReactiveProperty<bool>();

        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();
        public ReactiveCommand CancelCommand { get; } = new ReactiveCommand();

        public OptionSettingWindowViewModel()
        {
            LoadSettings();
            CancelCommand.Subscribe(_ => CloseThisWindow(false));

            OKCommand.Subscribe(_ =>
            {
                if (!IsChanged())
                {
                    CloseThisWindow(false);
                    return;
                }

                var result = MessageBox.Show("設定を保存しますか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result != MessageBoxResult.OK) return;

                SaveAll();
                CloseThisWindow(true);
            });
        }

        private void LoadSettings()
        {
            IsIgnoreWhiteSpace.Value = Properties.Settings.Default.LastIsIgnoreWhiteSpace;
        }

        private bool IsChanged()
        {
            if (Properties.Settings.Default.LastIsIgnoreWhiteSpace != IsIgnoreWhiteSpace.Value) return true;
            return false;
        }

        private void SaveAll()
        {
            //保存する
            //var str = AdditionalRowSettings.Select(setting => (setting.Col1Text.Value, setting.Col2Text.Value)).ToList();
            //Properties.Settings.Default.LastAdditionalSettings = new PropertyList().ToPropertyString(str);
            Properties.Settings.Default.LastIsIgnoreWhiteSpace = IsIgnoreWhiteSpace.Value;
            Properties.Settings.Default.Save();
        }

        private static void CloseThisWindow(bool isChanged)
        {
            var window = System.Windows.Application.Current.Windows.OfType<OptionSettingWindow>().FirstOrDefault();
            if (window != null)
            {
                window.DialogResult = isChanged;
                window.Close();
            }
        }

    }
}
