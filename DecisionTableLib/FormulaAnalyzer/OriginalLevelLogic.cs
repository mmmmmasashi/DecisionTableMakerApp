using DecisionTableLib.Decisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.FormulaAnalyzer
{
    internal class OriginalLevelLogic
    {
        /// <summary>
        /// [Language(C#,Java)]のように、因子水準表を上書きもしくは独自定義しているケース
        /// この場合は、1.因子名の更新, 2.独自因子-水準の抽出を行う
        /// </summary>
        /// <param name="rpn">更新された逆ポーランド記法のトークン集。( )を除去したもの</param>
        /// <param name="factors">今回定義された因子-水準のリスト</param>"
        internal (List<string> rpnNew, List<Factor> factors) OverwriteByOriginalLevels(List<string> rpn)
        {
            var rpnNew = new List<string>();
            var factorsNew = new List<Factor>();

            foreach (var rpnElement in rpn)
            {

                //(と)要素内に保持しているとき
                bool leftBracket = rpnElement.Contains("(");
                bool rightBracket = rpnElement.Contains(")");

                //一方のみtrueの場合は例外
                if (leftBracket && !rightBracket)
                {
                    throw new InvalidDataException("カッコで閉じていません");
                }
                else if (!leftBracket && rightBracket)
                {
                    throw new InvalidDataException("カッコの片方しか無い");
                }

                if (leftBracket && rightBracket)
                {
                    //TODO: "("が二個以上含まれている場合は例外
                    int idxOfLeftBracket = rpnElement.IndexOf("(");
                    int idxOfRightBracket = rpnElement.IndexOf(")");

                    //TODO:順序例外

                    //()で囲まれた範囲を除去, 半角スペースは除去
                    int bracketAreaCount = idxOfRightBracket - idxOfLeftBracket + 1;
                    var left = rpnElement.Substring(0, idxOfLeftBracket);
                    var bracketContent = rpnElement.Substring(idxOfLeftBracket + 1, bracketAreaCount - 2)
                        .Replace(" ", "");
                    var right = rpnElement.Substring(idxOfRightBracket + 1);

                    //因子名の更新
                    rpnNew.Add(left + right);

                    //独自因子-水準の抽出
                    var levels = bracketContent.Split(',');
                    var factor = new Factor(left, levels.Select(level => new Level(level)).ToList());
                    factorsNew.Add(factor);
                }
                else
                {
                    rpnNew.Add(rpnElement);//そのまま
                }
            }
            return (rpnNew, factorsNew);
        }

        /// <summary>
        /// 因子水準表を上書きする
        /// </summary>
        /// <param name="factorsTmp">書き換え前の因子水準表</param>
        /// <param name="factorOverwriteList">新しく上書きする因子水準表</param>
        internal IEnumerable<Factor> OverwriteFactors(List<Factor> factorsBefore, List<Factor> factorOverwriteList)
        {
            var factorsAfter = new List<Factor>(factorsBefore);

            if (factorOverwriteList.Select(f => f.Name).Distinct().Count() != factorOverwriteList.Count)
            {
                throw new InvalidDataException("独自定義した因子名が重複しています");
            }

            foreach (var factorOverwrite in factorOverwriteList)
            {
                //因子名が重複している場合は事前に削除
                if (factorsAfter.Any(f => f.Name == factorOverwrite.Name))
                {
                    factorsAfter.Remove(factorsAfter.First(f => f.Name == factorOverwrite.Name));
                }

                //上書き
                factorsAfter.Add(factorOverwrite);
            }

            return factorsAfter;
        }
    }
}
