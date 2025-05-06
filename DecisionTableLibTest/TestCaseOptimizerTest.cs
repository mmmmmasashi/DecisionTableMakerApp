using DecisionTableLib.Decisions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DecisionTableLibTest
{
    public class TestCaseOptimizerTest
    {
        private readonly DecisionTableMaker _maker;
        private readonly TestCaseOptimizer _opt;

        public TestCaseOptimizerTest()
        {
            _maker = new DecisionTableMaker(FactorLevelTable.Blank, DecisionTableLib.FormulaAnalyzer.PlusMode.FillEven, true);
            _opt = new TestCaseOptimizer();

        }

        //デフォルトで生成されるのはこれ
        //因子	水準	1	2	3	4	5	6	7	8	9
        //FactorA	LevelA1	x	x							
        //		LevelA2			x	x					
        //		LevelA3					x	x	x	x	x
        //FactorB	LevelB1	x		x		x				
        //		LevelB2		x		x		x	x	x	x
        //FactorC	LevelC1	x	x	x						
        //		LevelC2				x	x	x			
        //		C3							x	x	x
        //FactorD	LevelC1	x			x			x		
        //		LevelC2		x			x			x	
        //		C3			x			x			x



        [Fact]
        public void 最適化スコア計算_掛け算と足し算()
        {
            var formula = "[FactorA(LevelA1,LevelA2,LevelA3)] * [FactorB(LevelB1,LevelB 2)] + [FactorC(LevelC1,LevelC2,C3)]*[FactorD(LevelC1,LevelC2,C3)]";
            var table = _maker.CreateFrom(formula);

            OptimizeResult result= _opt.CalcUncoverdPairNum(table);
            Assert.Equal(6, result.TwoFactorPairNum);//4因子なので、4C2 = 6
            //理論値は、(3*2)+(3*3)+(3*3)+(2*3)+(2*3)+(3*3) = 6+9+9+6+6+9 = 45
            Assert.Equal(45, result.IdealTwoLevelPairNum);//4因子なので、4C2 = 6
            Assert.Equal(38, result.ActualTwoLevelPairNum);
            Assert.Equal(7, result.Score);
        }

        [Fact]
        public void 最適化スコア計算_全て掛け算の場合は必ず0となる()
        {
            var formula = "[FactorA(LevelA1,LevelA2,LevelA3)] * [FactorB(LevelB1,LevelB 2)] * [FactorC(LevelC1,LevelC2,C3)]*[FactorD(LevelC1,LevelC2,C3)]";
            var table = _maker.CreateFrom(formula);

            OptimizeResult result = _opt.CalcUncoverdPairNum(table);
            Assert.Equal(6, result.TwoFactorPairNum);//4因子なので、4C2 = 6
            Assert.Equal(45, result.IdealTwoLevelPairNum);//4因子なので、4C2 = 6
            Assert.Equal(45, result.ActualTwoLevelPairNum);
            Assert.Equal(0, result.Score);
        }
    }
}
