using DecisionTableLib.Decisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.FormulaAnalyzer
{
    public enum PlusMode
    {
        
        FillEnd,//末尾の要素で埋め合わせ
        FillEven,//同数程度となるように埋め合わせ
    }

    public class RPN
    {
        private readonly PlusMode _plusMode;

        public RPN(PlusMode plusMode = PlusMode.FillEnd)
        {
            this._plusMode = plusMode;
        }

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
                    foreach (List<(string, string)> existingTestCase in left)
                    {
                        foreach (List<(string, string)> multipliedTestCase in right)
                        {
                            product.Add(existingTestCase.Concat(multipliedTestCase).ToList());
                        }
                    }

                    // スタックに新しい組み合わせを追加
                    stack.Push(product);
                }
                else if (token == "+")
                {
                    //足し算とは、ケース数が大きい方のケース集合に、
                    //ケース数が少ない方のケース集合を溶け込ませること
                    
                    var right = stack.Pop();
                    var left = stack.Pop();

                    //以下は左の方が多い前提
                    //左のケース集合に、右のケース集合を溶け込ませる
                    //例) 左 [[("OS", "Windows")], [("OS", "Mac")], [("OS", "Linux")]]
                    //右 [[("Language", "Japanese")], [("Language", "English")]]のとき
                    //[[("OS", "Windows"), ("Language", "Japanese")], [("OS", "Mac"), ("Language", "Japanese")], [("OS", "Linux"), ("Language", "English")],

                    (left, right) = new Filler().Fill(left, right, _plusMode);

                    var product = new List<List<(string, string)>>();
                    for (int i = 0; i < left.Count; i++)
                    {
                        //左の要素と、右の要素を組み合わせる
                        var newTestCase = new List<(string, string)>();
                        newTestCase.AddRange(left[i]);
                        newTestCase.AddRange(right[i]);
                        product.Add(newTestCase);
                    }
                    stack.Push(product);
                }
                else
                {
                    //水準をテストケースのリストにする( [ [("OS", "Windows")], [("OS", "Mac")], [("OS", "Linux")] ] )
                    var targetFactor = factors.First(f => f.Name == token);
                    var values = targetFactor.Levels
                        .Select(level => new List<(string, string)>() { (targetFactor.Name, level.Name) } )
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
