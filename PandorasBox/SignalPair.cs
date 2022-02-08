using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class SignalPair
    {
        public Signal SellSignal;
        public Signal BuySignal;
        public int dayMod;
        public double profit;
        public double profit_byPercent;
        public string symbol;
        public string exchange;

        public SignalPair(Signal buySignal, Signal sellSignal)
        {
            BuySignal = buySignal;
            SellSignal = sellSignal;
            symbol = SellSignal.symbol;
            exchange = SellSignal.exchange;
            dayMod = (SellSignal.dayMod + BuySignal.dayMod)/2;
        }
    }
}
