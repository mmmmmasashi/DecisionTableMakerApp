using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib
{
    public class PropertyList
    {
        public List<(string, string)> FromPropertyString(string originalText)
        {
            // 文字列を'|'で分割
            var properties = new Queue(originalText.Split('|'));
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
    }
}
