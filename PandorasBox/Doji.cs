using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class Doji
    {
        public static bool isDojiStar(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (
                (candleSticks[index].getBodyHeight() / candleSticks[index].getShadowHeight() * 100 <= 20) &&
                (Math.Abs(candleSticks[index].getShadowHeightAbove() - candleSticks[index].getShadowHeightBelow()) * 100 <= 0.2)
                )
                return true;
            else
                return false;
        }

        public static bool isGraveStone(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (candleSticks[index].getShadowHeightAbove() >= candleSticks[index].getShadowHeightBelow() * 4)
            {
                return (candleSticks[index].getBodyHeight() / candleSticks[index].getShadowHeight() * 100 <= 20);
            }            
            else
                return false;
        }

        public static bool isDragonFly(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (candleSticks[index].getShadowHeightBelow() >= candleSticks[index].getShadowHeightAbove() * 4)
            {
                return (candleSticks[index].getBodyHeight() / candleSticks[index].getShadowHeight() * 100 <= 20);
            }
            else
                return false;
        }

        //Best used in long term uptrend experiencing a short term downtrend
        public static bool isBullishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (candleSticks.Count <2)
                return false;
            double prevClose = candleSticks[index - 1].getClose();
            double prevOpen = candleSticks[index - 1].getOpen();
            double currClose = candleSticks[index].getClose();
            double currOpen = candleSticks[index].getOpen();
            int prevDir = candleSticks[index-1].getDirection();
            int currDir = candleSticks[index].getDirection();

            if (isDojiStar(candleSticks, index) || isDojiStar(candleSticks, index - 1))
                return false;

            return ((prevOpen < currClose) && (prevClose > currOpen) && prevDir == -1 && currDir == 1);
        }

        public static bool isBearishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (candleSticks.Count < 2)
                return false;
            double prevClose = candleSticks[index - 1].getClose();
            double prevOpen = candleSticks[index - 1].getOpen();
            double currClose = candleSticks[index].getClose();
            double currOpen = candleSticks[index].getOpen();
            int prevDir = candleSticks[index - 1].getDirection();
            int currDir = candleSticks[index].getDirection();

            if (isDojiStar(candleSticks, index) || isDojiStar(candleSticks, index-1))
                return false;
            return ((prevClose < currOpen) && (prevOpen > currClose) && prevDir == 1 && currDir == -1);
        }

        public static bool isDarkCloud(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (candleSticks.Count < 3)
                return false;
            double prevClose = candleSticks[index - 1].getClose();
            double prevOpen = candleSticks[index - 1].getOpen();
            double currClose = candleSticks[index].getClose();
            double currOpen = candleSticks[index].getOpen();
            int prevDir = candleSticks[index - 1].getDirection();
            int prevprevDir = candleSticks[index - 2].getDirection();
            int currDir = candleSticks[index].getDirection();

            if (isDojiStar(candleSticks, index) || isDojiStar(candleSticks, index - 1))
                return false;
            return ((prevClose < currOpen) && (prevOpen < currClose) && prevprevDir == 1 && prevDir == 1 && currDir == -1);
        }

        public static bool isPiercing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
            if (candleSticks.Count < 2)
                return false;
            double prevClose = candleSticks[index - 1].getClose();
            double prevOpen = candleSticks[index - 1].getOpen();
            double prevHeight = candleSticks[index - 1].getBodyHeight();
            double currClose = candleSticks[index].getClose();
            double currOpen = candleSticks[index].getOpen();
            double currHeight = candleSticks[index].getBodyHeight();
            int prevDir = candleSticks[index - 1].getDirection();
            int currDir = candleSticks[index].getDirection();

            if (isDojiStar(candleSticks, index) || isDojiStar(candleSticks, index-1))
                return false;

            return ((prevClose > currOpen) && (prevOpen > currClose) && (currClose > prevHeight/2 + prevClose) && prevDir == -1 && currDir == 1);
        }

        /*
        public static bool isBullishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
        }
        
        public static bool isBullishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
        }

        public static bool isBullishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
        }

        public static bool isBullishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
        }

        public static bool isBullishEngulfing(List<EnhancedSimpleStockPoint> candleSticks, int index)
        {
        }
        */
    }
}
