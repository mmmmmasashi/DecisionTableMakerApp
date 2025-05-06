using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableMakerApp
{
    /// <summary>
    /// ディシジョンテーブルの末尾行に追加する行の設定を保持するクラス
    /// </summary>
    public class AdditionalRowSetting
    {
        private readonly Action<AdditionalRowSetting> _deleteCallBack;

        public ReactiveProperty<string> Col1Text { get; }
        public ReactiveProperty<string> Col2Text { get; }
        public ReactiveCommand DeleteCommand { get; }

        public AdditionalRowSetting(
            Action<AdditionalRowSetting> deleteCallBack,//TODO:エレガントな解法あるはず
            string col1Text, string col2Text)
        {
            _deleteCallBack = deleteCallBack;

            Col1Text = new ReactiveProperty<string>(col1Text);
            Col2Text = new ReactiveProperty<string>(col2Text);

            DeleteCommand = new ReactiveCommand();
            DeleteCommand.Subscribe(_ =>
            {
                _deleteCallBack(this);
            });
        }

    }
}
