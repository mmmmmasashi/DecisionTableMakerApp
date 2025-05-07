using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.FormulaAnalyzer
{
    internal class Tokenizer
    {
        internal List<string> Tokenize(string expr)
        {
            var tokens = new List<string>();
            int i = 0;

            try
            {
                while (i < expr.Length)
                {
                    if (char.IsWhiteSpace(expr[i])) { i++; continue; }

                    if (expr[i] == '[')
                    {
                        int j = expr.IndexOf(']', i);
                        tokens.Add(expr.Substring(i + 1, j - i - 1));
                        i = j + 1;
                    }
                    else if (expr[i] == '*' || expr[i] == '+' || expr[i] == '<')
                    {
                        tokens.Add(expr[i].ToString());
                        i++;
                    }
                    else
                    {
                        throw new Exception("Unexpected character in expression");
                    }
                }
                return tokens;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"トークン化エラー({i}文字目):括弧, 演算子等が正しいか確認してください。", ex);
            }

        }
    }
}
