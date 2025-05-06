using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib
{
    internal class PropertyList
    {
        internal List<(string, string)> FromPropertyString(string originalText)
        {
            if (originalText == string.Empty) return new List<(string, string)>() { ("", "") };

            // 文字列を'|'で分割
            var properties = new Queue<string>(originalText.Split('|'));

            
            // 2つずつペアにしてリストに追加
            var propertyList = new List<(string, string)>();
            while (properties.Count > 0)
            {
                // 2つの要素を取得
                var first = properties.Dequeue();
                if (properties.Count <= 0) throw new InvalidDataException("偶数でない要素数");
                var second = properties.Dequeue();

                // ペアをリストに追加
                propertyList.Add((first.ToString(), second.ToString()));
            }

            return propertyList;

        }

        internal string ToPropertyString(List<(string, string)> settings)
        {
            const string Separator = "|";
            IEnumerable<string> each = settings.Select(setting => $"{setting.Item1}{Separator}{setting.Item2}");
            return string.Join(Separator, each);
        }
    }
}
