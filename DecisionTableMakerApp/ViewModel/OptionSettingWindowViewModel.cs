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
        private readonly int RandomSearchMinNum = 1;

        public ReactiveProperty<string> RandomSearchNum { get; } = new ReactiveProperty<string>("10");
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

                if (!int.TryParse(RandomSearchNum.Value, out int randomSearchNum))
                {
                    MessageBox.Show("試行回数は数値で入力してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (randomSearchNum < RandomSearchMinNum)
                {
                    MessageBox.Show($"試行回数は{RandomSearchMinNum}以上の値を入力してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("設定を保存しますか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result != MessageBoxResult.OK) return;

                SaveAll();
                CloseThisWindow(true);
            });
        }


        private void DeleteAdditionalRowSetting(AdditionalRowSetting targetSetting)
        {
            AdditionalRowSettings.Remove(targetSetting);
        }


        private void LoadSettings()
        {
            //空白を無視するかどうかの設定を読み込む
            IsIgnoreWhiteSpace.Value = Properties.Settings.Default.LastIsIgnoreWhiteSpace;

            //試行回数の設定を読み込む
            RandomSearchNum.Value = "100";
            if (int.TryParse(Properties.Settings.Default.RandomSearchNum, out int randomSearchNum))
            {
                if (randomSearchNum >= RandomSearchMinNum)
                {
                    RandomSearchNum.Value = Properties.Settings.Default.RandomSearchNum;
                }
            }

            //行設定を読み込む
            var rowSettings = new AdditionalRowProperty().FromProperty(Properties.Settings.Default.LastAdditionalSettings);
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
            //試行回数が変更されているか
            if (Properties.Settings.Default.RandomSearchNum != RandomSearchNum.Value) return true;
            if (Properties.Settings.Default.LastIsIgnoreWhiteSpace != IsIgnoreWhiteSpace.Value) return true;

            var str = AdditionalRowSettings.Select(setting => (setting.Col1Text.Value, setting.Col2Text.Value)).ToList();
            if (Properties.Settings.Default.LastAdditionalSettings != new AdditionalRowProperty().ToPropertyString(str)) return true;

            return false;
        }

        private void SaveAll()
        {
            //保存する
            var str = AdditionalRowSettings.Select(setting => (setting.Col1Text.Value, setting.Col2Text.Value)).ToList();
            Properties.Settings.Default.LastAdditionalSettings = new AdditionalRowProperty().ToPropertyString(str);
            Properties.Settings.Default.RandomSearchNum = RandomSearchNum.Value;
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
