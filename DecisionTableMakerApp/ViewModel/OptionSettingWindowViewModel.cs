using DecisionTableLib;
using DecisionTableMakerApp.View;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionTableMakerApp.ViewModel
{
    public class OptionSettingWindowViewModel
    {

        public ReactiveProperty<bool> IsIgnoreWhiteSpace { get; } = new ReactiveProperty<bool>();
        public ObservableCollection<AdditionalRowSetting> AdditionalRowSettings { get; private set; } = new ObservableCollection<AdditionalRowSetting>();

        public ReactiveCommand AddAdditionalRowCommand { get; } = new ReactiveCommand();
        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();
        public ReactiveCommand CancelCommand { get; } = new ReactiveCommand();

        public OptionSettingWindowViewModel()
        {
            LoadSettings();

            AddAdditionalRowCommand.Subscribe(_ => AddNewRowSetting("", ""));

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


        //TODO : MainWIndowViewModel からのコピペになってる
        private IEnumerable<(string, string)> LoadAdditionalRowSettings()
        {
            string settingStr = Properties.Settings.Default.LastAdditionalSettings;
            settingStr ??= "結果|実施日||実施者||結果"; // デフォルト値

            return new PropertyList()
                .FromPropertyString(settingStr)
                .ToList();
        }

        private void DeleteAdditionalRowSetting(AdditionalRowSetting targetSetting)
        {
            AdditionalRowSettings.Remove(targetSetting);
        }


        private void LoadSettings()
        {
            //空白を無視するかどうかの設定を読み込む
            IsIgnoreWhiteSpace.Value = Properties.Settings.Default.LastIsIgnoreWhiteSpace;

            //行設定を読み込む
            var rowSettings = LoadAdditionalRowSettings();
            AdditionalRowSettings.Clear();
            foreach (var rowSetting in rowSettings)
            {
                AddNewRowSetting(rowSetting.Item1, rowSetting.Item2);
            }
        }

        private void AddNewRowSetting(string col1Text, string col2Text)
        {
            var newRowSetting = new AdditionalRowSetting(DeleteAdditionalRowSetting, col1Text, col2Text);
            AdditionalRowSettings.Add(newRowSetting);
        }

        private bool IsChanged()
        {
            if (Properties.Settings.Default.LastIsIgnoreWhiteSpace != IsIgnoreWhiteSpace.Value) return true;

            var str = AdditionalRowSettings.Select(setting => (setting.Col1Text.Value, setting.Col2Text.Value)).ToList();
            if (Properties.Settings.Default.LastAdditionalSettings != new PropertyList().ToPropertyString(str)) return true;

            return false;
        }

        private void SaveAll()
        {
            //保存する
            var str = AdditionalRowSettings.Select(setting => (setting.Col1Text.Value, setting.Col2Text.Value)).ToList();
            Properties.Settings.Default.LastAdditionalSettings = new PropertyList().ToPropertyString(str);

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
