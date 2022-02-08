using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class RiskManagement
    {
        /*
        public RiskManagement(double Capital, double MaxMarginOfLossAsPercent)
        {
            riskTolerance = MaxMarginOfLossAsPercent;
            capital = Capital;
        }*/

        //TODO: Continue here
        //Calculate a risk managed exit for each buy signal
        public static void ManageRiskForStock(Stock stock)
        {
            //TODO:Continue here
            List<Indicator> Indicators = stock.getIndicators();
            foreach (Indicator indc in Indicators)
            {
                for (int i = 0; i < indc.signals.Count; i++)
                {
                    if (indc.signals[i].signal == Utilities.Command.Buy)
                    {
                        Signal entrySignal = indc.signals[i];
                        //Signal exitSignal = RiskManagement.CalculateMinimalLossExit(this, entrySignal.date, entrySignal.price, 100);
                        Signal exitSignal = RiskManagement.CalculateGreedyBullExit(stock, entrySignal.date, entrySignal.price, 100);
                        indc.signals.Add(exitSignal);
                    }
                }
                indc.signals = indc.signals.OrderBy(signal => signal.dayMod).ToList();
            }
        }

        public static Signal CalculateMinimalLossExit(Stock stock, int entryDate, double entryPrice, int shares)
        {
            double ValueOfShares = entryPrice * shares;
            List<EnhancedSimpleStockPoint> stockDataCollection = stock.getDailyData();

            int currentDay = 0;
            while (stockDataCollection[currentDay].getDate() != entryDate && currentDay < stockDataCollection.Count)
                currentDay++;

            if(stockDataCollection[currentDay].getDate() != entryDate)
                throw new Exception("Entry date not found");

            //Let's assume I buy in near the close            
            double acceptableLoss = Utilities.capital * (Utilities.riskTolerance / 100);
            double currentNetLoss = (entryPrice - stockDataCollection[currentDay].getLow()) * shares; 

            while (currentDay++ < stockDataCollection.Count-1 && currentNetLoss < acceptableLoss)
            {
                currentNetLoss = (entryPrice - stockDataCollection[currentDay].getLow()) * shares;
            }

            if (currentDay == stockDataCollection.Count)
                currentDay--;

            return new Signal(Utilities.Command.Sell, stockDataCollection[currentDay].getDateMod(), stockDataCollection[currentDay].getDate(), stockDataCollection[currentDay].getLow(), Utilities.exchange, stockDataCollection[currentDay].getSymbol());


        }

        //TODO: Need to rework this, some data is missing
        public static Signal CalculateGreedyBullExit(Stock stock, int entryDate, double bestPrice, int shares)
        {
            double ValueOfShares = bestPrice * shares;
            List<EnhancedSimpleStockPoint> stockDataCollection = stock.getDailyData();

            int currentDay = 0;
            try
            {                
                while (stockDataCollection[currentDay].getDate() != entryDate && currentDay < stockDataCollection.Count)
                    currentDay++;

                if (stockDataCollection[currentDay].getDate() != entryDate)
                    throw new Exception("Entry date not found");
            }
            catch (Exception e)
            {
                Utilities.missingData.Add(stock.getSymbol());
                currentDay = stockDataCollection.Count - 1;
                
            }

            //Let's assume I bought at the best price since the entry date at any given time
            double acceptableLoss = Utilities.capital * (Utilities.riskTolerance / 100);
            double currentNetLoss = (bestPrice - stockDataCollection[currentDay].getLow()) * shares;

            while (currentDay < stockDataCollection.Count - 1 && currentNetLoss < acceptableLoss)
            {
                if (stockDataCollection[currentDay].getClose() > bestPrice)
                    bestPrice = stockDataCollection[currentDay].getClose();
                currentNetLoss = (bestPrice - stockDataCollection[currentDay].getLow()) * shares;

                currentDay++;
            }

            if (currentDay == stockDataCollection.Count)
                currentDay--;

            return new Signal(Utilities.Command.Sell, stockDataCollection[currentDay].getDateMod(), stockDataCollection[currentDay].getDate(), stockDataCollection[currentDay].getLow(), Utilities.exchange, stockDataCollection[currentDay].getSymbol());

        }

    }
}
