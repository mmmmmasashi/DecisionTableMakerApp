using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.FormulaAnalyzer
{
    public class RPN
    {
        /// <summary>
        /// 逆ポーランド記法に変換する
        /// </summary>
        /// <remarks>
        /// Shunting Yard Algorithm という有名なアルゴリズムを使用しています。
        /// </remarks>
        internal List<string> ToRPN(List<string> tokens)
        {
            // 出力リストと演算子スタックを初期化
            var output = new List<string>();
            var opStack = new Stack<string>();// 演算子スタック(一時的)

            // 演算子の優先順位を定義
            int Precedence(string op) => op == "*" ? 2 : (op == "+" ? 1 : 0);//*..2, +..1, それ以外は0

            foreach (var token in tokens)
            {
                if (token == "*" || token == "+")
                {
                    // トークンが演算子の場合、スタックのトップの演算子と比較
                    while (opStack.Any() && Precedence(opStack.Peek()) >= Precedence(token))
                    {
                        // スタックのトップの演算子が優先順位が高い場合、出力リストに追加
                        output.Add(opStack.Pop());
                    }
                    opStack.Push(token);
                }
                else
                {
                    // トークンがオペランドの場合、出力リストに追加
                    output.Add(token);
                }
            }

            // スタックに残っている演算子をすべて出力リストに追加
            while (opStack.Any()) output.Add(opStack.Pop());
            return output;
        }


    }
}
