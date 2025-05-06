using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public class TestCaseOptimizer
    {
        /// <summary>
        /// 最適化されてる度合を算出
        /// </summary>
        /// <remarks>
        /// ・全通りの2因子ペアをつくる
        /// ・各2因子の全組み合わせの合計値を出す(理論値Aとする)
        /// ・次に、実際の全TestCaseを対象に、上記の組み合わせの内、含んでいる組み合わせの数を出す(実測値Bとする)
        /// ・理論値A - 実測値B = 最適化スコアとする。 (小さいほどペアワイズの観点で優れており、0は最適化完了)
        /// </remarks>
        public OptimizeResult CalcUncoverdPairNum(DecisionTable table)
        {
            //全因子の列挙
            var factors = table.Factors;

            //2因子の組み合わせ
            var twoFactorCombinations = GetCombinations(factors);

            //理論値A 2つの因子の水準の組み合わせの数を全て足す
            int idealTwoLevelPairNum = twoFactorCombinations.Sum(factorPair => factorPair.Item1.Levels.Count * factorPair.Item2.Levels.Count);

            //理想的にカバーすべき因子水準の2つの全組み合わせ
            var idealAllPairs = ListUpLevelPairs(twoFactorCombinations);
            if (idealAllPairs.Count() != idealTwoLevelPairNum) throw new InvalidProgramException("実装ミス");

            //計測対象のディシジョンテーブルの内、上記の辞書の要素の内、
            //いくつを網羅できているかカウントする
            var testCases = table.TestCases;
            int actualTwoLevelPairNum = CountUpActualTwoLevelPairNum(testCases, idealAllPairs);

            return new OptimizeResult(
                twoFactorCombinations.Count,
                idealTwoLevelPairNum,
                actualTwoLevelPairNum);
        }

        /// <summary>
        /// 実際のテストケースの内、(因子1, 因子2) - (因子1水準, 因子2水準)リストの辞書の要素を網羅している数をカウントする
        /// </summary>
        private int CountUpActualTwoLevelPairNum(
            List<TestCase> testCases,
            List<(FactorLevelPair, FactorLevelPair)> idealAllPairs)
        {
            int count = 0;

            foreach (var twoPairs in idealAllPairs)
            {
                var pair1 = twoPairs.Item1;
                var pair2 = twoPairs.Item2;
                foreach (var testCase in testCases)
                {
                    //テストケースの中に、因子1の水準が含まれているか
                    if (testCase.HasFactorOf(pair1.FactorName) == false) continue;
                    //テストケースの中に、因子2の水準が含まれているか
                    if (testCase.HasFactorOf(pair2.FactorName) == false) continue;
                    //水準が一致しているか
                    if (testCase.LevelOf(pair1.FactorName).Name.Equals(pair1.LevelName) &&
                        testCase.LevelOf(pair2.FactorName).Name.Equals(pair2.LevelName))
                    {
                        count++;
                        break; //このペアはカウントしたので、次のペアへ
                    }
                }
            }

            return count;
        }

        //因子水準ペア2つをリストアップする
        private List<(FactorLevelPair, FactorLevelPair)> ListUpLevelPairs(List<(Factor, Factor)> twoFactorCombinations)
        {
            var levelPairs = new List<(FactorLevelPair, FactorLevelPair)>();
            foreach (var factorPair in twoFactorCombinations)
            {
                var factor1 = factorPair.Item1;
                var factor2 = factorPair.Item2;
                foreach (var levelOf1 in factor1.Levels)
                {
                    foreach (var levelOf2 in factor2.Levels)
                    {
                        var pair = new FactorLevelPair(factor1.Name, levelOf1.Name);
                        var pair2 = new FactorLevelPair(factor2.Name, levelOf2.Name);
                        levelPairs.Add((pair, pair2));
                    }
                }
            }
            return levelPairs;
        }

        ///// <summary>
        ///// (因子1名称, 因子2名称) - (因子1水準, 因子2水準)リストの辞書を作成
        ///// </summary>
        //private static Dictionary<(string, string), List<(string, string)>> CreateFactorsToLevelDictionary(List<(Factor, Factor)> twoFactorCombinations)
        //{
        //    var dictionary = new Dictionary<(string, string), List<(string, string)>>();
        //    foreach (var factorPair in twoFactorCombinations)
        //    {
        //        var factor1Name = factorPair.Item1.Name;
        //        var factor2Name = factorPair.Item2.Name;
        //        var key = (factor1Name, factor2Name);

        //        var list = new List<(string, string)>();
        //        foreach (var levelOf1 in factorPair.Item1.Levels)
        //        {
        //            foreach (var levelOf2 in factorPair.Item2.Levels)
        //            {
        //                (string, string) value = (levelOf1.Name, levelOf2.Name);
        //                list.Add(value);
        //            }
        //        }
        //        dictionary.Add(key, list);
        //    }

        //    return dictionary;
        //}

        private List<(T, T)> GetCombinations<T>(List<T> list)
        {
            var result = new List<(T, T)>();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    result.Add((list[i], list[j]));
                }
            }
            return result;
        }
    }
}
