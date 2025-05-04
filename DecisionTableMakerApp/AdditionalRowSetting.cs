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
        private readonly Action _updateTableCollback;

        public ReactiveProperty<string> Col1Text { get; }
        public ReactiveProperty<string> Col2Text { get; }
        public ReactiveCommand DeleteCommand { get; }

        public AdditionalRowSetting(
            Action<AdditionalRowSetting> deleteCallBack,//TODO:エレガントな解法あるはず
            Action updateTableCallBack, //TODO:エレガントな解法あるはず
            string col1Text, string col2Text)
        {
            _deleteCallBack = deleteCallBack;
            _updateTableCollback = updateTableCallBack;

            Col1Text = new ReactiveProperty<string>(col1Text);
            Col1Text.Subscribe(_ => _updateTableCollback());

            Col2Text = new ReactiveProperty<string>(col2Text);
            Col2Text.Subscribe(_ => _updateTableCollback());

            DeleteCommand = new ReactiveCommand();
            DeleteCommand.Subscribe(_ =>
            {
                _deleteCallBack(this);
            });
        }

    }
}
