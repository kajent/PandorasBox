using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class ShortTrade : SignalPair
    {   
        public ShortTrade(Signal buySignal, Signal sellSignal):base(buySignal, sellSignal)
        {         
            //If you bought it before you shorted it, somethings gone wrong!
            if ((SellSignal.date > BuySignal.date) || (SellSignal.dayMod > BuySignal.dayMod))
                throw new Exception("buy signal comes before sell signal on what should be a short");
            CalculateProfit();
        }

        public void CalculateProfit()
        {
            double initialPrice = SellSignal.price;
            double finalPrice = BuySignal.price;
            profit = initialPrice - finalPrice;
            profit_byPercent = 100 * (initialPrice - finalPrice) / initialPrice;
        }

        public double getProfit()
        {
            return base.profit;
        }

        public int getDate()
        {
            return base.dayMod;
        }
    }
}
