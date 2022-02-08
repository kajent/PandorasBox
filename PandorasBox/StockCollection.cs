using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;

namespace PandorasBox
{
    class StockCollection
    {
        private List<Stock> StockSet;
        public List<String> ValidDates;

        public StockCollection(List<DataSet> StockDataSet, SeriesChartType chartType)
        {
            StockSet = new List<Stock>();
            foreach(DataSet SingleStockData in StockDataSet)
            {
                Stock newStockData = new Stock(SingleStockData, chartType);
                StockSet.Add(newStockData);
            }
        }

        public List<Stock> getStockList()
        {
            return StockSet;
        }

        public List<String> getListOfSymbols()
        {
            List<String> result = new List<String>();
            for (int i = 0; i < StockSet.Count; i++)
            {
                result.Add(StockSet[i].getSymbol());
            }
            return result;
        }

        public void HardCodedStockValidator()
        {

            String Result = "Total stocks before date filtering: " + StockSet.Count + "Removed for date inconsistency:\n";
            for (int i = 0; i < ValidDates.Count; i++)
            {
                string currentDAte = ValidDates[i];
                for (int j = 0; j < StockSet.Count; j++)
                {
                    String stockDate = StockSet[j].getDailyData()[i].getDate().ToString();
                    if (stockDate != currentDAte)
                    {
                        Result += StockSet[j].getSymbol() + "\n ";
                        StockSet.RemoveAt(j--);
                    }
                }
            }
            Result += "Resulting stocks is " + StockSet.Count;
            Console.WriteLine(Result);
        }

        public void evaluatePorfolio(int startPoint, int endPoint)
        {
            double initValueOfPortfolio = 0;
            double endValueOfPortfolio = 0;
            double percentageGain =0;

            int stocksGoneUp = 0;
            int stocksGoneDown = 0;

            int failCount = 0;

            /*Core assumption
             * Regardless of two stocks value, if we buy enough stocks of em to cover X dollars, and they both move by Y%, they produce the same result
             * i.e. 2000/50 = 40 $50 stocks, and 2000/2 = 1000 $2 stocks. a rise of 2% of either produces the same profit
             */ 
            for (int i = 0; i < StockSet.Count; i++)
            {
                bool validEvaluation = StockSet[i].selfEvaluation(startPoint, endPoint);
                if (validEvaluation)
                {
                    initValueOfPortfolio += StockSet[i].getValueOfShares(startPoint);
                    endValueOfPortfolio += StockSet[i].getValueOfShares(endPoint);
                    double change = StockSet[i].getDeltaPercentageValueOfShares();
                    if (change > 0)
                        stocksGoneUp++;
                    else if (change < 0)
                        stocksGoneDown++;
                    percentageGain += change;                    
                }
                else
                {
                    failCount++;
                }
            }

            percentageGain = percentageGain / (StockSet.Count - failCount);

            //Utilities.WriteToLogFile("Initial value is " + initValueOfPortfolio.ToString());
            //Utilities.WriteToLogFile("End value is " + endValueOfPortfolio.ToString());
            //Utilities.WriteToLogFile("Average percentage difference is " + percentageGain.ToString());
            //Utilities.WriteToLogFile("Number of stocks gone up : " + stocksGoneUp);
            //Utilities.WriteToLogFile("Number of stocks gone down: " + stocksGoneDown);
            //Utilities.WriteToLogFile("Total of : " + StockSet.Count.ToString() + " stocks");
            //Utilities.WriteToLogFile("Failure count: " + failCount.ToString() + "\n");

            Utilities.WriteToLogFile(initValueOfPortfolio.ToString() + "," + endValueOfPortfolio.ToString() + "," + percentageGain.ToString() + "," 
                + stocksGoneUp + "," + stocksGoneDown + "," + StockSet.Count.ToString() + "," + failCount.ToString() + "\n");

            Console.WriteLine("Initial value is " + initValueOfPortfolio.ToString());
            Console.WriteLine("End value is " + endValueOfPortfolio.ToString());            
            Console.WriteLine("Average percentage difference is " + percentageGain.ToString());
            Console.WriteLine("Number of stocks gone up : " + stocksGoneUp);
            Console.WriteLine("Number of stocks gone down: " + stocksGoneDown);
            Console.WriteLine("Total of : " + StockSet.Count.ToString() + " stocks");
            Console.WriteLine("Failure count: " + failCount.ToString());
            for(int j = 0; j < StockSet[0].getIndicators().Count; j++)
            {
                Console.WriteLine("tpp:" + StockSet[0].getIndicators()[j].totalPercentageProfit);
                Console.WriteLine("tpp2:" + StockSet[0].getIndicators()[j].totalPercentageProfit);
            }
            
            
        }

        public int Size()
        {
            return StockSet.Count;
        }

        public void removeStock(String symbol)
        {
            for (int i = 0; i < StockSet.Count; i++)
                if (symbol == StockSet[i].getSymbol())
                {
                    StockSet.RemoveAt(i);
                    break;
                }
        }

        #region Special Stock Collection Filters
        //Remove all stocks that aren't trending in the specified direction between the min and max strengths
        public void getStocksTrendingOver(Object args)
        {   
            Utilities.Trend Direction = (Utilities.Trend)(args as Object[])[0];
            Utilities.TrendInterval Interval = (Utilities.TrendInterval) (args as Object[])[1];
            Double maxStrength = (Double)(args as Object[])[2];
            Double minStrength = (Double)(args as Object[])[3];
            Boolean exponential = (Boolean)(args as Object[])[4];

            if (Direction == Utilities.Trend.Irrelevant)
                return;

            switch (Interval)
            {
                case Utilities.TrendInterval.Quick:
                    {
                        for (int i = 0; i < StockSet.Count; i++)
                        {
                            int lastindex = 0;
                            if (exponential)
                                lastindex = StockSet[i].MA_EMAQ.Count - 1;
                            else
                                lastindex = StockSet[i].MA_SMAQ.Count - 1;

                            Utilities.Trend stockDirection = StockSet[i].getMATrendDirection(Utilities.TrendInterval.Quick, exponential, Utilities.INTERVALQ, lastindex);
                            double stockTrendStrength = StockSet[i].getMATrendStrength(Utilities.TrendInterval.Quick, exponential, Utilities.INTERVALQ, lastindex);
                            if ((stockDirection != Direction) || 
                                Math.Abs(stockTrendStrength) > maxStrength ||
                                Math.Abs(stockTrendStrength) < minStrength)
                                StockSet.RemoveAt(i--);
                        }
                    }
                    break;

                case Utilities.TrendInterval.Short:
                    {
                        for (int i = 0; i < StockSet.Count; i++)
                        {
                            int lastindex = 0;
                            if (exponential)
                                lastindex = StockSet[i].MA_EMAS.Count - 1;
                            else
                                lastindex = StockSet[i].MA_SMAS.Count - 1;

                            Utilities.Trend stockDirection = StockSet[i].getMATrendDirection(Utilities.TrendInterval.Short, exponential, Utilities.INTERVALS, lastindex);
                            double stockTrendStrength = StockSet[i].getMATrendStrength(Utilities.TrendInterval.Short, exponential, Utilities.INTERVALS, lastindex);
                            if ((stockDirection != Direction) ||
                                Math.Abs(stockTrendStrength) > maxStrength ||
                                Math.Abs(stockTrendStrength) < minStrength)
                                StockSet.RemoveAt(i--);
                        }
                    }
                    break;

                case Utilities.TrendInterval.Intermediate:
                    {
                        for (int i = 0; i < StockSet.Count; i++)
                        {
                            int lastindex = 0;
                            if (exponential)
                                lastindex = StockSet[i].MA_EMAI.Count - 1;
                            else
                                lastindex = StockSet[i].MA_SMAI.Count - 1;

                            Utilities.Trend stockDirection = StockSet[i].getMATrendDirection(Utilities.TrendInterval.Intermediate, exponential, Utilities.INTERVALI, lastindex);
                            double stockTrendStrength = StockSet[i].getMATrendStrength(Utilities.TrendInterval.Intermediate, exponential, Utilities.INTERVALI, lastindex);
                            if ((stockDirection != Direction) ||
                                Math.Abs(stockTrendStrength) > maxStrength ||
                                Math.Abs(stockTrendStrength) < minStrength)
                                StockSet.RemoveAt(i--);
                        }
                    }
                    break;

                case Utilities.TrendInterval.Long:
                    {
                        for (int i = 0; i < StockSet.Count; i++)
                        {
                            int lastindex = 0;
                            if (exponential)
                                lastindex = StockSet[i].MA_EMAL.Count - 1;
                            else
                                lastindex = StockSet[i].MA_SMAL.Count - 1;

                            Utilities.Trend stockDirection = StockSet[i].getMATrendDirection(Utilities.TrendInterval.Long, exponential, Utilities.INTERVALL, lastindex);
                            double stockTrendStrength = StockSet[i].getMATrendStrength(Utilities.TrendInterval.Long, exponential, Utilities.INTERVALL, lastindex);
                            if ((stockDirection != Direction) ||
                                Math.Abs(stockTrendStrength) > maxStrength ||
                                Math.Abs(stockTrendStrength) < minStrength)
                                StockSet.RemoveAt(i--);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Invalid trend interval selected");
                    break;
            }
        }

        public void getStocksCrossingOver(Utilities.TrendInterval TrendOnTop, Utilities.TrendInterval TrendOnBottom)
        {
            for (int i = 0; i < StockSet.Count; i++)
            {
                List<Double> StockOnTop = StockSet[i].getTrend(TrendOnTop);
                List<Double> StockOnBottom = StockSet[i].getTrend(TrendOnBottom);

                if ((StockOnTop == null) || (StockOnBottom == null))
                    StockSet.RemoveAt(i--);
                else
                {
                    Double LastEntryStockOnTop = StockOnTop[StockOnTop.Count-1];
                    Double LastEntryStockOnBottom = StockOnBottom[StockOnBottom.Count - 1];
                    if (!(LastEntryStockOnTop > LastEntryStockOnBottom))
                        StockSet.RemoveAt(i--);
                }
            }
        }

        public void getStocksCrossingOver(Object args)
        {
            Utilities.MA_around_MA QtoS = (Utilities.MA_around_MA)(args as Object[])[0];
            Utilities.MA_around_MA StoI = (Utilities.MA_around_MA)(args as Object[])[1];
            Utilities.MA_around_MA ItoL = (Utilities.MA_around_MA)(args as Object[])[2];
            Boolean exponential = (Boolean) (args as Object[])[3];

            for (int i = 0; i < StockSet.Count; i++)
            {
                bool remove = false;

                Double QMA_Value = -1, SMA_Value = -1, IMA_Value = -1, LMA_Value = -1;

                int length = StockSet[i].getDailyData().Count;

                if (exponential)
                {
                    if(StockSet[i].MA_EMAQ.Count > 0)
                        QMA_Value = StockSet[i].MA_EMAQ[length - Utilities.INTERVALQ];

                    if (StockSet[i].MA_EMAS.Count > 0)
                        SMA_Value = StockSet[i].MA_EMAS[length - Utilities.INTERVALS];

                    if (StockSet[i].MA_EMAI.Count > 0)
                        IMA_Value = StockSet[i].MA_EMAI[length - Utilities.INTERVALI];

                    if (StockSet[i].MA_EMAL.Count > 0)
                        LMA_Value = StockSet[i].MA_EMAL[length - Utilities.INTERVALL];
                }
                else
                {
                    if (StockSet[i].MA_SMAQ.Count > 0)
                        QMA_Value = StockSet[i].MA_SMAQ[length - Utilities.INTERVALQ - 1];
                    if (StockSet[i].MA_SMAS.Count > 0)
                        SMA_Value = StockSet[i].MA_SMAS[length - Utilities.INTERVALS - 1];
                    if (StockSet[i].MA_SMAI.Count > 0)
                        IMA_Value = StockSet[i].MA_SMAI[length - Utilities.INTERVALI - 1];
                    if (StockSet[i].MA_SMAL.Count > 0)
                        LMA_Value = StockSet[i].MA_SMAL[length - Utilities.INTERVALL - 1];
                }

                if(SMA_Value > -1)
                switch (QtoS)
                {
                    case Utilities.MA_around_MA.Above:
                        if (!(QMA_Value > SMA_Value))
                            remove = true;
                        break;
                    case Utilities.MA_around_MA.Below:
                        if (!(QMA_Value < SMA_Value))
                            remove = true;
                        break;
                    case Utilities.MA_around_MA.Irrelevant:
                        //Do nothing
                        break;
                }

                if (IMA_Value > -1)
                switch (StoI)
                {
                    case Utilities.MA_around_MA.Above:
                        if (!(SMA_Value > IMA_Value))
                            remove = true;
                        break;
                    case Utilities.MA_around_MA.Below:
                        if (!(SMA_Value < IMA_Value))
                            remove = true;
                        break;
                    case Utilities.MA_around_MA.Irrelevant:
                        //Do nothing
                        break;
                }

                if (LMA_Value > -1)
                switch (ItoL)
                {
                    case Utilities.MA_around_MA.Above:
                        if (!(IMA_Value > LMA_Value))
                            remove = true;
                        break;
                    case Utilities.MA_around_MA.Below:
                        if (!(IMA_Value < LMA_Value))
                            remove = true;
                        break;
                    case Utilities.MA_around_MA.Irrelevant:
                        //Do nothing
                        break;
                }

                if (remove)
                    StockSet.RemoveAt(i--);
            }
        }

        public void getStocksByRangeWithinMA(Object args)
        {
            Utilities.TrendInterval TrendInterval = (Utilities.TrendInterval)(args as Object[])[0];
            Double Gap = (Double)(args as Object[])[1];
            bool Above = (Boolean)(args as Object[])[2];
            bool Below = (Boolean)(args as Object[])[3];
            //TODO:CONTINUE HERE, probably not working properly
            for (int i = 0; i < StockSet.Count; i++)
            {
                List<Double> MA = StockSet[i].getTrend(TrendInterval);
                if (MA != null)
                {                    
                    Double MA_Value = MA[MA.Count-1];
                    int length = StockSet[i].getDailyData().Count;

                    Double Price_Close = StockSet[i].getDailyData()[length - 1].getClose();
                    Double Price_Open = StockSet[i].getDailyData()[length - 1].getOpen();
                    int Direction = StockSet[i].getDailyData()[length - 1].getDirection();

                    if (Above && !Below)
                    {
                        //Open near the MA and move upward, else reject the stock
                        bool remove = false;
                        if (Math.Abs(((Price_Open - MA_Value) / MA_Value * 100)) >= Gap)
                            remove = true;
                        if (Direction != 1)
                            remove = true;
                        if (remove)
                            StockSet.RemoveAt(i--);
                    }

                    if (!Above && Below)
                    {
                        //Open near the MA and move Downward, else reject the stock
                        bool remove = false;
                        if (Math.Abs(((Price_Open - MA_Value) / MA_Value * 100)) >= Gap)
                            remove = true;
                        if (Direction != -1)
                            remove = true;
                        if (remove)
                            StockSet.RemoveAt(i--);
                    }

                    if (Above && Below)
                    {
                        if (Math.Abs(((Price_Close - MA_Value) / MA_Value * 100)) >= Gap)
                            StockSet.RemoveAt(i--);
                    }
                }
                else
                    StockSet.RemoveAt(i--);
            }
        }


        

        public void getStocksByFibonacciStyle(Utilities.FibonacciFlag FibonacciEvent)
        {
            for (int i = 0; i < StockSet.Count; i++)
            {
                if (StockSet[i].getFibonacciStatus() != FibonacciEvent)                
                        StockSet.RemoveAt(i--);
            }
        }

        public void getStocksByRSI(Object args)
        {
            Utilities.RSIType rsiType = (Utilities.RSIType)(args as Object[])[0];
            for (int i = 0; i < StockSet.Count; i++)
            {
                int RSILength = StockSet[i].getRSILength();

                if (rsiType == Utilities.RSIType.Bullish)
                {
                    if (!StockSet[i].RSI_OverSold(RSILength - 1))
                        StockSet.RemoveAt(i--);
                }
                else
                    if (!StockSet[i].RSI_OverBought(RSILength - 1))
                        StockSet.RemoveAt(i--);
            }
        }

        public void getStocksByStochastic(Object args)
        {
            Utilities.StochasticType stochasticType = (Utilities.StochasticType)(args as Object[])[0];
                        
            for (int i = 0; i < StockSet.Count; i++)
            {
                int StochasticLength = StockSet[i].getStochasticLength();
                int StochasticSMALength = StockSet[i].getStochasticSMALength();

                if (stochasticType == Utilities.StochasticType.Bullish)
                {
                    if (!StockSet[i].Stochastic_Oversold(StochasticLength - 1, StochasticSMALength - 1))
                        StockSet.RemoveAt(i--);
                }
                if (stochasticType == Utilities.StochasticType.Bearish)
                {
                    if (!StockSet[i].Stochastic_Overbought(StochasticLength - 1, StochasticSMALength - 1))
                        StockSet.RemoveAt(i--);
                }
                    
            }
        }

        //TODO: Right now it only does positive force, make it accept negative force
        public void getStocksByForce(Object args)
        {
            Utilities.ForceType forceType = (Utilities.ForceType)(args as Object[])[0];

            for (int i = 0; i < StockSet.Count; i++)
            {
                try
                {
                    bool remove = false;
                    Series forceEMA = StockSet[i].getForceIndexEMA();
                    double force = forceEMA.Points[forceEMA.Points.Count - 1].YValues[0];
                    if (force < 0 && forceType == Utilities.ForceType.Bullish)
                        remove = true;

                    if (force > 0 && forceType == Utilities.ForceType.Bearish)
                        remove = true;

                    if (remove)
                        StockSet.RemoveAt(i--);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + StockSet[i].getSymbol() + " failed to pass through filter");
                    StockSet.RemoveAt(i);
                }
            }
        }

        public void getStocksByBollinger(Object args)
        {
            Utilities.BandZones TargetZone = (Utilities.BandZones)(args as Object[])[0];
            int duration = Utilities.filterDuration;
            int window = Utilities.filterWindow;
            for (int i = 0; i < StockSet.Count; i++)
            {
                try
                {
                    bool insufficientDuration = false;
                    bool insufficientWindow = true;

                    List<Series> Bollingers = StockSet[i].CollectBollingerBands();
                    Series BollingerUpperBand = Bollingers[0];
                    Series BollingerLowerBand = Bollingers[1];
                    List<Double> MA = StockSet[i].MA_SMAS;
                    List<EnhancedSimpleStockPoint> DailyData = StockSet[i].getDailyData();

                    int malength = MA.Count - 1;
                    int ddlength = DailyData.Count - 1;
                    int bblength = BollingerLowerBand.Points.Count -1;

                    //Filter by bollinger condition applying over the duration
                    for(int k = 0; k < duration; k++)
                    {
                        Double MA_Value = MA[malength - k];
                        Double UpperBandValue = Convert.ToDouble(BollingerUpperBand.Points[bblength - k].YValues[0].ToString());
                        Double LowerBandValue = Convert.ToDouble(BollingerLowerBand.Points[bblength - k].YValues[0].ToString());
                        Double Price_High = DailyData[ddlength - k].getHigh();
                        Double Price_Low = DailyData[ddlength - k].getLow();

                        if ((TargetZone == Utilities.BandZones.UpperBand) && !(Price_High > UpperBandValue))
                            insufficientDuration = true;
                        if ((TargetZone == Utilities.BandZones.LowerBand) && !(Price_Low < LowerBandValue))
                            insufficientDuration = true;
                        if ((TargetZone == Utilities.BandZones.Middle) && !(Price_Low < MA_Value && Price_High > MA_Value))
                            insufficientDuration = true;
                    }

                    //Filter by bollinger condition being true atleast once over the window
                    for (int k = 0; k < window; k++)
                    {
                        Double MA_Value = MA[malength - k];
                        Double UpperBandValue = Convert.ToDouble(BollingerUpperBand.Points[bblength - k].YValues[0].ToString());
                        Double LowerBandValue = Convert.ToDouble(BollingerLowerBand.Points[bblength - k].YValues[0].ToString());
                        Double Price_High = DailyData[ddlength - k].getHigh();
                        Double Price_Low = DailyData[ddlength - k].getLow();

                        if ((TargetZone == Utilities.BandZones.UpperBand) && (Price_High > UpperBandValue))
                            insufficientWindow = false;
                        if ((TargetZone == Utilities.BandZones.LowerBand) && (Price_Low < LowerBandValue))
                            insufficientWindow = false;
                        if ((TargetZone == Utilities.BandZones.Middle) && (Price_Low < MA_Value && Price_High > MA_Value))
                            insufficientWindow = false;
                    }

                    if ((insufficientDuration) && (insufficientWindow))
                        StockSet.RemoveAt(i--);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + StockSet[i].getSymbol() + " failed to pass through filter");
                    StockSet.RemoveAt(i);
                }
            }
        }

        public void getStocksByHeikenAshi(Object args)
        {            
            Utilities.HeikenAshiType desiredHA = (Utilities.HeikenAshiType)(args as Object[])[0];

            int strength = (int) (args as Object[])[1];

            for (int i = 0; i < StockSet.Count; i++)
            {
                try
                {
                    bool remove = false;
                    List<HeikenAshiStockPoint> HeikenAshiData = StockSet[i].getHeikenAshiData();

                    for (int j = 0; j < strength; j++)
                    {
                        if (HeikenAshiData[HeikenAshiData.Count - 1 - j].getHeikenAshiSignal() != desiredHA)
                                remove = true;
                    }                           
                        

                    if (remove)
                        StockSet.RemoveAt(i--);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + StockSet[i].getSymbol() + " failed to pass through Heiken Ashi filter");
                    StockSet.RemoveAt(i);
                }
            }
        }

        public void getStocksByADX(Object args)
        {
            Utilities.ADXTrend targetADXTrend = (Utilities.ADXTrend)(args as Object[])[0];
            Utilities.ADX_PDIvsNDI PDIvsNDI = (Utilities.ADX_PDIvsNDI)(args as Object[])[1];
            Utilities.ADX_Trending Trending = (Utilities.ADX_Trending)(args as Object[])[2];
            int TrendStrength = 2;
            for (int i = 0; i < StockSet.Count; i++)
            {
                bool remove = false;

                Series[] ADX_Indicators = StockSet[i].CollectADX();
                Series ADX = ADX_Indicators[0];
                Series ADX_PDI = ADX_Indicators[1];
                Series ADX_NDI = ADX_Indicators[2];

                Double currentADX = ADX.Points[ADX.Points.Count - 1].YValues[0];
                Double currentPDI = ADX_PDI.Points[ADX_PDI.Points.Count - 1].YValues[0];
                Double currentNDI = ADX_NDI.Points[ADX_NDI.Points.Count - 1].YValues[0];

                if (Trending == Utilities.ADX_Trending.Trending)
                {
                    for (int j = ADX.Points.Count - 1; j > ADX.Points.Count - 1 - TrendStrength; j--)
                    {
                        if (ADX.Points[j].YValues[0] < ADX.Points[j - 1].YValues[0])
                            remove = true;
                    }
                }

                switch (PDIvsNDI)
                {
                    case Utilities.ADX_PDIvsNDI.PDI_above_NDI:
                        {
                            if (!(currentPDI > currentNDI))
                                remove = true;                            
                        }
                        break;
                    case Utilities.ADX_PDIvsNDI.NDI_above_PDI:
                        {
                            if (!(currentNDI > currentPDI))
                                remove = true;
                        }
                        break;
                    case Utilities.ADX_PDIvsNDI.Irrelevant:
                        {
                            //do nothing
                        }
                        break;
                }

                switch (targetADXTrend)
                {
                    case Utilities.ADXTrend.NoTrend:
                        {
                            if (currentADX > Utilities.ADX_normal_trend_line)
                                remove = true;
                        }
                        break;
                    case Utilities.ADXTrend.Normal:
                        {
                            if (!(currentADX > Utilities.ADX_normal_trend_line))
                                remove = true;
                        }
                        break;
                    case Utilities.ADXTrend.Strong:
                        {
                            if (!(currentADX > Utilities.ADX_strong_trend_line))
                                remove = true;
                        }
                        break;
                    case Utilities.ADXTrend.Extreme:
                        {
                            if (!(currentADX > Utilities.ADX_extreme_trend_line))
                                remove = true;
                        }
                        break;
                    case Utilities.ADXTrend.Irrelevant:
                        {
                            //do nothing
                        }
                        break;
                }

                if (remove)
                    StockSet.RemoveAt(i--);
            }
        }

        public void getStocksByMACD(Object args)
        {            
            Utilities.MACD_around_Signal macdAroundSignal = (Utilities.MACD_around_Signal)(args as Object[])[0];
            Utilities.MACD_around_Zero macdAboveZeroReq = (Utilities.MACD_around_Zero)(args as Object[])[1];            

            int duration = Utilities.filterDuration;
            int window = Utilities.filterWindow;

            for (int i = 0; i < StockSet.Count; i++)
            {
                try
                {                    
                    bool remove = false;

                    if (macdAroundSignal == Utilities.MACD_around_Signal.Above)
                    {
                        bool Indicator_Holds_Over_Duration = true;
                        bool Indicator_True_Within_Window = false;

                        Series macd = StockSet[i].getMACD_Diff_EMA();
                        Series macdSignal = StockSet[i].getMACD_Signal_EMA();

                        for (int k = 0; k < duration; k++)
                        {
                            //Condition must be true for the length of the duration
                            double macdValue = macd.Points[macd.Points.Count - k - 1].YValues[0];
                            double macdSignalValue = macdSignal.Points[macdSignal.Points.Count - k - 1].YValues[0];

                            if (macdValue <= macdSignalValue)
                                Indicator_Holds_Over_Duration = false;
                        }

                        for (int k = 0; k < window; k++)
                        {
                            //Condition must occur once within window
                            double macdValue = macd.Points[macd.Points.Count - k - 1].YValues[0];
                            double macdSignalValue = macdSignal.Points[macdSignal.Points.Count - k - 1].YValues[0];

                            if (macdValue >= macdSignalValue)
                                Indicator_True_Within_Window = true;
                        }

                        if (!Indicator_Holds_Over_Duration && !Indicator_True_Within_Window)
                            remove = true;
                    }
                    else if (macdAroundSignal == Utilities.MACD_around_Signal.Below)
                    {
                        bool Indicator_Holds_Over_Duration = true;
                        bool Indicator_True_Within_Window = false;

                        Series macd = StockSet[i].getMACD_Diff_EMA();
                        Series macdSignal = StockSet[i].getMACD_Signal_EMA();

                        for (int k = 0; k < duration; k++)
                        {
                            //Condition must be true for the length of the duration
                            double macdValue = macd.Points[macd.Points.Count - k - 1].YValues[0];
                            double macdSignalValue = macdSignal.Points[macdSignal.Points.Count - k - 1].YValues[0];

                            if (macdValue >= macdSignalValue)
                                Indicator_Holds_Over_Duration = false;
                        }

                        for (int k = 0; k < window; k++)
                        {
                            //Condition must occur once within window
                            double macdValue = macd.Points[macd.Points.Count - k - 1].YValues[0];
                            double macdSignalValue = macdSignal.Points[macdSignal.Points.Count - k - 1].YValues[0];

                            if (macdValue <= macdSignalValue)
                                Indicator_True_Within_Window = true;
                        }

                        if (!Indicator_Holds_Over_Duration && !Indicator_True_Within_Window)
                            remove = true;
                    }

                    if (macdAboveZeroReq == Utilities.MACD_around_Zero.Above)
                    {
                        bool Indicator_Holds_Over_Duration = true;
                        bool Indicator_True_Within_Window = false;

                        Series macd = StockSet[i].getMACD_Diff_EMA();

                        for (int k = 0; k < duration; k++)
                        {
                            //Condition must be true for the length of the duration
                            if (macd.Points[macd.Points.Count - k - 1].YValues[0] < 0)
                                Indicator_Holds_Over_Duration = false;
                        }

                        for (int k = 0; k < window; k++)
                        {
                            //Condition must occur once within window                            
                            if (macd.Points[macd.Points.Count - k -1].YValues[0] > 0)
                                Indicator_True_Within_Window = true;
                        }

                        if (!Indicator_Holds_Over_Duration && !Indicator_True_Within_Window)
                            remove = true;
                    }
                    else if (macdAboveZeroReq == Utilities.MACD_around_Zero.Below) //macd below zero
                    {
                        bool Indicator_Holds_Over_Duration = true;
                        bool Indicator_True_Within_Window = false;

                        Series macd = StockSet[i].getMACD_Diff_EMA();

                        for (int k = 0; k < duration; k++)
                        {
                            //Condition must be true for the length of the duration
                            if (macd.Points[macd.Points.Count - k - 1].YValues[0] > 0)
                                Indicator_Holds_Over_Duration = false;
                        }

                        for (int k = 0; k < window; k++)
                        {
                            //Condition must occur once within window                            
                            if (macd.Points[macd.Points.Count - k - 1].YValues[0] < 0)
                                Indicator_True_Within_Window = true;
                        }

                        if (!Indicator_Holds_Over_Duration && !Indicator_True_Within_Window)
                            remove = true;
                    }
                    
                    if (remove)
                        StockSet.RemoveAt(i--);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message +  StockSet[i].getSymbol() + " failed to pass through MACD filter");
                    StockSet.RemoveAt(i);
                }
            }
        }

        public void getStocksByDojiExclusive(Object args)
        {
            bool dojiStar = (Boolean)(args as Object[])[0];
            bool dojiDragonFly = (Boolean)(args as Object[])[1];
            bool dojiGravestone = (Boolean)(args as Object[])[2];
            bool dojiPiercing = (Boolean)(args as object[])[3];
            bool dojiBearishEngulfing = (Boolean)(args as object[])[4];
            bool dojiBullishEngulfing = (Boolean)(args as object[])[5];            
            bool dojiDarkCloud = (Boolean)(args as object[])[6];


            for (int i = 0; i < StockSet.Count; i++)
            {
                bool removeThisStock = true;

                if (dojiStar)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isDojiStar(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (dojiGravestone)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isGraveStone(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (dojiDragonFly)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isDragonFly(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (dojiPiercing)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isPiercing(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (dojiBullishEngulfing)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isBullishEngulfing(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (dojiBearishEngulfing)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isBearishEngulfing(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (dojiDarkCloud)
                {
                    List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                    if (Doji.isDarkCloud(dailydata, dailydata.Count - 1))
                        removeThisStock = false;
                }

                if (removeThisStock)
                    StockSet.RemoveAt(i--);
            }

        }

        public void getStocksByDojiStar(Object args)
        {
            for (int i = 0; i < StockSet.Count; i++)
            {
                List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                if (!Doji.isDojiStar(dailydata, dailydata.Count - 1))
                    StockSet.RemoveAt(i--);
            }
        }

        public void getStocksByDojiGraveStone(Object args)
        {
            for (int i = 0; i < StockSet.Count; i++)
            {
                List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                if (!Doji.isGraveStone(dailydata, dailydata.Count - 1))
                    StockSet.RemoveAt(i--);
            }
        }

        public void getStocksByDojiDragonFly(Object args)
        {
            for (int i = 0; i < StockSet.Count; i++)
            {
                List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();
                if (!Doji.isDragonFly(dailydata, dailydata.Count - 1))
                    StockSet.RemoveAt(i--);
            }
        }

        public void getStocksBySouthernKangarooTail(Object args)
        {
            //TODO:CONTINUE HERE, is this working as intended?
            
            for (int i = 0; i < StockSet.Count; i++)
            {
                List<EnhancedSimpleStockPoint> dailydata = StockSet[i].getDailyData();

                if (dailydata.Count < 3 || Doji.isDojiStar(dailydata, dailydata.Count - 1) || Doji.isDojiStar(dailydata, dailydata.Count - 2) || Doji.isDojiStar(dailydata, dailydata.Count - 3))
                {
                    StockSet.RemoveAt(i--);

                }
                else
                {

                    EnhancedSimpleStockPoint tailStart = dailydata[dailydata.Count - 3];
                    EnhancedSimpleStockPoint tailMid = dailydata[dailydata.Count - 2];
                    EnhancedSimpleStockPoint tailEnd = dailydata[dailydata.Count - 1];

                    bool isTail = true;

                    double tailStartAvg = (tailStart.getHigh() + tailStart.getLow()) / 2;
                    double tailEndAvg = (tailEnd.getHigh() + tailEnd.getLow()) / 2;
                    double tailMidAvg = (tailMid.getHigh() + tailMid.getLow()) / 2;

                    if (tailMidAvg >= tailStartAvg || tailMidAvg >= tailEndAvg)
                        isTail = false;

                    /*
                    tailStartAvg = (tailStart.getOpen() + tailStart.getClose()) / 2;
                    tailEndAvg = (tailEnd.getOpen() + tailEnd.getClose()) / 2;
                    tailMidAvg = (tailMid.getOpen() + tailMid.getClose()) / 2;

                    if (tailMidAvg >= tailStartAvg || tailMidAvg >= tailEndAvg)
                        isTail = false;

                    if (tailMid.getBodyHeight() < tailStart.getBodyHeight() || tailMid.getBodyHeight() < tailEnd.getBodyHeight())
                        isTail = false;

                    if (tailMid.getBodyHeight() < tailStart.getBodyHeight() || tailMid.getBodyHeight() < tailEnd.getBodyHeight())
                        isTail = false;
                    */

                    
                    //Start and End shadow should be smaller than tail shadow 
                    if (tailMid.getShadowHeight() < tailStart.getShadowHeight() || tailMid.getShadowHeight() < tailEnd.getShadowHeight())
                        isTail = false;
                                        
                    //Start and End lows should be near eachother
                    //if (Math.Abs(tailStart.getLow() - tailEnd.getLow()) / (tailStart.getBodyHeight() * 0.5 + tailEnd.getBodyHeight() * 0.5) >= 0.25)
                    //    isTail = false;
                                        
                    if (isTail == false)
                        StockSet.RemoveAt(i--);
                }
            }
        }

        public List<String> getStocksByInsufficientData(int timePeriod, int timeOffset)
        {
            List<String> removeTheseSymbols = new List<string>();
            for(int i = 0; i < StockSet.Count; i++)
            {
                if (StockSet[i].getDailyData().Count < timePeriod - timeOffset)
                    removeTheseSymbols.Add(StockSet[i].getSymbol());
            }
            return removeTheseSymbols;
        }
               
        #endregion


    }
}
