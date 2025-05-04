using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.FormulaAnalyzer
{
    internal class Filler
    {
        /// <summary>
        /// 指定されたリストを、目標とする要素数に合わせて埋める
        /// </summary>
        internal List<T> Fill<T>(List<T> list, int targetCount, PlusMode plusMode)
        {
            if (targetCount <= list.Count) return list;

            switch (plusMode)
            {
                case PlusMode.FillEnd:
                    return FillEnd(list, targetCount);
                case PlusMode.FillEven:
                    return FillEven(list, targetCount);
                default:
                    throw new ArgumentOutOfRangeException(nameof(plusMode), plusMode, null);
            }
        }


        /// <summary>
        /// listの要素数が、targetCountで指定された数になるように、要素を複製して埋める。
        /// 複製の仕方は、各要素数が均等になるようにする。
        /// </summary>
        /// <example>
        /// list = ["A", "B", "C"]で、targetCount = 7の場合
        /// 戻り値は["A", "A", "B", "B", "C", "C", "C"]となる
        /// </example>
        private List<T> FillEven<T>(List<T> list, int targetCount)
        {
            // 1要素あたりの目標数
            int countToAddPerElem = targetCount / list.Count;

            var newList = new List<T>();
            foreach (var item in list)
            {
                for (int i = 0; i < countToAddPerElem; i++)
                {
                    newList.Add(item);
                }
            }

            // 不足数
            int remainder = targetCount - newList.Count;
            for (int i = 0; i < remainder; i++)
            {
                newList.Add(list.Last());
            }
            return newList;
        }

        /// <summary>
        /// PlusMode.FillEnd のロジックを処理する
        /// </summary>
        private List<T> FillEnd<T>(List<T> list, int targetCount)
        {
            var newList = new List<T>(list);

            int lengthDiff = targetCount - list.Count;
            for (int i = 0; i < lengthDiff; i++)
            {
                // 一旦、末尾の要素で補う
                newList.Add(list.Last());
            }

            return newList;
        }
    }
}
