using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public record OptimizeResult
    {
        public int TwoFactorPairNum { get; }

        /// <summary>
        /// 2因子の水準のペアの合計数
        /// </summary>
        public int IdealTwoLevelPairNum { get; }

        /// <summary>
        /// 理想の上記ペアの内、実際に網羅できている数
        /// </summary>
        public int ActualTwoLevelPairNum { get; }
        public int Score { get => IdealTwoLevelPairNum - ActualTwoLevelPairNum; }

        public OptimizeResult(int twoFactorPairNum, int idealTwoLevelPairNum, int actualTwoLevelPairNum)
        {
            TwoFactorPairNum = twoFactorPairNum;
            IdealTwoLevelPairNum = idealTwoLevelPairNum;
            ActualTwoLevelPairNum = actualTwoLevelPairNum;
        }

    }
}
