using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class Portfolio
    {
        private List<Stock> stocks;
        private List<TechnicalIndicatorPerformance> IndicatorPerformance;
        private List<Stock> unresponsiveStocks;

        public Portfolio(StockCollection selectedStocks)
        {
            stocks = selectedStocks.getStockList();
            IndicatorPerformance = new List<TechnicalIndicatorPerformance>();
        }

        //TODO: This might be orphaned as it's not being used
        public void filterStocksByIndicators(List<Utilities.IndicatorSignalNames> indcNames)
        {
            for (int i = 0; i < stocks.Count; i++)
            {
                bool removeStock = false;

                List<Indicator> indicators = stocks[i].getIndicators();
                int targetIndcLocation = int.MaxValue;
                
                foreach (Utilities.IndicatorSignalNames indcName in indcNames)
                {
                    for(int j = 0; j < indicators.Count; j++)
                        if (indicators[j].name == indcName)
                        {
                            List<Signal> signals = indicators[j].signals;
                            Signal buySignal = null, sellSignal = null;
                            for (int k = 0; k < indicators[j].signals.Count; j++)
                            {
                                if (signals[j].signal == Utilities.Command.Buy)
                                    buySignal = signals[j];
                            }

                            for (int k = 0; k < indicators[j].signals.Count; j++)
                            {
                                if (signals[j].signal == Utilities.Command.Sell && buySignal.date < signals[j].date)
                                    sellSignal = signals[j];
                            }

                            if (buySignal == null || sellSignal == null)
                                removeStock = true;
                        }
                }
                if (removeStock == true)
                {
                    unresponsiveStocks.Add(stocks[i]);
                    stocks.RemoveAt(i--);
                }
            }
        }

        public void CalculatePortfolioPerformance()
        {
            List<Indicator> indicators = stocks[0].getIndicators();
            for (int i = 0; i < indicators.Count; i++)
            {
                IndicatorPerformance.Add(new TechnicalIndicatorPerformance(indicators[i].name));
            }

            foreach (Stock stock in stocks)
            {
                //CalculateStockPerformance_by_TechnicalIndicator(stock);
            }
        }

        //Buy first and sell later tactic assumed
        public void CalculateStockPerformance_by_TechnicalIndicator(Stock stock, Utilities.IndicatorSignalNames indcName)
        {
            List<Indicator> indicators = stock.getIndicators();
            for (int i = 0; i < indicators.Count; i++)
            {
                List<Signal> signals = indicators[i].signals;
                Signal buySignal = null, sellSignal = null;
                for(int j = 0; j < indicators[i].signals.Count; j++)
                {
                    if (signals[j].signal == Utilities.Command.Buy)
                        buySignal = signals[j];
                }

                for (int j = 0; j < indicators[i].signals.Count; j++)
                {
                    if (signals[j].signal == Utilities.Command.Sell && buySignal.date < signals[j].date)
                        sellSignal = signals[j];
                }
                

                //IndicatorPerformance[i].addNetResult(
            }
            
        }        
    }

    class TechnicalIndicatorPerformance
    {
        Utilities.IndicatorSignalNames _name;
        List<double> percentNetResults;

        public TechnicalIndicatorPerformance(Utilities.IndicatorSignalNames name)
        {
            _name = name;
        }

        //assume we get in and out at close
        public void addNetResult(Signal entry, Signal exit)
        {
            double intro = entry.price;
            double outtro = exit.price;
            percentNetResults.Add((outtro - intro) / intro);
        }
    }
}
