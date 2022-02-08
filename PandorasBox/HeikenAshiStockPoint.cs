using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class HeikenAshiStockPoint : EnhancedSimpleStockPoint
    {
        public Utilities.HeikenAshiType StockSignal;
        public HeikenAshiStockPoint(int date, UInt32 volume, double open, double high, double low, double close, string symbol)
            : base(date, volume, open, high, low, close, symbol)
        {
            StockSignal = Utilities.HeikenAshiType.None;           
        }

        public void CrunchHeikenAshiSignal()
        {
            double topShadow = base.getShadowHeightAbove();
            double bottomShadow = base.getShadowHeightBelow();
            double bodySize = base.getBodyHeight();

            if (topShadow > 0 && bottomShadow == 0 && bodySize > 0)
                StockSignal = Utilities.HeikenAshiType.Bullish;

            if (topShadow == 0 && bottomShadow > 0 && bodySize > 0)
                StockSignal = Utilities.HeikenAshiType.Bearish;
            
            if (topShadow > 0 && bottomShadow > 0 && bodySize > 0)
                StockSignal = Utilities.HeikenAshiType.Reversal;
        }

        public Utilities.HeikenAshiType getHeikenAshiSignal()
        {
            return StockSignal;
        }
    }
}
