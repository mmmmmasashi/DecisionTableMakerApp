using DecisionTableLib.Decisions;
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
        /// 逆ポーランド記法を評価する
        /// </summary>
        /// <param name="rpn">逆ポーランド記法で表現された要素集</param>
        /// <param name="factors">因子リスト</param>
        /// <returns></returns>
        internal IEnumerable<IEnumerable<(string, string)>> EvaluateRPN(List<string> rpn, IEnumerable<Factor> factors)
        {

            //参考にしたコードはこう >     static List<List<string>> EvaluateRPN(List<string> rpn, Dictionary<string, List<string>> factors)
            var stack = new Stack<List<List<(string, string)>>>();

            foreach (var token in rpn)
            {
                if (token == "*")
                {
                    // スタックから2つの要素を取り出す
                    //例) 要素1 [[("OS", "Windows")], ([("OS", "Mac")], ([("OS", "Linux")], 要素2 [[("Language", "Japanese")], [("Language", "English")], [("Language", "Chinese")]
                    var right = stack.Pop();
                    var left = stack.Pop();

                    //left, rightの要素を全通り組み合わせる
                    var product = new List<List<(string, string)>>();
                    foreach (List<(string, string)> eachTestCase in left)
                    {
                        foreach (List<(string, string)> newTestCase in right)
                        {
                            product.Add(eachTestCase.Concat(newTestCase).ToList());
                        }
                    }

                    // スタックに新しい組み合わせを追加
                    stack.Push(product);
                }
                else if (token == "+")
                {
                    var right = stack.Pop();
                    var left = stack.Pop();

                    //TODO:この処理は間違っている
                    stack.Push(left.Concat(right).ToList());
                }
                else
                {
                    //水準をテストケースのリストにする( [ [("OS", "Windows")], [("OS", "Mac")], [("OS", "Linux")] ] )
                    var targetFactor = factors.First(f => f.Name == token);
                    var values = targetFactor.Levels
                        .Select(level => new List<(string, string)>() { (targetFactor.Name, level) } )
                        .ToList();

                    //スタックに積む
                    stack.Push(values);
                }
            }
            return stack.Pop();
        }

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
