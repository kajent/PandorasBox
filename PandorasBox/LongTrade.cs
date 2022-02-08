using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class LongTrade : SignalPair
    {
        public LongTrade(Signal buySignal, Signal sellSignal):base(buySignal, sellSignal)
        {            
            //If you sold it before you bought it, somethings gone wrong!
            if ((SellSignal.date < BuySignal.date) || (SellSignal.dayMod < BuySignal.dayMod))
                throw new Exception("buy signal comes before sell signal on what should be a long");
            CalculateProfit();
        }

        public void CalculateProfit()
        {
            double initialPrice = BuySignal.price;
            double finalPrice = SellSignal.price;
            profit = finalPrice - initialPrice ;
            profit_byPercent = 100 * (finalPrice - initialPrice) / initialPrice;
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
