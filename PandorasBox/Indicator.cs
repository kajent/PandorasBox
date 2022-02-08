using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PandorasBox
{
    class Indicator
    {
        public Utilities.IndicatorSignalNames name;
        public List<Signal> signals;
        public List<Double> priceProfits;
        public List<Double> percentageProfits;
        public List<ShortTrade> shorts;
        public List<LongTrade> longs;
        public Double totalPriceProfit;
        public Double totalPercentageProfit;
        public Indicator(Utilities.IndicatorSignalNames Name)
        {
            name = Name;
            signals = new List<Signal>();
            priceProfits = new List<Double>();
            percentageProfits = new List<Double>();
            shorts = new List<ShortTrade>();
            longs = new List<LongTrade>();
            totalPriceProfit = 0;
            totalPercentageProfit = 0;

        }
        
        public void crunchSignalPairs()
        {
            for (int i = 0; i < signals.Count-1; i++)
            {
                if ((signals[i].signal == Utilities.Command.Sell) && (signals[i + 1].signal == Utilities.Command.Buy))
                {
                    shorts.Add(new ShortTrade(signals[i+1], signals[i]));
                }

                if ((signals[i].signal == Utilities.Command.Buy) && (signals[i + 1].signal == Utilities.Command.Sell))
                {
                    longs.Add(new LongTrade(signals[i], signals[i+1]));
                }
            }
        }

        //Calculates profits going from every buy signal to the next sell signal
        //WARNING: Irrelevant now

        public void calculateLongRangeProfits()
        {
            for (int i = 0; i < signals.Count-1;i++)
            {
                if ((signals[i].signal == Utilities.Command.Buy) && (signals[i + 1].signal == Utilities.Command.Sell))
                {
                    priceProfits.Add(signals[i + 1].price - signals[i].price);
                    percentageProfits.Add((signals[i + 1].price - signals[i].price)*100 / signals[i].price);
                }
            }

            for (int i = 0; i < priceProfits.Count; i++ )
                totalPriceProfit += priceProfits[i];

            for (int i = 0; i < percentageProfits.Count; i++)
                totalPercentageProfit += percentageProfits[i];
        }

        public void addSignal(Signal signal)
        {
            signals.Add(signal);
        }

        public void sortSignals()
        {
            var sortedSignals = from sig in signals
                                orderby sig.dayMod
                                select sig;
            
        }

        public void removeRedundantLastSignals()
        {
            try
            {
                for (int i = signals.Count - 1; i - 1 >= 0; i--)
                {
                    if ((i >= 1) && (signals[i].signal == signals[i - 1].signal))
                    {
                        signals.RemoveAt(i);
                    }

                }
            }
            catch (Exception e)
            {
                Console.Write("WTF");
            }
            /*
            if (signals.Count > 1)
            {
                if (signals[signals.Count - 1].signal == signals[signals.Count - 2].signal)
                    signals.RemoveAt(signals.Count - 1);
            }
             **/
        }



        public void calculateRiskManagedExit()
        {
            for (int i = 0; i < signals.Count; i++)
            {

            }
        }

        public double getAverageLongProfits()
        {
            double netResult = 0;
            for (int i = 0; i < longs.Count; i++)
                netResult += longs[i].profit;

            return netResult / longs.Count;
        }

        public double getAverageShortProfits()
        {
            double netResult = 0;
            for (int i = 0; i < shorts.Count; i++)
                netResult += shorts[i].profit;

            return netResult / shorts.Count;
        }

        public double getAverageLongProfitsPercent()
        {
            double netResult = 0;
            for (int i = 0; i < longs.Count; i++)
                netResult += longs[i].profit_byPercent;

            return netResult / longs.Count;
        }

        public double getAverageShortProfitsPercent()
        {
            double netResult = 0;
            for (int i = 0; i < shorts.Count; i++)
                netResult += shorts[i].profit_byPercent;

            return netResult / shorts.Count;
        }

        public Utilities.StocksProfit getSummedLongProfitsPercent()
        {
            double netResult = 0;
            for (int i = 0; i < longs.Count; i++)
                netResult += longs[i].profit_byPercent;

            String symbol = this.signals[0].symbol;

            return new Utilities.StocksProfit(netResult, symbol);
        }

        public Utilities.StocksProfit getSummedShortProfitsPercent()
        {
            double netResult = 0;
            for (int i = 0; i < shorts.Count; i++)
                netResult += shorts[i].profit_byPercent;

            String symbol = this.signals[0].symbol;

            return new Utilities.StocksProfit(netResult, symbol);
        }


        public void reportProfits()
        {
            double avgLongProfits = getAverageLongProfits();
            Console.WriteLine("Profits on all long trades averaged $" + avgLongProfits.ToString());
            Console.WriteLine("Profits on all short trades averaged $" + getAverageShortProfits().ToString());
            Utilities.ProfitsByPercent_Longs.Add(getSummedLongProfitsPercent());
            Utilities.ProfitsByPercent_Shorts.Add(getSummedShortProfitsPercent());
            //Utilities.ProfitsByPercent.Add(
        }
    }
}
