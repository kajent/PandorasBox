using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization;
using System.Data;
using System.Drawing;

/* Calculation Sequence
 * 
 * 1 - Stock initialized with EnchancedSimpleStockPoints
 *      - max and min over the interval are determined, 
 *      - set Trends for each stock to 'not calculated'
 *      - set fibonacci ratios
 *      - set selected chart type
 *      
 * 2 - Convert stock data points from EnhancedSimpleStockPoints to Series data so it can be analyzed using MSChart.
 *      - price series and volume series are returned to main program
 *      
 * 3 - Main program starts doing MA analysis (AnalyzeWithinMarketGraph) and collects resultant datasets
 * 
 * 4 - Process resultant datasets
 *      - set SMAQ through EMAL to the corresponding datasets from analysis performed within MSChart
 *      - set Q,S,I,L trends and strengthsbased on SMAQ - EMAL datasets
 *      - calculate bull/bear cross overs for EMA
 *      - calculate fibonacci points
 */


namespace PandorasBox
{
    class Stock
    {       
        #region Constants
        private readonly int INTERVALQ;//Quick
        private readonly int INTERVALS;//Short
        private readonly int INTERVALI;//Intermediate
        private readonly int INTERVALL;//Long
        private readonly SeriesChartType chartType;
        #endregion
        
        private List<EnhancedSimpleStockPoint> DailyDataCollection;
        private List<HeikenAshiStockPoint> HeikinAshi;
        
        #region Moving Average Variables
        
        public List<Double> MA_SMAQ;
        public List<Double> MA_SMAS;
        public List<Double> MA_SMAI;
        public List<Double> MA_SMAL;

        public List<Double> MA_EMAQ;
        public List<Double> MA_EMAS;
        public List<Double> MA_EMAI;
        public List<Double> MA_EMAL;
        
       
        
        #endregion
                
        private DataSet BollingerBands;
        private double BollingerSTDRatio;

        private DataSet RSI;
        private DataSet RSI_Stochastic;
        private List<Double> Stochastic;
        private List<Double> Stochastic_SMA;
        private int Stochastic_K_Period;
        private int Stochastic_D_Period;


        private double RSI_UpperBound;
        private double RSI_LowerBound;
        private int RSI_period;
        private static Color overSold = Color.LightGreen;
        private static Color overBought = Color.Red;

        private List<double> ForceIndex;
        private List<double> ForceIndex_EMA;
        private int ForceIndex_EMA_period;

        private DataSet EnvelopeUpperChannel;
        private DataSet EnvelopeLowerChannel;

        private static Color volUp = Color.LightGreen;
        private static Color volDown = Color.Red;

        private Utilities.FibonacciFlag FibonacciStatus = Utilities.FibonacciFlag.NotCalculated;

        private List<DataSet> BuySellSignals;
        private List<Indicator> Indicators;

        private double[] Fibonacci; 

        private double minStockValue;
        private double maxStockValue;
        private String symbol;
        private int numberOfShares;
        private double deltaValueOfShares;
        private double deltaPercentageValueOfShares;

        private int MACD_shortEMA_period;
        private int MACD_longEMA_period;
        private int MACD_signalEMA_period;
        private List<double> MACD_shortEMA;
        private List<double> MACD_longEMA;
        private List<double> MACD_long_short_diff;
        private List<double> MACD_signalEMA;
        private List<double> MACD_Histogram;

        private List<double> TrueRange;
        private int ADX_TrueRange_Period;
        private List<double> TrueRange_EMA;
        private List<double> P_DM;
        private List<double> N_DM;
        private List<double> P_DM_EMA;
        private List<double> N_DM_EMA;
        private List<double> Plus_DI;
        private List<double> Minus_DI;
        private List<double> DX;
        private List<double> ADX;

        private List<Utilities.StockPoint> LocalMaxs;
        private List<Utilities.StockPoint> LocalMins;

        
        #region Constructors
        public Stock()
        {
            DailyDataCollection = new List<EnhancedSimpleStockPoint>();
            HeikinAshi = new List<HeikenAshiStockPoint>();
            
            INTERVALQ = Utilities.INTERVALQ;
            INTERVALS = Utilities.INTERVALS;
            INTERVALI = Utilities.INTERVALI;
            INTERVALL = Utilities.INTERVALL;

            Stochastic = new List<Double>();
            Stochastic_SMA = new List<Double>();

            Stochastic_K_Period = Utilities.RSI_period;

            BollingerSTDRatio = Utilities.BollingerSTDRatio;
            Indicators = new List<Indicator>();
            Fibonacci = new Double[7] { 1.00, 0.618, 0.500, 0.382, 0.236, 0.146, 0.00 };
            numberOfShares = 1;
            BuySellSignals = new List<DataSet>();

            RSI = new DataSet();
            RSI_Stochastic = new DataSet();
            RSI_LowerBound = Utilities.RSI_LowerBound;
            RSI_UpperBound = Utilities.RSI_UpperBound;
            RSI_period = Utilities.RSI_period;

            ForceIndex = new List<double>();
            ForceIndex_EMA = new List<double>();
            ForceIndex_EMA_period = Utilities.ForceIndex_EMA_period;

            MACD_longEMA_period = Utilities.MACD_longEMA_period;
            MACD_shortEMA_period = Utilities.MACD_shortEMA_period;
            MACD_signalEMA_period = Utilities.MACD_signalEMA_period;
            MACD_longEMA = new List<double>();
            MACD_shortEMA = new List<double>();
            MACD_signalEMA = new List<double>();
            MACD_long_short_diff = new List<double>();
            MACD_Histogram = new List<double>();

            ADX_TrueRange_Period = Utilities.ADX_period;
            LocalMins = new List<Utilities.StockPoint>();
            LocalMaxs = new List<Utilities.StockPoint>();
       
        }

        public Stock(List<EnhancedSimpleStockPoint> IncomingDailyDataCollection, SeriesChartType selectedChartType): this()
        {
            DailyDataCollection = IncomingDailyDataCollection;
            minStockValue = Utilities.getStockMinValue_ForAnalysis(DailyDataCollection);
            maxStockValue = Utilities.getStockMaxValue(DailyDataCollection);
            symbol = DailyDataCollection[0].getSymbol();
            chartType = selectedChartType;
            
        }

        public Stock(DataSet StockDataSet, SeriesChartType selectedChartType):this()
        {
            DailyDataCollection = Utilities.Convert_DataSet_to_EnhancedStock_ForAnalyzing(StockDataSet);
            minStockValue = Utilities.getStockMinValue_ForAnalysis(DailyDataCollection);
            maxStockValue = Utilities.getStockMaxValue(DailyDataCollection);
            symbol = DailyDataCollection[0].getSymbol();
            chartType = selectedChartType;
        }
        #endregion

        #region Accessors
        public double getMinStockValue()
        {
            return minStockValue;
        }

        public double getMaxStockValue()
        {
            return maxStockValue;
        }

        public String getSymbol()
        {
            return symbol;
        }

        public int getNumberOfShares()
        {
            return numberOfShares;
        }

        public Utilities.Trend getMATrendDirection(Utilities.TrendInterval TargetInterval, Boolean Exponential, int range, int index)
        {
            Utilities.Trend output = Utilities.Trend.Irrelevant;
            if (Exponential)
                switch (TargetInterval)
                {
                    case Utilities.TrendInterval.Quick:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_EMAQ, range, index);
                        break;
                    case Utilities.TrendInterval.Short:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_EMAS, range, index);
                        break;
                    case Utilities.TrendInterval.Intermediate:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_EMAI, range, index);
                        break;
                    case Utilities.TrendInterval.Long:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_EMAL, range, index);
                        break;
                }
            else
                switch (TargetInterval)
                {
                    case Utilities.TrendInterval.Quick:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_SMAQ, range, index);
                        break;
                    case Utilities.TrendInterval.Short:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_SMAS, range, index);
                        break;
                    case Utilities.TrendInterval.Intermediate:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_SMAI, range, index);
                        break;
                    case Utilities.TrendInterval.Long:
                        output = Utilities.GetTrendDirection_of_DoubleList(MA_SMAL, range, index);
                        break;
                }
            return output;
        }

        public Double getMATrendStrength(Utilities.TrendInterval TargetInterval, Boolean Exponential, int range, int index)
        {
            Double output = Double.NaN;
            if (Exponential)
                switch (TargetInterval)
                {
                    case Utilities.TrendInterval.Quick:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_EMAQ, range, index);
                        break;
                    case Utilities.TrendInterval.Short:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_EMAS, range, index);
                        break;
                    case Utilities.TrendInterval.Intermediate:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_EMAI, range, index);
                        break;
                    case Utilities.TrendInterval.Long:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_EMAL, range, index);
                        break;
                }
            else
                switch (TargetInterval)
                {
                    case Utilities.TrendInterval.Quick:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_SMAQ, range, index);
                        break;
                    case Utilities.TrendInterval.Short:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_SMAS, range, index);
                        break;
                    case Utilities.TrendInterval.Intermediate:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_SMAI, range, index);
                        break;
                    case Utilities.TrendInterval.Long:
                        output = Utilities.GetTrendStrength_of_DoubleList(MA_SMAL, range, index);
                        break;
                }
            return output;
        }

        

        public int getStochasticLength()
        {
            return Stochastic.Count;
        }

        public int getStochasticSMALength()
        {
            return Stochastic_SMA.Count;
        }

        public int getRSILength()
        {
            return RSI.Tables["RSI"].Rows.Count;
        }

        public double getDeltaPercentageValueOfShares()
        {
            return deltaPercentageValueOfShares;
        }

        public double getDeltaValueOfShares()
        {
            return deltaValueOfShares;
        }

        public List<Indicator> getIndicators()
        {
            return Indicators;
        }

        public List<Series> getMovingAverages()
        {
            List<Series> output = new List<Series>();

            output.Add(Utilities.Convert_DoubleList_to_Series(MA_SMAQ, Utilities.INTERVALQ, "SMAQ"));
            output.Add(Utilities.Convert_DoubleList_to_Series(MA_SMAS, Utilities.INTERVALS, "SMAS"));
            output.Add(Utilities.Convert_DoubleList_to_Series(MA_SMAI, Utilities.INTERVALI, "SMAI"));
            output.Add(Utilities.Convert_DoubleList_to_Series(MA_SMAL, Utilities.INTERVALL, "SMAL"));

            output.Add(Utilities.Convert_DoubleList_to_Series(MA_EMAQ, Utilities.INTERVALQ, "EMAQ"));
            output.Add(Utilities.Convert_DoubleList_to_Series(MA_EMAS, Utilities.INTERVALS, "EMAS"));
            output.Add(Utilities.Convert_DoubleList_to_Series(MA_EMAI, Utilities.INTERVALI, "EMAI"));
            output.Add(Utilities.Convert_DoubleList_to_Series(MA_EMAL, Utilities.INTERVALL, "EMAL"));

            return output;
        }

        public List<Double> getTrend(Utilities.TrendInterval TrendInterval)
        {
            switch (TrendInterval)
            {
                case Utilities.TrendInterval.Quick:
                    return MA_EMAQ;
                case Utilities.TrendInterval.Short:
                    return MA_EMAS;
                case Utilities.TrendInterval.Intermediate:
                    return MA_EMAI;
                case Utilities.TrendInterval.Long:
                    return MA_EMAL;
            }
            //this will run
            return null;
        }

        public Utilities.FibonacciFlag getFibonacciStatus()
        {
            return this.FibonacciStatus;
        }

        public List<EnhancedSimpleStockPoint> getDailyData()
        {
            return DailyDataCollection;
        }

        public List<HeikenAshiStockPoint> getHeikenAshiData()
        {
            return HeikinAshi;
        }

        #endregion

        #region Complex Accessors
        /*
        public Series getQSBulls()
        {
            return Utilities.Convert_DataSet_to_Series(QSMA_CrossOvers, "BullCross", "QSBull", SeriesChartType.Point, MarkerStyle.Cross, Color.Gold, Color.Gold);
        }
        */
       

        public Series CollectMACDCrossOverSignals()
        {
            Series MACDXovers = GrabSpecificIndicator(Utilities.IndicatorSignalNames.MACDXover);
            MACDXovers.Name = "MACDXovers";
            return MACDXovers;
        }

        public Series CollectMACDLongs()
        {
            Indicator targetIndicator = Indicators.Find(delegate(Indicator o) { return o.name == Utilities.IndicatorSignalNames.MACDXover; });
            Series MACDLongSeries = Utilities.Convert_Longs_To_Profit_Series(targetIndicator, Utilities.getIndicatorSignalNameAsString(targetIndicator.name), SeriesChartType.Column, MarkerStyle.None, Color.Blue);
            MACDLongSeries.Name = "MACDLong";
            return MACDLongSeries;
        }

        public Series CollectMACDShorts()
        {
            Indicator targetIndicator = Indicators.Find(delegate(Indicator o) { return o.name == Utilities.IndicatorSignalNames.MACDXover; });
            Series MACDShortSeries = Utilities.Convert_Shorts_To_Profit_Series(targetIndicator, Utilities.getIndicatorSignalNameAsString(targetIndicator.name), SeriesChartType.Column, MarkerStyle.None, Color.Green);
            MACDShortSeries.Name = "MACDShort";
            return MACDShortSeries;
        }

        public Series CollectCustomLongs()
        {
            Indicator targetIndicator = Indicators.Find(delegate(Indicator o) { return o.name == Utilities.IndicatorSignalNames.CustomXOver; });
            Series CustomLongSeries = Utilities.Convert_Longs_To_Profit_Series(targetIndicator, Utilities.getIndicatorSignalNameAsString(targetIndicator.name), SeriesChartType.Column, MarkerStyle.None, Color.Blue);
            CustomLongSeries.Name = "CustomLong";
            return CustomLongSeries;
        }

        public Series CollectCustomShorts()
        {
            Indicator targetIndicator = Indicators.Find(delegate(Indicator o) { return o.name == Utilities.IndicatorSignalNames.CustomXOver; });
            Series CustomShortSeries = Utilities.Convert_Shorts_To_Profit_Series(targetIndicator, Utilities.getIndicatorSignalNameAsString(targetIndicator.name), SeriesChartType.Column, MarkerStyle.None, Color.Green);
            CustomShortSeries.Name = "CustomShort";
            return CustomShortSeries;
        }

        public Series GrabSpecificIndicator(Utilities.IndicatorSignalNames indicatorName)
        {
            //Series specifiedSeries = Utilities.Convert_DataSet_to_Series(QSMA_CrossOvers, "BullCross", "QSBull", SeriesChartType.Point, MarkerStyle.Cross, Color.Gold, Color.Gold);
            Indicator targetIndicator = Indicators.Find(delegate(Indicator o) { return o.name == indicatorName; });
            Series targetSeries = Utilities.Convert_Indicator_To_Series(targetIndicator, Utilities.getIndicatorSignalNameAsString(targetIndicator.name), SeriesChartType.Line, MarkerStyle.Cross, Color.Blue);
            targetSeries.Sort(PointSortOrder.Ascending, "X");
            return targetSeries;            
        }

        public double getValueOfShares(int timeOffSet)
        {
            if (timeOffSet > DailyDataCollection.Count -1)
                return -1;
            double valueOfShares = numberOfShares * DailyDataCollection[DailyDataCollection.Count - 1 - timeOffSet].getClose();
            return valueOfShares;
        }

        public Series getMACD_ShortEMA()
        {
            return Utilities.Convert_DoubleList_to_Series(MACD_shortEMA, MACD_shortEMA_period, "12-EMA");
        }

        public Series getMACD_LongEMA()
        {
            return Utilities.Convert_DoubleList_to_Series(MACD_longEMA, MACD_longEMA_period, "26-EMA");
        }

        public Series getMACD_Diff_EMA()
        {
            return Utilities.Convert_DoubleList_to_Series(MACD_long_short_diff, MACD_longEMA_period, "MACD");
        }

        public Series getMACD_Signal_EMA()
        {
            return Utilities.Convert_DoubleList_to_Series(MACD_signalEMA, MACD_signalEMA_period + MACD_longEMA_period -1, "9-EMA-Signal");
        }

        public Series getMACD_HistoGram()
        {
            Series Histogram = Utilities.Convert_DoubleList_to_Series(MACD_Histogram, MACD_signalEMA_period + MACD_longEMA_period -1, "MACD-Histogram");
            Histogram.ChartType = SeriesChartType.Column;
            Histogram.MarkerSize = 4;
            Histogram.MarkerBorderWidth = 5;
            Histogram.Color = Color.Green;
            return Histogram;
        }

        public Series getForceIndex()
        {
            Series FI = Utilities.Convert_DoubleList_to_Series(ForceIndex, 1, "Force Index");
            FI.ChartType = SeriesChartType.Area;
            FI.MarkerSize = 4;
            FI.MarkerBorderWidth = 5;

            for (int i = 0; i < FI.Points.Count; i++)
            {
                if (FI.Points[i].YValues[0] >= 0)
                    FI.Points[i].Color = overSold;
                if (FI.Points[i].YValues[0] <= 0)
                    FI.Points[i].Color = overBought;
            }
            return FI;
        }

        public Series getForceIndexEMA()
        {
            Series FI = Utilities.Convert_DoubleList_to_Series(ForceIndex_EMA, ForceIndex_EMA_period, "Force Index EMA 13");
            FI.ChartType = SeriesChartType.Area;
            //FI.MarkerSize = 4;
            //FI.MarkerBorderWidth = 5;
            FI.Color = Color.Blue;
            for (int i = 0; i < FI.Points.Count; i++)
            {
                if (FI.Points[i].YValues[0] >= 0)
                    FI.Points[i].Color = overSold;
                if (FI.Points[i].YValues[0] <= 0)
                    FI.Points[i].Color = overBought;
            }

            return FI;
        }

        public Series getStochastic()
        {
            Series K = Utilities.Convert_DoubleList_to_Series(Stochastic, Stochastic_K_Period, "StochasticK");
            return K;
        }

        public Series getStochasticEMA()
        {
            Series K = Utilities.Convert_DoubleList_to_Series(Stochastic_SMA, Stochastic_D_Period + Stochastic_K_Period, "StochasticD");
            return K;
        }

        public Series getHeikinAshi()
        {
            Series HeikinAshiPrice = Utilities.Convert_HeikenAshiStockData_to_PriceSeries(HeikinAshi, "Heikin-Ashi", chartType);
            HeikinAshiPrice.Color = Color.Blue;
            return HeikinAshiPrice;
        }

        public Series getEnvelopeUpperchannel()
        {
            Series EnvUpperChannel = Utilities.Convert_DataSet_to_Series(EnvelopeUpperChannel, "Envelope Upper Channel", "EnvU", SeriesChartType.Line, MarkerStyle.None, Color.Green, Color.Green);
            return EnvUpperChannel;
        }

        public Series getEnvelopeLowerchannel()
        {
            Series EnvLowerChannel = Utilities.Convert_DataSet_to_Series(EnvelopeLowerChannel, "Envelope Lower Channel", "EnvL", SeriesChartType.Line, MarkerStyle.None, Color.Green, Color.Green);
            return EnvLowerChannel;
        }

        public List<Series> CollectFibonacciSeries()
        {
            List<Series> FibonacciSeries = new List<Series>();
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[0],"100%",SeriesChartType.Line, Color.Green, 0, DailyDataCollection.Count -1));
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[1], "61.8%", SeriesChartType.Line, Color.Green, 0, DailyDataCollection.Count - 1));
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[2], "50.0%", SeriesChartType.Line, Color.Black, 0, DailyDataCollection.Count - 1));
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[3], "38.2%", SeriesChartType.Line, Color.Green, 0, DailyDataCollection.Count - 1));
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[4], "23.6%", SeriesChartType.Line, Color.Green, 0, DailyDataCollection.Count - 1));
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[5], "14.6%", SeriesChartType.Line, Color.Green, 0, DailyDataCollection.Count - 1));
            FibonacciSeries.Add(Utilities.Convert_YValue_to_SeriesLine(Fibonacci[6], "00.0%", SeriesChartType.Line, Color.DarkGreen, 0, DailyDataCollection.Count - 1));
            //TODO: Do something with fibonnacii

            foreach (Series fiboseries in FibonacciSeries)
            {
                fiboseries.MarkerSize = 1;
            }
            return FibonacciSeries;
        }

        public List<Series> CollectBollingerBands()
        {
            if (BollingerBands == null)
                return null;
            List<Series> BollingerBandSet = new List<Series>();
            BollingerBandSet.Add(Utilities.Convert_DataSet_to_Series(BollingerBands, "Bollinger Upper Band", "Upper Bollinger", SeriesChartType.Line, MarkerStyle.None, Color.Gold, Color.Gold));
            BollingerBandSet.Add(Utilities.Convert_DataSet_to_Series(BollingerBands, "Bollinger Lower Band", "Lower Bollinger", SeriesChartType.Line, MarkerStyle.None, Color.Gold, Color.Gold));
            BollingerBandSet.Add(Utilities.Convert_DataSet_to_Series(BollingerBands, "Bollinger Upper Mid Band", "Upper Mid Bollinger", SeriesChartType.Line, MarkerStyle.None, Color.Gold, Color.Gold));
            BollingerBandSet.Add(Utilities.Convert_DataSet_to_Series(BollingerBands, "Bollinger Lower Mid Band", "Lower Mid Bollinger", SeriesChartType.Line, MarkerStyle.None, Color.Gold, Color.Gold)); 
                        
            return BollingerBandSet;
        }

        public Series CollectRSI()
        {
            if (RSI.Tables.Count == 0)
                return null;
            Series Series_RSI = Utilities.Convert_DataSet_to_Series(RSI, "RSI", "RSI", SeriesChartType.Line, MarkerStyle.None, Color.Black, Color.Black);
            for (int i = 0; i < Series_RSI.Points.Count; i++)
            {
                if (Series_RSI.Points[i].YValues[0] >= RSI_UpperBound)
                    Series_RSI.Points[i].Color = overBought;
                if (Series_RSI.Points[i].YValues[0] <= RSI_LowerBound)
                    Series_RSI.Points[i].Color = overSold;
            }
            return Series_RSI;
        }

        public Series CollectRSIStochastic()
        {
            if (RSI_Stochastic.Tables.Count == 0)
                return null;
            Series Series_RSI_Stochastic = Utilities.Convert_DataSet_to_Series(RSI_Stochastic, "RSI_Stochastic", "RSI_Stochastic", SeriesChartType.Line, MarkerStyle.None, Color.Black, Color.Black);
            for (int i = 0; i < Series_RSI_Stochastic.Points.Count; i++)
            {
                if (Series_RSI_Stochastic.Points[i].YValues[0] >= RSI_UpperBound)
                    Series_RSI_Stochastic.Points[i].Color = overBought;
                if (Series_RSI_Stochastic.Points[i].YValues[0] <= RSI_LowerBound)
                    Series_RSI_Stochastic.Points[i].Color = overSold;
            }
            return Series_RSI_Stochastic;
        }

        public Series[] CollectADX()
        {
            Series ADX_ = Utilities.Convert_DoubleList_to_Series(ADX, ADX_TrueRange_Period*(1+Utilities.ADX_smoothing_iterations), "ADX");
            Series P_DI = Utilities.Convert_DoubleList_to_Series(Plus_DI, ADX_TrueRange_Period+1, "-PDI");
            Series M_DI = Utilities.Convert_DoubleList_to_Series(Minus_DI, ADX_TrueRange_Period+1, "-MDI");
            //Series ATR = Utilities.Convert_DoubleList_to_Series(ADX_TrueRange_EMA, ADX_TrueRange_Period, "ATR");
            Series ATR = Utilities.Convert_DoubleList_to_Series(TrueRange_EMA, 1 + ADX_TrueRange_Period, "-ATR");
            ADX_.Color = Color.Purple;
            P_DI.Color = Color.Green;
            M_DI.Color = Color.Red;
            ATR.Color = Color.Black;
            return new Series[]{ADX_, P_DI, M_DI, ATR};
        }

        public Series CollectLocalMax()
        {
            Series LocalMax_Series = Utilities.Convert_StockPoints_to_Series(LocalMaxs, symbol, SeriesChartType.Point, Color.Green);
            LocalMax_Series.Name = "MaxDV";
            return LocalMax_Series;
        }

        public Series CollectLocalMin()
        {
            Series LocalMin_Series = Utilities.Convert_StockPoints_to_Series(LocalMins, symbol, SeriesChartType.Point, Color.Red);
            LocalMin_Series.Name = "MinDV";
            return LocalMin_Series;
        }
       
        
        public Object getStockPointByDate(int targetDate)
        {

            for (int i = 0; i < DailyDataCollection.Count; i++ )
            {
                if (DailyDataCollection[i].getDate() == targetDate)
                    return new Object[]{DailyDataCollection[i], i};
            }
            return null;
        }


        #endregion

        #region Overbought Oversold indicators
        public bool RSI_OverBought(int index)
        {
            if (RSI.Tables.Count == 0)
                return false;
            double Current_RSI = Convert.ToDouble(RSI.Tables["RSI"].Rows[index][1].ToString());
            if (Current_RSI >= RSI_UpperBound)
                return true;
            else
                return false;
        }

        public bool RSI_OverSold(int index)
        {
            if (RSI.Tables.Count == 0)
                return false;
            double Current_RSI = Convert.ToDouble(RSI.Tables["RSI"].Rows[index][1].ToString());
            if (Current_RSI <= RSI_LowerBound)
                return true;
            else
                return false;
        }

        public bool Stochastic_Overbought(int StochIndex, int StochSMAIndex)
        {
            if (Stochastic.Count == 0)
                return false;
            double current_Stochastic = Stochastic[StochIndex];
            double current_Stochastic_SMA = Stochastic_SMA[StochSMAIndex];
            double previous_Stochastic = Stochastic[StochIndex - 1];

            if (current_Stochastic <= RSI_UpperBound && previous_Stochastic >= RSI_UpperBound && current_Stochastic < current_Stochastic_SMA)
                return true;
            else
                return false;
        }

        public bool Stochastic_Oversold(int StochIndex, int StochSMAIndex)
        {
            if (Stochastic.Count == 0)
                return false;
            double current_Stochastic = Stochastic[StochIndex];
            double current_Stochastic_SMA = Stochastic_SMA[StochSMAIndex];
            double previous_Stochastic = Stochastic[StochIndex - 1];

            //Continue here

            if (current_Stochastic >= RSI_LowerBound && previous_Stochastic <= RSI_LowerBound && current_Stochastic > current_Stochastic_SMA)
                return true;
            else
                return false;
        }
        #endregion

        #region Analysis
        #region Frequent Calls from Single Stock Analyzer
        public List<Series> ConvertAllDataToSeriesData()
        {
            List<Series> result = new List<Series>();

            ModifyDatesForGraphing();
            Series PriceCandle = Utilities.Convert_EnhancedStockData_to_PriceSeries(DailyDataCollection, getSymbol(), chartType);
            PriceCandle.Color = Color.Blue;

            Series PriceLine = Utilities.Convert_EnhancedStockData_to_PriceSeries(DailyDataCollection, getSymbol()+"_Line", SeriesChartType.Line);
            PriceLine.Color = Color.Beige;
            PriceLine.Enabled = false;
                        
            Series Volume = Utilities.Convert_EnhancedStockData_to_VolumeSeries(DailyDataCollection, "Volume");
            Volume.Color = Color.Orange;
            for (int i = 0; i < Volume.Points.Count; i++)
            {
                if (DailyDataCollection[i].getDirection() == 1)
                    Volume.Points[i].Color = volUp;
                if (DailyDataCollection[i].getDirection() ==-1)
                    Volume.Points[i].Color = volDown;
            }

            Volume.Color = Color.Blue;

            //Series VolumeUp = Utilities.Convert_EnhancedStockData_to_VolumeUpSeries(DailyDataCollection, "VolumeUp");
            //Volume.Color = Color.Green;

            //Series VolumeDown = Utilities.Convert_EnhancedStockData_to_VolumeDownSeries(DailyDataCollection, "VolumeDown");
            //Volume.Color = Color.Red;

            result.Add(PriceCandle);
            result.Add(Volume);
            //result.Add(VolumeUp);
            //result.Add(VolumeDown);
            result.Add(PriceLine);            
            return result;
            
        }

        public void ModifyDatesForGraphing()
        {
            for (int i = 0; i < DailyDataCollection.Count; i++)
            {
                DailyDataCollection[i].setDateMod(i);
            }
        }

        public void ProcessAnalyzedResults(Series PriceLine)
        {
            MA_SMAQ = Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ);
            MA_SMAS = Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALS);
            MA_SMAI = Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALI);
            MA_SMAL = Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALL);

            MA_EMAQ = Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ);
            MA_EMAS = Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALS);
            MA_EMAI = Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALI);
            MA_EMAL = Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALL);
            
            
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALQ, "SMAQ");
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALS, "SMAS");
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALI, "SMAI");
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_SMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALL, "SMAL");

            
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALQ, "SMAQ");
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALS, "SMAS");
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALI, "SMAI");
            //Utilities.Convert_DoubleList_to_Series(Utilities.Convert_DoubleList_to_EMA_List(Utilities.Convert_Series_to_DoubleList(PriceLine), Utilities.INTERVALQ), Utilities.INTERVALL, "SMAL");
            

            //Start analyzing based on moving averages
            
            CalculateBollingerBands();
            //CalculateBuySellSignals();
            CalculateRSI();
            CalculateStochastic();
            CalculateForceIndex();
            Calculate_MACD();
            Calculate_HeikinAshi();
            CalculateChannel();
            CalculateADX();
            ComplexSignalGeneration(Utilities.BuySettings, Utilities.SellSettings);
            CalculateDivergeance();
           
            //ManageRisk();
        }
        #endregion

        //TODO:Continue here
        public double ForcedStart(int date, int index)
        {
            int currentDate = DailyDataCollection[index].getDate();
            int length = DailyDataCollection.Count();
            if (currentDate != date)
                Utilities.incosistentData++;
                //throw new Exception ("Date did not line up as expected");

            Indicator forced = new Indicator(Utilities.IndicatorSignalNames.ForcedEntry);           

            forced.addSignal(new Signal(Utilities.Command.Buy, index, date, DailyDataCollection[index].getClose(), Utilities.exchange,  symbol));
            Signal exit = RiskManagement.CalculateGreedyBullExit(this, date, DailyDataCollection[index].getClose(), 100);
            forced.addSignal(exit);
            Indicators.Clear();
            Indicators.Add(forced);

            double buyPrice = DailyDataCollection[index].getClose();
            double sellPrice = exit.price;
            double profitPercent = (sellPrice - buyPrice) / buyPrice;
            return profitPercent;
        }

        #region HeikinAshi
        public void Calculate_HeikinAshi()
        {
            Indicator indicator = new Indicator(Utilities.IndicatorSignalNames.HeikenAshi);
            for (int i = 0; i < DailyDataCollection.Count; i++)
            {
                double open = DailyDataCollection[i].getOpen();
                double high = DailyDataCollection[i].getHigh();
                double low = DailyDataCollection[i].getLow();
                double close = DailyDataCollection[i].getClose();
                int date = DailyDataCollection[i].getDateMod();
                uint volume = DailyDataCollection[i].getVolume();
                string symbol = DailyDataCollection[i].getSymbol();
                HeikinAshi.Add(new HeikenAshiStockPoint(date, volume, open, high, low, close, symbol));
            }

            for (int i = 1; i < HeikinAshi.Count; i++)
            {
                double open = HeikinAshi[i].getOpen();
                double high = HeikinAshi[i].getHigh();
                double low = HeikinAshi[i].getLow();
                double close = HeikinAshi[i].getClose();
                double openPrevious = HeikinAshi[i-1].getOpen();
                double closePrevious = HeikinAshi[i-1].getClose();
                double xClose = (open + high + low + close)/4;
                double xOpen = (openPrevious + closePrevious)/2;
                double xHigh = Math.Max(Math.Max(high, xOpen),xClose);
                double xLow = Math.Min(Math.Min(low,xOpen),xClose);

                HeikinAshi[i].setClose(xClose);
                HeikinAshi[i].setOpen(xOpen);
                HeikinAshi[i].setHigh(xHigh);
                HeikinAshi[i].setLow(xLow);
                HeikinAshi[i].CrunchHeikenAshiSignal();
            }

            int comboLength = Utilities.HeikenAshi_period;
            for (int i = comboLength; i < HeikinAshi.Count; i++)
            {                
                bool buy = true;
                bool sell = true;
                bool exit = true;
                for (int j = 0; j < comboLength; j++)
                {
                    if (HeikinAshi[i - j].getHeikenAshiSignal() != Utilities.HeikenAshiType.Bullish)
                        buy = false;
                    if (HeikinAshi[i - j].getHeikenAshiSignal() != Utilities.HeikenAshiType.Bearish)
                        sell = false;
                    if (HeikinAshi[i - j].getHeikenAshiSignal() != Utilities.HeikenAshiType.Reversal)
                        exit = false;
                }
                if (buy == true && sell == true && exit == true)
                    throw new Exception("Somethings gone horribly wrong with HeikenAshi");

                if(buy)
                    indicator.addSignal(new Signal(Utilities.Command.Buy, i, DailyDataCollection[i].getDate(), DailyDataCollection[i].getClose(), Utilities.exchange, symbol));
                if(sell)
                    indicator.addSignal(new Signal(Utilities.Command.Sell, i, DailyDataCollection[i].getDate(), DailyDataCollection[i].getClose(), Utilities.exchange, symbol));
                //if (sell)
                //    indicator.addSignal(new Signal(Utilities.Command.Sell, i, DailyDataCollection[i].getDate(), DailyDataCollection[i].getClose(), Utilities.exchange, symbol));

            }
            Indicators.Add(indicator);
        }

        #endregion

        #region Crunch Trends
        //Trend Strength calculated over last INTERVALQ days of quick trend
        //Trend Strength calculated over last INTERVALS days of short trend
        //Trend Strength calculated over last INTERVALI days of intermediate trend
        //Trend Strength calculated over last INTERVALL days of long trend
        //Rule of thumb, twice the amount of days is needed as the interval length to calculate these        
        
       
        #endregion

        #region Calculate Cross Overs
                        
        #endregion

        

        #region Bollinger Bands
        //TODO: Do something with this
        public void CalculateBollingerBands()
        {
            if (MA_SMAS != null)
            {
                BollingerBands = new DataSet();
                BollingerBands.Tables.Add(new DataTable("Bollinger Upper Band"));
                BollingerBands.Tables["Bollinger Upper Band"].Columns.Add("X Values", typeof(Double));
                BollingerBands.Tables["Bollinger Upper Band"].Columns.Add("Y Values", typeof(Double));

                BollingerBands.Tables.Add(new DataTable("Bollinger Lower Band"));
                BollingerBands.Tables["Bollinger Lower Band"].Columns.Add("X Values", typeof(Double));
                BollingerBands.Tables["Bollinger Lower Band"].Columns.Add("Y Values", typeof(Double));

                BollingerBands.Tables.Add(new DataTable("Bollinger Upper Mid Band"));
                BollingerBands.Tables["Bollinger Upper Mid Band"].Columns.Add("X Values", typeof(Double));
                BollingerBands.Tables["Bollinger Upper Mid Band"].Columns.Add("Y Values", typeof(Double));

                BollingerBands.Tables.Add(new DataTable("Bollinger Lower Mid Band"));
                BollingerBands.Tables["Bollinger Lower Mid Band"].Columns.Add("X Values", typeof(Double));
                BollingerBands.Tables["Bollinger Lower Mid Band"].Columns.Add("Y Values", typeof(Double));

                for (int i = 0; i < MA_SMAS.Count; i++)
                {
                    //TODO:Hotfix
                    double x = i + Utilities.INTERVALS - 1;
                    double Savg =MA_SMAS[i];
                    double Std = 0;
                    for (int j = INTERVALS - 1 + i; j >= i; j--)
                        Std += Math.Pow(DailyDataCollection[j].getClose() - Savg, 2);
                    Std = Math.Sqrt(Std / INTERVALS);
                    
                    double ybollinger_upper = Savg + BollingerSTDRatio * Std ;
                    double ybollinger_lower = Savg - BollingerSTDRatio * Std;
                    BollingerBands.Tables["Bollinger Upper Band"].Rows.Add(x, ybollinger_upper);
                    BollingerBands.Tables["Bollinger Lower Band"].Rows.Add(x, ybollinger_lower);

                    double ybollinger_upper_mid = Savg + BollingerSTDRatio * Std/2;
                    double ybollinger_lower_mid = Savg - BollingerSTDRatio * Std/2;
                    BollingerBands.Tables["Bollinger Upper Mid Band"].Rows.Add(x, ybollinger_upper_mid);
                    BollingerBands.Tables["Bollinger Lower Mid Band"].Rows.Add(x, ybollinger_lower_mid);
                }
            }
        }
        #endregion

        #region Channel
        public void CalculateChannel()
        {
            if (MA_EMAS != null)
            {
                List<Double> deviations = new List<double>();
                EnvelopeUpperChannel = new DataSet();
                EnvelopeLowerChannel = new DataSet();

                EnvelopeUpperChannel.Tables.Add(new DataTable("Envelope Upper Channel"));
                EnvelopeUpperChannel.Tables["Envelope Upper Channel"].Columns.Add("X Values", typeof(Double));
                EnvelopeUpperChannel.Tables["Envelope Upper Channel"].Columns.Add("Y Values", typeof(Double));
                
                EnvelopeLowerChannel.Tables.Add(new DataTable("Envelope Lower Channel"));
                EnvelopeLowerChannel.Tables["Envelope Lower Channel"].Columns.Add("X Values", typeof(Double));
                EnvelopeLowerChannel.Tables["Envelope Lower Channel"].Columns.Add("Y Values", typeof(Double));

                for (int i = 0; i < MA_EMAS.Count; i++)
                {
                    deviations.Add(Math.Abs(MA_EMAS[i] - DailyDataCollection[i + INTERVALS - 1].getHigh()));
                    deviations.Add(Math.Abs(MA_EMAS[i] - DailyDataCollection[i + INTERVALS - 1].getLow()));
                }
                deviations.Sort();

                while (deviations.Count > 2 * 0.95 * MA_EMAS.Count)
                    deviations.RemoveAt(deviations.Count - 1);
                                
                double deviation95 = 0;

                if (deviations.Count > 0)
                    deviation95 = deviations[deviations.Count - 1];

                for (int i = 0; i < MA_EMAS.Count; i++)
                {
                    double x = i + Utilities.INTERVALS - 1; ;
                    double yupper = MA_EMAS[i] + deviation95;
                    double ylower = MA_EMAS[i] - deviation95;

                    EnvelopeUpperChannel.Tables["Envelope Upper Channel"].Rows.Add(x, yupper);
                    EnvelopeLowerChannel.Tables["Envelope Lower Channel"].Rows.Add(x, ylower);
                }                
            }
        }
        #endregion

        #region Divergeance
        public void CalculateDivergeance()
        {
            List<Utilities.StockPoint> Stockpoints = Utilities.Convert_EnhancedStockData_to_StockPoints(DailyDataCollection);
            List<Utilities.StockPoint> MaxStockpoints = Utilities.Convert_EnhancedStockData_to_StockPoints(DailyDataCollection);
            List<Utilities.StockPoint> MinStockpoints = Utilities.Convert_EnhancedStockData_to_StockPoints(DailyDataCollection);
            List<Utilities.StockPoint> DXbyDY = new List<Utilities.StockPoint>();
            

            //will break if stock has 2 points or less
            for (int i = 1; i < Stockpoints.Count; i++)
            {
                Double dx = Stockpoints[i].x;
                Double dy = Stockpoints[i].y - Stockpoints[i-1].y;
                Utilities.StockPoint newStockPoint = new Utilities.StockPoint();
                newStockPoint.x = dx;
                newStockPoint.y = dy;
                LocalMaxs.Add(newStockPoint);
            }

            for (int i = 1; i < LocalMaxs.Count; i++)
            {
                Double dx = LocalMaxs[i].x;
                Double dy = LocalMaxs[i].y - LocalMaxs[i - 1].y;
                Utilities.StockPoint newStockPoint = new Utilities.StockPoint();
                newStockPoint.x = dx;
                newStockPoint.y = dy;
                LocalMins.Add(newStockPoint);
            }

            //Stockpoints.OrderBy<
            /*
            var sortedSignals = from sig in Stockpoints
                                orderby sig.y ascending
                                select sig;

            List<Utilities.StockPoint> MaxtoMinStockPoitns = sortedSignals.ToList<Utilities.StockPoint>();

            for (int i = 0; i < MaxtoMinStockPoitns.Count; i++)
            {
            }
             */ 
        }
        #endregion

        #region MACD
        public void Calculate_MACD()
        {
            if (DailyDataCollection.Count >= MACD_shortEMA_period)
            {
                MACD_shortEMA = new List<double>(DailyDataCollection.Count);

                double runningAvg = 0;
                double esf = (Double)2 / (MACD_shortEMA_period + 1);
                for (int i = 0; i < MACD_shortEMA_period; i++)
                {
                    runningAvg += DailyDataCollection[i].getClose();
                }

                runningAvg = runningAvg / MACD_shortEMA_period;
                //Initialize EMA with SMA
                MACD_shortEMA.Add(runningAvg);

                for (int i = MACD_shortEMA_period; i < DailyDataCollection.Count; i++)
                {
                    double EMA_Today = (DailyDataCollection[i].getClose() * esf) +
                        (MACD_shortEMA[MACD_shortEMA.Count - 1] * (1 - esf));
                    MACD_shortEMA.Add(EMA_Today);
                }
            }

            if (DailyDataCollection.Count >= MACD_longEMA_period)
            {
                MACD_longEMA = new List<double>(DailyDataCollection.Count);

                double runningAvg = 0;
                double esf = (Double)2 / (MACD_longEMA_period + 1);
                for (int i = 0; i < MACD_longEMA_period; i++)
                {
                    runningAvg += DailyDataCollection[i].getClose();
                }

                runningAvg = runningAvg / MACD_longEMA_period;
                //Initialize EMA with SMA
                MACD_longEMA.Add(runningAvg);

                for (int i = MACD_longEMA_period; i < DailyDataCollection.Count; i++)
                {
                    double EMA_Today = (DailyDataCollection[i].getClose() * esf) +
                        (MACD_longEMA[MACD_longEMA.Count - 1] * (1 - esf));
                    MACD_longEMA.Add(EMA_Today);
                }

                for (int i = 0; i < MACD_longEMA.Count; i++)
                {
                    MACD_long_short_diff.Add(MACD_shortEMA[i + MACD_longEMA_period - MACD_shortEMA_period] - MACD_longEMA[i]);
                }
            }
            
            if (DailyDataCollection.Count >= MACD_longEMA_period + MACD_signalEMA_period)
            {
                MACD_signalEMA = new List<double>(MACD_long_short_diff.Count);

                double runningAvg = 0;
                double esf = (Double)2 / (MACD_signalEMA_period + 1);
                for (int i = 0; i < MACD_signalEMA_period; i++)
                {
                    runningAvg += MACD_long_short_diff[i];
                }

                runningAvg = runningAvg / MACD_signalEMA_period;
                //Initialize EMA with SMA
                MACD_signalEMA.Add(runningAvg);

                for (int i = MACD_signalEMA_period; i < MACD_long_short_diff.Count; i++)
                {
                    double EMA_Today = (MACD_long_short_diff[i] * esf) +
                        (MACD_signalEMA[MACD_signalEMA.Count - 1] * (1 - esf));
                    MACD_signalEMA.Add(EMA_Today);
                }
                
                
                for (int i = 0; i < MACD_signalEMA.Count; i++)
                {
                    MACD_Histogram.Add(MACD_long_short_diff[i + MACD_signalEMA_period -1] - MACD_signalEMA[i]);
                    //MACD_Histogram.Add(MACD_longEMA[i] - MACD_shortEMA[i + MACD_longEMA_period - MACD_shortEMA_period]);
                }               
            }
            
        }
        #endregion

        #region Force Index
        public void CalculateForceIndex()
        {
            if (DailyDataCollection.Count <= ForceIndex_EMA_period)
                Console.WriteLine("Unable to calculate Force Index ");
            else
            {
                for (int i = 1; i < DailyDataCollection.Count; i++)
                {
                    ForceIndex.Add((DailyDataCollection[i].getClose() - DailyDataCollection[i - 1].getClose()) * DailyDataCollection[i].getVolume());
                }

                ForceIndex_EMA = Utilities.Convert_DoubleList_to_EMA_List(ForceIndex, ForceIndex_EMA_period);
            }
        }

        #endregion

        #region Relative Strength Index
        public void CalculateRSI()
        {
            if (DailyDataCollection.Count <= RSI_period)
                Console.WriteLine("Unable to calculate RSI");
            else
            {
                RSI.Tables.Add(new DataTable("RSI"));
                RSI.Tables["RSI"].Columns.Add("X Values", typeof(Double));
                RSI.Tables["RSI"].Columns.Add("Y Values", typeof(Double));

                Console.WriteLine("RSI!");
                for (int i = RSI_period; i < DailyDataCollection.Count; i++)
                {
                    double avgGain = 0;
                    double avgLoss = 0;
                    for (int j = i - (RSI_period-1); j <= i; j++)
                    {
                        double close = DailyDataCollection[j].getClose();
                        double open = DailyDataCollection[j].getOpen();
                        if (DailyDataCollection[j].getClose() - DailyDataCollection[j-1].getClose() > 0)
                            avgGain += DailyDataCollection[j].getClose() - DailyDataCollection[j-1].getClose();
                        if (DailyDataCollection[j].getClose() - DailyDataCollection[j-1].getClose() < 0)
                            avgLoss += Math.Abs(DailyDataCollection[j].getClose() - DailyDataCollection[j-1].getClose());
                        /*
                        if (DailyDataCollection[j].getDirection() == 1)
                            avgGain += (DailyDataCollection[j].getClose() - DailyDataCollection[j].getOpen());
                        if (DailyDataCollection[j].getDirection() == -1)
                            avgLoss += (DailyDataCollection[j].getOpen() - DailyDataCollection[j].getClose());
                        Console.WriteLine(open - close);
                        */ 
                    }
                    avgGain /= RSI_period;
                    avgLoss /= RSI_period;
                    double RS_ = avgGain / avgLoss;
                    double RSI_ = 100 - 100 / (1 + RS_);
                    RSI.Tables["RSI"].Rows.Add(i, RSI_);

                    //int dz = DailyDataCollection[i].getDate();
                    
                    //Console.WriteLine(DailyDataCollection[i].getClose() + "," + i + "," + RSI_);
                }                
            }
        }
        #endregion

        #region Stochastic
        public void CalculateStochastic()
        {
            Stochastic_D_Period = 3;
            if (DailyDataCollection.Count <= Stochastic_K_Period)
                Console.WriteLine("Unable to calculate RSI");
            else
            {
                //Console.WriteLine("Stochastic!");
                for (int i = Stochastic_K_Period; i < DailyDataCollection.Count; i++)
                {
                    double nhigh = double.MinValue;
                    double nlow = double.MaxValue;
                    double currentStoch = 0;
                    double close = DailyDataCollection[i].getClose();

                    for (int j = i - (Stochastic_K_Period - 1); j <= i; j++)
                    {
                        
                        double high = DailyDataCollection[j].getHigh();
                        double low = DailyDataCollection[j].getLow();
                        
                        if (nhigh < high)
                            nhigh = high;
                        if(nlow > low)
                            nlow = low;
                    }

                    currentStoch = 100*(close - nlow)/(nhigh - nlow) ;
                    Stochastic.Add(currentStoch);
                }

                
                Stochastic_SMA = Utilities.Convert_DoubleList_to_SMA_List(Stochastic, Stochastic_D_Period);
            }
        }
        #endregion

        //TODO: Continue here
        #region RSI Stochastic
        public void CalculateRSIStochastic()
        {
            if (RSI.Tables["RSI"].Rows.Count <= RSI_period)
                Console.WriteLine("Unable to calculate Stochastic RSI");
            else
            {
                RSI_Stochastic.Tables.Add(new DataTable("RSI_Stochastic"));
                RSI_Stochastic.Tables["RSI_Stochastic"].Columns.Add("X Values", typeof(Double));
                RSI_Stochastic.Tables["RSI_Stochastic"].Columns.Add("Y Values", typeof(Double));

                Console.WriteLine("RSI_Stochastic!");
                for (int i = RSI_period; i < RSI.Tables["RSI"].Rows.Count; i++)
                {
                    double nhigh = double.MinValue;
                    double nlow = double.MaxValue;
                    double currentRSI = 0;
                    double currentX = Convert.ToDouble(RSI.Tables["RSI"].Rows[i]["X Values"].ToString());

                    for (int j = i-RSI_period; j <= i; j++)
                    {
                        currentRSI = Convert.ToDouble(RSI.Tables["RSI"].Rows[j]["Y Values"].ToString());
                        
                        if (nhigh < currentRSI)
                            nhigh = currentRSI;
                        if (nlow > currentRSI)
                            nlow = currentRSI;
                    }

                    double RSIStoch = 100*(currentRSI - nlow) / (nhigh - nlow);
                    RSI_Stochastic.Tables["RSI_Stochastic"].Rows.Add(currentX, RSIStoch);
                }
            }
        }
        #endregion

        #region ADX
        /*
         * A = Today’s High – Yesterday’s High
         * B = Yesterday’s Low – Today’s Low
         * Depending upon the values of A and B, three possible scenarios are:
         * Values	            Scenarios
         * Both A and B < 0	    +DM=0, -DM=0
         * A > B	            +DM=A, -DM=0
         * A < B	            +DM=0, -DM=B
         */
        public void CalculateADX()
        {            
            Indicator indicator = new Indicator(Utilities.IndicatorSignalNames.ATR);

            TrueRange = new List<double>();
            TrueRange_EMA = new List<double>();
            P_DM = new List<double>();
            N_DM = new List<double>();
            Plus_DI = new List<double>();
            Minus_DI = new List<double>();
            DX = new List<double>();
            ADX = new List<double>();

            #region load test data
            /*
            List<EnhancedSimpleStockPoint> test = new List<EnhancedSimpleStockPoint>();
            test.Add(new EnhancedSimpleStockPoint(0, 0, 29.8720, 30.1983, 29.4072, 29.8720, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.2381, 30.2776, 29.3182, 30.2381, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.0996, 30.4458, 29.9611, 30.0996, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.9028, 29.3478, 28.7443, 28.9028, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.9225, 29.3477, 28.5566, 28.9225, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.4775, 29.2886, 28.4081, 28.4775, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.5566, 28.8334, 28.0818, 28.5566, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.5576, 28.7346, 27.4289, 27.5576, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.4675, 28.6654, 27.6565, 28.4675, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.2796, 28.8532, 27.8345, 28.2796, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.4882, 28.6356, 27.3992, 27.4882, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.2310, 27.6761, 27.0927, 27.2310, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 26.3507, 27.2112, 26.1826, 26.3507, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 26.3309, 26.8651, 26.1332, 26.3309, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.0333, 27.4090, 26.6277, 27.0333, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 26.2221, 26.9441, 26.1332, 26.2221, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 26.0144, 26.5189, 25.4307, 26.0144, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 25.4605, 26.5189, 25.3518, 25.4605, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.0333, 27.0927, 25.8760, 27.0333, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.4487, 27.6860, 26.9640, 27.4487, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.3586, 28.4477, 27.1421, 28.3586, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.4278, 28.5267, 28.0123, 28.4278, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 27.9530, 28.6654, 27.8840, 27.9530, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 29.0116, 29.0116, 27.9928, 29.0116, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 29.3776, 29.8720, 28.7643, 29.3776, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 29.3576, 29.8028, 29.1402, 29.3576, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 28.9107, 29.7529, 28.7127, 28.9107, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.6149, 30.6546, 28.9290, 30.6149, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.0502, 30.5951, 30.0304, 30.0502, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.1890, 30.7635, 29.3863, 30.1890, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 31.1202, 31.1698, 30.1365, 31.1202, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.5356, 30.8923, 30.4267, 30.5356, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 29.7827, 30.0402, 29.3467, 29.7827, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.0402, 30.6645, 29.9906, 30.0402, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 30.4861, 30.5951, 29.5152, 30.4861, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 31.4670, 31.9724, 30.9418, 31.4670, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.0515, 32.1011, 31.5364, 32.0515, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 31.9724, 32.0317, 31.3580, 31.9724, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 31.1302, 31.6255, 30.9220, 31.1302, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 31.6551, 31.8534, 31.1994, 31.6551, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.6360, 32.7055, 32.1308, 32.6360, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.5866, 32.7648, 32.2298, 32.5866, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.1903, 32.5766, 31.9724, 32.1903, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.1011, 32.1308, 31.5562, 32.1011, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.9335, 33.1215, 32.2101, 32.9335, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.0027, 33.1909, 32.6262, 33.0027, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 31.9425, 32.5172, 31.7642, 31.9425, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.3883, 32.4379, 31.7840, 32.3883, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.4875, 33.2207, 32.0912, 32.4875, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.8046, 32.8343, 32.1903, 32.8046, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.3792, 33.6169, 32.7648, 33.3792, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.4188, 33.7459, 33.0423, 33.4188, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.1711, 33.5971, 33.0522, 33.1711, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.6268, 34.0825, 33.3297, 33.6268, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.9638, 34.5780, 33.7260, 33.9638, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.0529, 34.2214, 33.6962, 34.0529, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.7266, 34.7663, 34.2015, 34.7266, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.6969, 34.7364, 34.3105, 34.6969, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.7067, 35.0140, 34.1420, 34.7067, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.8944, 34.9447, 33.5674, 33.8944, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.9142, 34.4194, 33.5674, 33.9142, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.0331, 34.3995, 33.3692, 34.0331, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.6169, 34.1619, 33.2108, 33.6169, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 32.7154, 33.3396, 32.6560, 32.7154, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.0819, 33.3892, 32.7747, 33.0819, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.0621, 33.5079, 32.9235, 33.0621, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.9240, 33.9638, 33.0820, 33.9240, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.0825, 34.4194, 33.6368, 34.0825, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.9638, 34.7167, 33.8647, 33.9638, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.3396, 33.9440, 33.0027, 33.3396, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 33.2306, 33.6567, 33.0127, 33.2306, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.4691, 34.5086, 32.8738, 34.4691, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.2312, 34.8653, 34.1124, 34.2312, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.6275, 34.7464, 33.8944, 34.6275, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.0536, 35.1725, 34.4393, 35.0536, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.0543, 36.1633, 35.2816, 36.0543, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.1038, 36.4504, 35.7768, 36.1038, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.9948, 36.0344, 35.5985, 35.9948, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.4011, 36.4504, 36.0048, 36.4011, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.4407, 36.7380, 36.0839, 36.4407, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.3318, 36.6091, 35.7868, 36.3318, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.6091, 36.8270, 36.3318, 36.6091, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.4803, 36.8369, 35.9552, 36.4803, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.4803, 36.8865, 36.4110, 36.4803, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.3119, 36.3802, 35.8659, 36.3119, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.5688, 35.9948, 35.2516, 35.5688, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.2220, 35.8561, 35.1923, 35.2220, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.5578, 35.8759, 35.1230, 35.5578, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.4896, 35.7273, 35.2418, 35.4896, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.8703, 36.0688, 35.6225, 35.8703, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.7990, 35.6025, 34.7394, 34.7990, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.7196, 34.9775, 34.4915, 34.7196, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.3049, 35.5827, 34.9974, 35.3049, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.9993, 36.0688, 34.9974, 35.9993, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.0786, 36.2076, 35.7612, 36.0786, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.1580, 36.4555, 35.8307, 36.1580, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.0885, 36.4359, 35.8207, 36.0885, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.1084, 36.5448, 36.0985, 36.1084, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.3149, 35.8107, 35.2156, 35.3149, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.1263, 35.2552, 34.7593, 35.1263, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.2534, 35.2058, 34.2335, 34.2534, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.4320, 34.5908, 34.0252, 34.4320, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.4915, 34.7296, 34.3725, 34.4915, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 34.6403, 34.8586, 34.2833, 34.6403, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.3049, 35.3149, 34.2038, 35.3049, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 35.4338, 35.5034, 35.1165, 35.4338, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 36.6243, 36.6342, 35.8505, 36.6243, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 37.0608, 37.1401, 36.4259, 37.0608, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 37.2592, 37.2691, 36.8722, 37.2592, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 37.6162, 37.6956, 37.3087, 37.6162, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 37.8742, 37.8742, 37.3385, 37.8742, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.1917, 38.3801, 37.8245, 38.1917, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.0347, 39.1736, 38.0825, 39.0347, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.7372, 39.0546, 38.4693, 38.7372, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.0347, 39.0944, 38.5586, 39.0347, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.1538, 39.2729, 38.6181, 39.1538, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.0249, 39.1141, 38.6876, 39.0249, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.2530, 39.8581, 39.1935, 39.2530, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.1339, 39.5330, 39.0944, 39.1339, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.7193, 39.7392, 39.3225, 39.7193, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.7193, 39.8680, 39.4513, 39.7193, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.4117, 39.8185, 39.1439, 39.4117, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.0644, 39.6102, 38.9257, 39.0644, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.5605, 39.7491, 39.2729, 39.5605, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.2827, 39.5309, 39.0249, 39.2827, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.9454, 39.1836, 38.7372, 38.9454, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.5506, 39.8978, 38.8860, 39.5506, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.7689, 39.8383, 39.3225, 39.7689, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.3126, 39.6399, 38.9653, 39.3126, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.1717, 38.6975, 38.1520, 38.1717, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.7174, 38.7968, 38.3007, 38.7174, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.9852, 39.0844, 38.2312, 38.9852, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.4415, 39.5209, 38.8860, 39.4415, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.9671, 40.0368, 39.4217, 39.9671, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.9276, 40.2450, 39.7788, 39.9276, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.0468, 40.4335, 39.9276, 40.0468, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.9771, 40.2757, 39.7589, 39.9771, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.0763, 40.1458, 39.3522, 40.0763, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.1161, 40.7509, 39.8383, 40.1161, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.7093, 39.8780, 39.4713, 39.7093, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.9653, 40.2649, 38.8464, 38.9653, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 38.9356, 39.1538, 38.7272, 38.9356, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 39.1935, 39.2532, 38.7075, 39.1935, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.0368, 40.0566, 39.2035, 40.0368, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.4137, 40.4534, 40.1359, 40.4137, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.7609, 40.9593, 40.2649, 40.7609, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.1478, 41.1973, 40.6717, 41.1478, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.1874, 41.2867, 40.8898, 41.1874, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.3461, 41.3759, 40.8799, 41.3461, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.4750, 41.6140, 41.2371, 41.4750, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.0802, 42.1397, 41.5049, 42.0802, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.0703, 42.3183, 41.8719, 42.0703, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.1406, 42.3193, 41.9122, 42.1406, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.2696, 42.3988, 41.8625, 42.2696, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.3492, 42.5279, 42.1404, 42.3492, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.1505, 42.8655, 42.0781, 42.1505, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.7732, 42.3888, 41.4752, 41.7732, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.4059, 41.7931, 41.2867, 41.4059, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.1107, 42.3591, 41.5944, 42.1107, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.9221, 42.3194, 41.7335, 41.9221, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.9519, 42.3019, 41.3065, 41.9519, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.7107, 41.8328, 40.7107, 40.7107, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.5916, 40.9590, 40.4327, 40.5916, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.9192, 41.1079, 40.5419, 40.9192, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.6440, 41.8427, 40.9298, 41.6440, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.7633, 41.7832, 41.4752, 41.7633, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.9420, 42.3193, 41.8427, 41.9420, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.1803, 42.2498, 41.7633, 42.1803, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.2696, 42.5676, 41.9817, 42.2696, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.2795, 42.4485, 42.0711, 42.2795, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.8555, 42.9876, 42.5476, 42.8555, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.7561, 42.8158, 42.5676, 42.7561, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.4781, 42.6867, 42.1803, 42.4781, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.9052, 42.9647, 42.2895, 42.9052, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.9151, 43.1533, 42.6370, 42.9151, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.8356, 43.5109, 42.7561, 42.8356, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.0044, 43.1832, 42.4881, 43.0044, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.8257, 43.4215, 42.7165, 42.8257, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.6867, 43.4513, 42.4881, 42.6867, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.0413, 42.8059, 41.8825, 42.0413, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.0980, 42.0214, 41.0086, 41.0980, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.7931, 41.8925, 41.2470, 41.7931, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.6711, 41.8526, 40.6313, 40.6711, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.8399, 41.2767, 40.3532, 40.8399, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 40.9689, 41.0285, 40.5519, 40.9689, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 41.0384, 41.5747, 40.9888, 41.0384, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.0512, 42.1009, 41.4852, 42.0512, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.2995, 42.3492, 41.7832, 42.2995, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.2030, 43.2129, 42.5775, 43.2030, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.3122, 43.4627, 43.0938, 43.3122, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.5903, 43.8286, 43.3024, 43.5903, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.3421, 43.8484, 43.2428, 43.3421, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.6995, 43.8286, 43.3221, 43.6995, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.1464, 44.3350, 43.8088, 44.1464, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.2854, 44.2854, 43.9378, 44.2854, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0401, 44.1761, 43.7392, 44.0401, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.3520, 43.7690, 43.0442, 43.3520, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.1336, 43.2428, 42.9747, 43.1336, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.8286, 44.0868, 43.5506, 43.8286, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.6796, 43.8384, 43.4215, 43.6796, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.8683, 43.9080, 43.6995, 43.8683, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.2030, 43.5208, 42.5973, 43.2030, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.2528, 43.3122, 42.8059, 43.2528, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.6995, 43.9876, 43.5704, 43.6995, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.7591, 44.1761, 43.6697, 43.7591, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.5803, 44.1861, 43.5704, 43.5803, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.8088, 44.4144, 43.3520, 43.8088, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.6002, 43.9776, 43.5109, 43.6002, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.3341, 43.6896, 43.0540, 43.3341, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.7690, 43.8088, 43.0143, 43.7690, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.9876, 44.2257, 43.9279, 43.9876, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.8188, 44.2754, 43.6400, 43.8188, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.2357, 44.2854, 43.8782, 44.2357, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.9876, 44.3549, 43.8584, 43.9876, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0469, 44.3846, 43.9776, 44.0469, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.5109, 43.8981, 43.4513, 43.5109, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.2246, 44.2446, 43.7671, 44.2246, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.7220, 44.8911, 44.4335, 44.7220, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.9905, 45.0502, 44.7916, 44.9905, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.3188, 45.3287, 44.9707, 45.3188, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.7366, 45.7366, 45.3586, 45.7366, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9753, 46.0548, 45.6968, 45.9753, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.7863, 46.0052, 45.7466, 45.7863, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9256, 46.0151, 45.7167, 45.9256, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.5078, 46.0301, 45.5078, 45.5078, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1743, 46.2389, 46.0251, 46.1743, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1743, 46.2538, 45.9157, 46.1743, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.8957, 46.3036, 45.8262, 45.8957, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9256, 46.0251, 45.6769, 45.9256, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3036, 46.3036, 45.6868, 46.3036, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1146, 46.3931, 45.8759, 46.1146, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.5376, 45.8957, 45.2890, 45.5376, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1046, 46.2439, 45.3685, 46.1046, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1444, 46.2737, 45.9753, 46.1444, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.6073, 46.3036, 45.4083, 45.6073, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3434, 46.3931, 45.7068, 46.3434, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.6769, 46.3572, 45.1896, 45.6769, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2492, 46.1046, 45.0602, 45.2492, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.9263, 45.2392, 43.8069, 43.9263, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0755, 44.3639, 43.8865, 44.0755, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.1152, 44.6524, 43.8169, 44.1152, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.4635, 44.6126, 43.7771, 44.4635, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.3194, 44.1949, 43.0908, 43.3194, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.5635, 43.7869, 42.4044, 42.5635, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.0310, 43.0515, 42.6530, 43.0310, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.4189, 43.5483, 42.8022, 43.4189, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.6576, 43.7372, 43.1901, 43.6576, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.3944, 43.4289, 42.3944, 42.3944, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.7524, 42.7923, 41.8969, 42.7524, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.4442, 42.9514, 42.4143, 42.4442, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.8818, 43.2797, 42.5337, 42.8818, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.7923, 43.0808, 42.5237, 42.7923, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.4388, 43.5582, 42.5337, 43.4388, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.5283, 43.6478, 42.9315, 43.5283, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0854, 44.1152, 43.6179, 44.0854, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.3341, 44.3341, 44.0257, 44.3341, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6126, 44.6921, 44.2146, 44.6126, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.5927, 44.8115, 44.3838, 44.5927, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.5032, 44.7916, 44.3241, 44.5032, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.9263, 44.5032, 43.7174, 43.9263, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.3739, 44.5529, 44.0854, 44.3739, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.3639, 44.4635, 43.5980, 44.3639, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.5230, 44.6225, 44.1649, 44.5230, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.1696, 45.2492, 44.7121, 45.1696, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.3089, 45.6073, 45.2094, 45.3089, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.3586, 45.5775, 45.1896, 45.3586, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.5078, 45.5675, 45.1596, 45.5078, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1942, 46.2937, 45.7466, 46.1942, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.2837, 46.3931, 46.1942, 46.2837, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.5423, 46.8009, 46.1543, 46.5423, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.9203, 47.0298, 46.5523, 46.9203, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.0993, 47.0993, 46.7313, 47.0993, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.1093, 47.2784, 46.8507, 47.1093, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.9899, 47.1292, 46.6617, 46.9899, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.2884, 47.3579, 46.9800, 47.2884, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.4177, 47.6464, 47.2784, 47.4177, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.5768, 47.6265, 47.3480, 47.5768, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.2896, 47.7476, 47.1203, 47.2896, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.7177, 47.9069, 47.0505, 47.7177, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.0488, 48.0961, 47.5683, 48.0488, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.8173, 47.9667, 47.7078, 47.8173, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.7476, 48.3949, 47.6978, 47.7476, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.7974, 48.1260, 47.5385, 47.7974, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.0264, 48.1956, 47.8969, 48.0264, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.1858, 48.3451, 47.8571, 48.1858, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.9567, 48.2454, 47.8671, 47.9567, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.9567, 48.4944, 47.5883, 47.9567, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.4048, 48.5143, 47.9367, 48.4048, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.5442, 48.6936, 48.1858, 48.5442, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.4247, 48.6637, 48.1658, 48.4247, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.5343, 48.6139, 48.0363, 48.5343, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.8230, 48.8429, 48.4297, 48.8230, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.8629, 48.9923, 48.7334, 48.8629, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.1118, 49.1416, 48.6538, 49.1118, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.6993, 49.7093, 49.2910, 49.6993, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.9184, 49.9781, 49.6594, 49.9184, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.3209, 49.9084, 48.9923, 49.3209, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.2910, 49.4503, 48.6936, 49.2910, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.5400, 49.6694, 49.2213, 49.5400, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.8188, 49.9781, 49.5151, 49.8188, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.0976, 50.1474, 49.0520, 50.0976, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.3067, 50.3565, 49.8785, 50.3067, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.1972, 50.4362, 50.0876, 50.1972, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.1318, 50.2171, 49.0023, 49.1318, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.1616, 49.4205, 48.7732, 49.1616, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.0180, 50.1176, 49.4005, 50.0180, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.0296, 50.0778, 48.9923, 49.0296, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.7192, 49.9582, 49.2213, 49.7192, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.2256, 49.1118, 47.8770, 48.2256, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.9767, 48.2952, 47.4389, 47.9767, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3734, 48.1161, 41.3746, 46.3734, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2183, 46.6025, 44.0964, 45.2183, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.5683, 47.5982, 47.1103, 47.5683, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.5185, 48.1858, 47.0007, 47.5185, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.4147, 48.4545, 47.6978, 48.4147, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.6480, 48.5840, 47.5286, 47.6480, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.7319, 47.3194, 46.1842, 46.7319, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.8812, 47.0705, 45.8756, 46.8812, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.2340, 47.3791, 46.0249, 46.2340, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.8656, 46.4332, 45.3577, 45.8656, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.1628, 45.0889, 44.0632, 44.1628, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6507, 45.4474, 43.3065, 44.6507, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.4714, 45.1337, 44.4516, 44.4714, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.5113, 44.5710, 43.0476, 44.5113, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0134, 45.3776, 43.9338, 44.0134, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.6763, 45.6963, 44.8996, 45.6763, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.4075, 45.7958, 45.0092, 45.4075, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.9892, 46.0547, 44.9394, 44.9892, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.0547, 46.0847, 44.9394, 46.0547, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.4929, 46.5725, 45.8854, 46.4929, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.9021, 46.0945, 44.7478, 44.9021, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0831, 45.2681, 44.0334, 44.0831, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0034, 44.2923, 43.4060, 44.0034, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.6350, 44.7205, 43.5056, 43.6350, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.8798, 44.9296, 44.0532, 44.8798, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.3079, 45.3478, 44.4216, 45.3079, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2980, 46.0448, 45.2183, 45.2980, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.5128, 46.5725, 45.2613, 46.5128, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.7020, 46.9410, 46.3037, 46.7020, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.8514, 46.9809, 46.4431, 46.8514, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.8906, 47.2298, 46.7509, 46.8906, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.4915, 47.5690, 46.2221, 46.4915, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1324, 47.0303, 46.0426, 46.1324, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9428, 46.3618, 45.5836, 45.9428, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2444, 45.8829, 45.0948, 45.2444, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.1646, 45.5537, 44.8454, 45.1646, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.0050, 45.4440, 44.7057, 45.0050, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.2690, 44.3565, 42.9697, 43.2690, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.6106, 43.5683, 42.5407, 42.6106, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.4909, 42.8600, 41.6728, 42.4909, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.3711, 42.7218, 41.9920, 42.3711, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 42.5008, 43.2690, 42.1516, 42.5008, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.8577, 43.9075, 42.6006, 43.8577, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.0971, 44.2767, 43.5783, 44.0971, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.5161, 44.5261, 43.9774, 44.5161, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6458, 44.9252, 44.3565, 44.6458, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2245, 45.3941, 44.6957, 45.2245, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.4539, 45.7034, 45.1347, 45.4539, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.4938, 45.6335, 44.8853, 45.4938, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.2368, 45.5219, 44.1969, 44.2368, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6159, 44.7057, 43.9973, 44.6159, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.1546, 45.1546, 43.7579, 45.1546, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.5361, 45.6535, 44.4563, 44.5361, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.6635, 45.8730, 45.1347, 45.6635, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9528, 45.9927, 45.2744, 45.9528, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3319, 46.3518, 45.8031, 46.3319, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3119, 46.6112, 46.1024, 46.3119, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9428, 46.4716, 45.7732, 45.9428, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.6036, 46.3020, 45.1447, 45.6036, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.7034, 45.9827, 44.9651, 45.7034, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.5614, 46.6811, 46.1025, 46.5614, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3618, 46.5913, 46.1423, 46.3618, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.8287, 46.8806, 46.3918, 46.8287, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.7210, 46.8108, 46.4117, 46.7210, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.6511, 46.7409, 45.9428, 46.6511, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.9704, 47.0801, 46.6811, 46.9704, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.5639, 46.8407, 46.1723, 46.5639, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2943, 45.8131, 45.1048, 45.2943, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.9352, 45.1347, 44.3465, 44.9352, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6159, 44.9551, 44.6059, 44.6159, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6957, 45.0050, 44.1969, 44.6957, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.2664, 45.6734, 44.9252, 45.2664, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.4440, 45.7133, 45.0050, 45.4440, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.7556, 45.3542, 44.4563, 44.7556, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.8154, 44.9252, 44.4363, 44.8154, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.3765, 45.2345, 44.3565, 44.3765, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.5484, 44.0173, 43.3638, 43.5484, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.9674, 44.1570, 43.1693, 43.9674, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.4386, 44.2168, 43.3987, 43.4386, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.9674, 44.0572, 42.8700, 43.9674, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.5085, 44.1470, 43.4985, 43.5085, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 43.3588, 43.7479, 43.0795, 43.3588, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 44.6558, 44.8055, 43.9674, 44.6558, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.1546, 45.1746, 44.6259, 45.1546, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.9029, 45.9129, 45.4440, 45.9029, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 45.5936, 45.9228, 45.5188, 45.5936, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.1423, 46.3419, 45.7133, 46.1423, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.3219, 46.5913, 46.2122, 46.3219, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 46.4915, 46.5614, 46.1423, 46.4915, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.1400, 47.2597, 46.8307, 47.1400, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.3395, 47.5890, 46.9704, 47.3395, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.6388, 47.6887, 47.0801, 47.6388, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.8284, 47.8683, 47.4293, 47.8284, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 47.9950, 48.1400, 47.7500, 47.9950, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.8299, 48.9300, 48.1101, 48.8299, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.8200, 49.1700, 48.6100, 48.8200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.6900, 49.0210, 48.4200, 48.6900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.6700, 49.1600, 48.3200, 48.6700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.6600, 49.6900, 49.1500, 49.6600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.3900, 49.7500, 49.3500, 49.3900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.3700, 49.5400, 48.5900, 49.3700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.2900, 49.5300, 49.1100, 49.2900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.0700, 49.8400, 48.7500, 49.0700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.0100, 49.5300, 48.7810, 49.0100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 48.4800, 49.0500, 48.2000, 48.4800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.6600, 49.7600, 49.0000, 49.6600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.2300, 49.7100, 48.9100, 49.2300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.4100, 49.5400, 49.0000, 49.4100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.7500, 49.8700, 49.0800, 49.7500, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 49.7700, 50.0200, 49.6200, 49.7700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.1100, 50.2100, 49.2600, 50.1100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.5200, 50.7500, 50.2800, 50.5200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.4200, 50.6400, 50.1700, 50.4200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.4900, 51.5000, 50.6300, 51.4900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.3000, 51.7200, 51.3000, 51.3000, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 50.8200, 51.3000, 50.4200, 50.8200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.1900, 51.5700, 50.8700, 51.1900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.2900, 51.7101, 50.7900, 51.2900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.6400, 51.6900, 51.2100, 51.6400, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.8900, 52.2300, 51.8500, 51.8900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.0300, 52.1500, 51.4200, 52.0300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.1900, 52.2300, 51.6600, 52.1900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.3000, 52.4500, 51.8400, 52.3000, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.1800, 52.4900, 52.1698, 52.1800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.2200, 52.7500, 51.9800, 52.2200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.7800, 52.9300, 52.5750, 52.7800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.0200, 53.0400, 52.3600, 53.0200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.6700, 53.8614, 53.5000, 53.6700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.6700, 53.8100, 53.5100, 53.6700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.7375, 53.8300, 53.4499, 53.7375, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.4500, 54.0401, 53.2100, 53.4500, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.7150, 53.7700, 53.1000, 53.7150, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.3850, 53.4800, 52.6600, 53.3850, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.5100, 53.3672, 52.1100, 52.5100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.3150, 52.8800, 52.2900, 52.3150, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.4500, 52.2462, 50.8500, 51.4500, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 51.6000, 51.8700, 51.3500, 51.6000, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.4300, 52.7900, 52.1300, 52.4300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.4700, 52.5900, 52.1400, 52.4700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.9100, 52.9100, 52.1700, 52.9100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.0700, 52.4500, 51.7700, 52.0700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.1200, 53.2500, 52.5600, 53.1200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.7700, 53.1300, 52.6700, 52.7700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.7300, 52.9000, 52.1000, 52.7300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 52.0850, 52.7355, 51.8800, 52.0850, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.1900, 53.4600, 52.8400, 53.1900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.7300, 53.8100, 53.2100, 53.7300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.8700, 53.9400, 53.5000, 53.8700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.8450, 53.9500, 53.6800, 53.8450, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 53.8800, 54.5200, 53.8200, 53.8800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.0800, 54.1500, 53.6899, 54.0800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.1350, 54.4400, 53.9500, 54.1350, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.4950, 54.5500, 54.0900, 54.4950, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.3000, 54.7400, 54.2700, 54.3000, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.3950, 54.6200, 54.2300, 54.3950, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.1600, 54.7000, 54.0300, 54.1600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.5800, 54.6600, 54.0600, 54.5800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.5200, 54.6800, 54.4100, 54.5200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.5600, 54.7600, 54.1600, 54.5600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.8900, 54.8900, 54.6200, 54.8900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.8850, 54.9600, 54.7900, 54.8850, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.7420, 54.8700, 54.6100, 54.7420, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.7700, 54.8600, 54.2100, 54.7700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.6700, 54.9200, 54.5500, 54.6700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.7900, 54.9000, 54.7300, 54.7900, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.6600, 54.8000, 54.5500, 54.6600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 54.4600, 54.6200, 54.2100, 54.4600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.3100, 55.6900, 54.9500, 55.3100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.2650, 55.5500, 54.9200, 55.2650, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.7400, 55.7600, 55.0700, 55.7400, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.9200, 55.9600, 55.6800, 55.9200, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.8700, 56.0500, 55.3200, 55.8700, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.0800, 56.1800, 55.5800, 56.0800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.1600, 56.3600, 55.9500, 56.1600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.5550, 56.5600, 56.2000, 56.5550, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.5750, 56.7300, 56.4100, 56.5750, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.0000, 57.0200, 56.4600, 57.0000, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.1600, 57.2300, 56.4900, 57.1600, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.5100, 57.2600, 56.3200, 56.5100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.1100, 56.3500, 55.6800, 56.1100, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.6800, 56.4900, 55.6500, 55.6800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.4500, 56.4600, 55.6800, 56.4500, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.5325, 56.5500, 56.0500, 56.5325, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.8300, 56.9800, 56.4500, 56.8300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.1800, 57.3500, 56.9200, 57.1800, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 55.7300, 57.2200, 55.4700, 55.7300, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.0000, 56.1600, 55.3900, 56.0000, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.0500, 57.1800, 56.3600, 57.0500, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 56.9550, 57.1700, 56.8400, 56.9550, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.0575, 57.1400, 56.4000, 57.0575, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.3750, 57.4200, 56.9000, 57.3750, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.6500, 57.9700, 57.4000, 57.6500, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 58.0250, 58.0700, 57.5600, 58.0250, "symbol"));
            test.Add(new EnhancedSimpleStockPoint(0, 0, 57.9300, 58.1200, 57.7500, 57.9300, "symbol"));
            


            DailyDataCollection = test;*/
            #endregion

            //TrueRange.Add(Math.Abs(DailyDataCollection[0].getHigh() - DailyDataCollection[0].getLow()));
            //P_DM.Add(0);
            //N_DM.Add(0);
            for (int i = 1; i < DailyDataCollection.Count; i++)
            {                
                double currentHigh = DailyDataCollection[i].getHigh();
                double currentLow = DailyDataCollection[i].getLow();
                double previousClose = DailyDataCollection[i-1].getClose();
                double previousHigh = DailyDataCollection[i-1].getHigh();
                double previousLow = DailyDataCollection[i-1].getLow();

                double Method1 = Math.Abs(currentHigh - currentLow);
                double Method2 = Math.Abs(currentHigh - previousClose);
                double Method3 = Math.Abs(currentLow - previousClose);
                double TR = (new List<double> { Method1, Method2, Method3 }).Max();
                TrueRange.Add(TR);

                double upMove = currentHigh - previousHigh;
                double downMove = previousLow - currentLow;

                if ( ((upMove < 0) && (downMove < 0)) || (upMove == downMove))
                {
                    P_DM.Add(0);
                    N_DM.Add(0);
                }

                else if (upMove > downMove)
                {
                    P_DM.Add(upMove);
                    N_DM.Add(0);
                }

                else if (upMove < downMove)
                {
                    P_DM.Add(0);
                    N_DM.Add(downMove);
                }
                else
                    throw new Exception("omg");

            }
            //ADX_TrueRange_EMA = Utilities.Convert_DoubleList_to_SMA_List(ADX_TrueRange, ADX_TrueRange_Period);
            TrueRange_EMA = Utilities.Convert_DoubleList_to_WilderSmoothedTR_List(TrueRange, ADX_TrueRange_Period);

            P_DM_EMA = Utilities.Convert_DoubleList_to_WilderSmoothedTR_List(P_DM, ADX_TrueRange_Period);
            N_DM_EMA = Utilities.Convert_DoubleList_to_WilderSmoothedTR_List(N_DM, ADX_TrueRange_Period);

            List<Double> TR14 = Utilities.Convert_DoubleList_to_X14(TrueRange, ADX_TrueRange_Period);
            List<Double> PDM14 = Utilities.Convert_DoubleList_to_X14(P_DM, ADX_TrueRange_Period);
            List<Double> NDM14 = Utilities.Convert_DoubleList_to_X14(N_DM, ADX_TrueRange_Period);
            List<Double> PDI14 = Utilities.Convert_DoubleList_to_XI14(TR14, PDM14);
            List<Double> NDI14 = Utilities.Convert_DoubleList_to_XI14(TR14, NDM14);


            //Calculate Plus and Minus DI
            for (int i = 0; i < TrueRange_EMA.Count; i++)
            {
                Plus_DI.Add(100 * P_DM_EMA[i] / TrueRange_EMA[i]);
                Minus_DI.Add(100 * N_DM_EMA[i] / TrueRange_EMA[i]);
            }

            for (int i = 0; i < Plus_DI.Count; i++)
            {
                double difference = Math.Abs(PDI14[i] - NDI14[i]);
                double sum = PDI14[i] + NDI14[i];
                double DXentry = 100 * difference / sum;
                DX.Add(DXentry);
            }
            
            
            ADX = Utilities.Convert_DoubleList_to_WilderSmoothedADX_List(DX, ADX_TrueRange_Period);
            //ADX = Utilities.Convert_DoubleList_to_WilderSmoothedADX_List(DX, ADX_TrueRange_Period);
            for (int i = 1; i < Utilities.ADX_smoothing_iterations; i++)
            {
                ADX = Utilities.Convert_DoubleList_to_SMA_List(ADX, ADX_TrueRange_Period);
            }
        }
        #endregion


        public delegate int ChangeInt(int x);

        static public int DoubleIt(int x)
        {
            return x * 2;
        }


        #region Complex Signal Generation
        public Indicator ComplexSignalGeneration(TradeSettings BuySetting, TradeSettings SellSetting)
        {            
            if ((BuySetting == null) && (SellSetting == null))
            {
                Indicator customIndicator1 = new Indicator(Utilities.IndicatorSignalNames.CustomXOver);

                int Day1 = DailyDataCollection[0].getDateMod();
                int Date1 = DailyDataCollection[0].getDate();

                int Day2 = DailyDataCollection[DailyDataCollection.Count - 1].getDateMod();
                int Date2 = DailyDataCollection[DailyDataCollection.Count - 1].getDate();

                customIndicator1.addSignal(new Signal(Utilities.Command.Buy, Day1, Date1, DailyDataCollection[0].getClose(), Utilities.exchange, symbol));
                customIndicator1.addSignal(new Signal(Utilities.Command.Sell, Day2, Date2, DailyDataCollection[DailyDataCollection.Count-1].getClose(), Utilities.exchange, symbol));

                //customIndicator1 = ComplexSignalComparisonCrunch(SellSetting, customIndicator1);
                customIndicator1.crunchSignalPairs();
                customIndicator1.reportProfits();


                Indicators.Add(customIndicator1);
            }

            Indicator customIndicator = new Indicator(Utilities.IndicatorSignalNames.Custom);

            if (BuySetting == null)
                return null;

            try
            {
                Series[] ADX_Indicators = CollectADX();
                Series ADX = ADX_Indicators[0];
                Series ADX_PDI = ADX_Indicators[1];
                Series ADX_NDI = ADX_Indicators[2];

                int Length_StockPointCount = DailyDataCollection.Count;
                int Length_HeikinAshi = HeikinAshi.Count;
                int Length_Bollinger = DailyDataCollection.Count;
                if (BollingerBands != null)    
                    Length_Bollinger = BollingerBands.Tables["Bollinger Upper Mid Band"].Rows.Count;
                int Length_RSI = RSI.Tables["RSI"].Rows.Count;
                int Length_Stochastic = Stochastic.Count;
                int Length_StochasticSMA = Stochastic_SMA.Count;
                int Length_MACD_short = MACD_shortEMA.Count;
                int Length_MACD_long = MACD_longEMA.Count;
                int Length_MACD_longshortdiff = MACD_long_short_diff.Count;
                int Length_MACD_signal = MACD_signalEMA.Count;
                int Length_MACD_histogram = MACD_Histogram.Count;
               
                int Length_ForceEMA = ForceIndex_EMA.Count;
                int Length_ADX = ADX.Points.Count;
                int Length_ADX_PDI = ADX_PDI.Points.Count;
                int Length_ADX_NDI = ADX_NDI.Points.Count;

                int Length_MA_SMAQ = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_SMAQ = MA_SMAQ.Count;

                int Length_MA_SMAS = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_SMAS = MA_SMAS.Count;

                int Length_MA_SMAI = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_SMAI = MA_SMAI.Count;

                int Length_MA_SMAL = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_SMAL = MA_SMAL.Count;

                int Length_MA_EMAQ = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_EMAQ = MA_EMAQ.Count;

                int Length_MA_EMAS = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_EMAS = MA_EMAS.Count;

                int Length_MA_EMAI = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_EMAI = MA_EMAI.Count;

                int Length_MA_EMAL = DailyDataCollection.Count;
                if (MA_SMAS != null)
                    Length_MA_EMAL = MA_EMAL.Count;

                //int Length_DI = PD

                int maxLength = (new List<int> 
                { Length_StockPointCount, Length_HeikinAshi, Length_Bollinger, Length_RSI, Length_Stochastic, Length_MACD_histogram, Length_StochasticSMA,
                    Length_Stochastic, Length_MACD_short, Length_MACD_long, Length_MACD_longshortdiff, Length_MACD_signal,Length_ADX, Length_ADX_NDI, Length_ADX_PDI, Length_ForceEMA,
                    Length_MA_SMAQ, Length_MA_SMAS, Length_MA_SMAI, Length_MA_SMAL, Length_MA_EMAQ, Length_MA_EMAS, Length_MA_EMAI, Length_MA_EMAL}).Max(); ;

                int minLength = (new List<int> 
                { Length_StockPointCount, Length_HeikinAshi, Length_Bollinger, Length_RSI, Length_Stochastic, Length_MACD_histogram, Length_StochasticSMA,
                    Length_Stochastic, Length_MACD_short, Length_MACD_long, Length_MACD_longshortdiff,  Length_MACD_signal,Length_ADX, Length_ADX_NDI, Length_ADX_PDI, Length_ForceEMA,
                    Length_MA_SMAQ, Length_MA_SMAS, Length_MA_SMAI, Length_MA_SMAL, Length_MA_EMAQ, Length_MA_EMAS, Length_MA_EMAI, Length_MA_EMAL}).Min(); ;


                List<EnhancedSimpleStockPoint> DailyData = this.getDailyData();
                

                //Prep Bollinger
                List<Series> Bollingers = this.CollectBollingerBands();
                //Series BollingerUpperBand = Bollingers[0];
                //Series BollingerLowerBand = Bollingers[1];

               

                
                for (int i = maxLength - minLength; i < maxLength; i++)
                {
                    Boolean ConditionsPassed_Buy = true;
                    Boolean BollingerPassed_Buy = false;
                    Boolean ForcePassed_Buy = false;
                    Boolean HeikenAshiPassed_Buy = false;
                    Boolean MACDPassed_Buy = false;
                    Boolean RSIPassed_Buy = false;
                    Boolean StochasticPassed_Buy = false;
                    Boolean DojiPassed_Buy = false;
                    Boolean ADX_Passed_Buy = false;
                    Boolean DI_Passed_Buy = false;
                    Boolean ADXTrending_Passed_Buy = false;
                    Boolean MAXtoY_Passed_Buy = false;
                    Boolean MAStrength_Passed_Buy = false;
                    Boolean MA_Stock_vs_MA_Passed_Buy = false;

                    Boolean ConditionsPassed_Sell = true;
                    Boolean BollingerPassed_Sell = false;
                    Boolean ForcePassed_Sell = false;
                    Boolean HeikenAshiPassed_Sell = false;
                    Boolean MACDPassed_Sell = false;
                    Boolean RSIPassed_Sell = false;
                    Boolean StochasticPassed_Sell = false;
                    Boolean DojiPassed_Sell = false;
                    Boolean ADX_Passed_Sell = false;
                    Boolean DI_Passed_Sell = false;
                    Boolean ADXTrending_Passed_Sell = false;
                    Boolean MAXtoY_Passed_Sell = false;
                    Boolean MAStrength_Passed_Sell = false;
                    Boolean MA_Stock_vs_MA_Passed_Sell = false;


                    int DailyDataIndex = i;                    
                    int Day = DailyData[DailyDataIndex].getDateMod();
                    int Date = DailyData[DailyDataIndex].getDate();

                    //Bollinger section             
                    if (BuySetting.FilterBy_Bollinger)
                    {
                        int BollingerIndex = Length_Bollinger - maxLength + i;
                        int SMASIndex = Length_MA_SMAS - maxLength + i;
                        Double MA_Value = MA_SMAS[SMASIndex];
                        Double UpperBandValue = Convert.ToDouble(Bollingers[0].Points[BollingerIndex].YValues[0].ToString());
                        Double LowerBandValue = Convert.ToDouble(Bollingers[1].Points[BollingerIndex].YValues[0].ToString());
                        Double Price_High = DailyData[DailyDataIndex].getHigh();
                        Double Price_Low = DailyData[DailyDataIndex].getLow();

                        BollingerPassed_Buy = BuySetting.MeetsBollingerRequirement_Buy.Invoke
                            (new Double[] { Price_High, Price_Low }, new Double[] { UpperBandValue, LowerBandValue }, MA_Value);
                    }

                    //Force Section
                    if (BuySetting.FilterBy_Force)
                    {
                        int ForceEMAIndex = Length_ForceEMA - maxLength + i;
                        Double ForceValue = ForceIndex_EMA[ForceEMAIndex];
                        ForcePassed_Buy = BuySetting.MeetsForceRequirements_Buy.Invoke(ForceValue);
                    }

                    //Heiken Ashi Section
                    if (BuySetting.FilterBy_HeikenAshi)
                    {
                        int passedcounter = 0;
                        for (int j = 0; j < BuySetting.HeikenAshiStrength; j++)
                        {
                            int HeikenAshiIndex = Length_HeikinAshi - maxLength + i;
                            HeikenAshiStockPoint targetHeikenashiPoint = HeikinAshi[HeikenAshiIndex - j];
                            if (BuySetting.MeetsHeikenAshiRequirement_Buy.Invoke(targetHeikenashiPoint, BuySetting.HeikenAshiType))
                                passedcounter++;
                        }
                        if (passedcounter == BuySetting.HeikenAshiStrength)
                            HeikenAshiPassed_Buy = true;
                    }

                    //MACD Section
                    if (BuySetting.FilterBy_MACD)
                    {
                        int MACDIndex = Length_MACD_longshortdiff - maxLength + i;
                        int MACDSignalIndex = Length_MACD_signal - maxLength + i;
                        double MacdValue = MACD_long_short_diff[MACDIndex];
                        double MacdSignal = MACD_signalEMA[MACDSignalIndex];
                        MACDPassed_Buy =
                            BuySetting.MeetsMACD_AroundSignal_Requirements_Buy(MacdValue, MacdSignal) &&
                            BuySetting.MeetsMACD_AroundZero_Requirements_Buy(MacdValue, MacdSignal);
                    }

                    //Stochastic
                    if (BuySetting.FilterBy_Stochastic)
                    {
                        int StochasticIndex = Length_Stochastic - maxLength + i;
                        int StochasticSMAIndex = Length_StochasticSMA - maxLength + i;
                        StochasticPassed_Buy = BuySetting.MeetsStochasticRequirements_Buy(this, StochasticIndex, StochasticSMAIndex);
                    }

                    //RSI
                    if (BuySetting.FilterBy_RSI)
                    {
                        int RSIIndex = Length_RSI - maxLength + i;
                        RSIPassed_Buy = BuySetting.MeetsRSIRequirements_Buy(this, RSIIndex);
                    }

                    //Doji
                    if (BuySetting.FilterBy_Doji)
                    {
                        DojiPassed_Buy = BuySetting.MeetsDojiRequirements_Buy(this, i);
                    }

                    //MA
                    if (BuySetting.FilterBy_MA)
                    {
                        MAXtoY_Passed_Buy = BuySetting.MeetsMA_XtoY_Buy(this, i);
                        MAStrength_Passed_Buy = BuySetting.MeetsMATrendStrengthRequirements_Buy(this, i);
                        MA_Stock_vs_MA_Passed_Buy = BuySetting.MeetsStockvsMA_Buy(this, i);
                    }


                    //ADX
                    if (BuySetting.Filterby_ADX)
                    {
                        int currentADXIndex = Length_ADX - maxLength + i;
                        int currentPDIIndex = Length_ADX_PDI - maxLength + i;
                        int currentNDIIndex = Length_ADX_NDI - maxLength + i;

                        Double currentADX = ADX.Points[currentADXIndex].YValues[0];
                        Double currentPDI = ADX_PDI.Points[currentPDIIndex].YValues[0];
                        Double currentNDI = ADX_NDI.Points[currentNDIIndex].YValues[0];

                        ADX_Passed_Buy = BuySetting.MeetsADXRequirements_Buy(currentADX);
                        DI_Passed_Buy = BuySetting.MeetsDIRequirements_Buy(currentPDI, currentNDI);
                        ADXTrending_Passed_Buy = BuySetting.MeetsADXTrendingRequirements_Buy(ADX, currentADXIndex);
                    }


                    if (!BollingerPassed_Buy && BuySetting.FilterBy_Bollinger)
                        ConditionsPassed_Buy = false;

                    if (!ForcePassed_Buy && BuySetting.FilterBy_Force)
                        ConditionsPassed_Buy = false;

                    if (!HeikenAshiPassed_Buy && BuySetting.FilterBy_HeikenAshi)
                        ConditionsPassed_Buy = false;

                    if (!MACDPassed_Buy && BuySetting.FilterBy_MACD)
                        ConditionsPassed_Buy = false;

                    if (!RSIPassed_Buy && BuySetting.FilterBy_RSI)
                        ConditionsPassed_Buy = false;

                    if (!StochasticPassed_Buy && BuySetting.FilterBy_Stochastic)
                        ConditionsPassed_Buy = false;
                     
                    if (!DojiPassed_Buy && BuySetting.FilterBy_Doji)
                        ConditionsPassed_Buy = false;

                    if (!ADX_Passed_Buy && BuySetting.Filterby_ADX)
                        ConditionsPassed_Buy = false;

                    if (!DI_Passed_Buy && BuySetting.Filterby_ADX)
                        ConditionsPassed_Buy = false;

                    if (!ADXTrending_Passed_Buy && BuySetting.Filterby_ADX)
                        ConditionsPassed_Buy = false;

                    if (!MAXtoY_Passed_Buy && BuySetting.FilterBy_MA)
                        ConditionsPassed_Buy = false;

                    if (!MAStrength_Passed_Buy && BuySetting.FilterBy_MA)
                        ConditionsPassed_Buy = false;

                    if (!MA_Stock_vs_MA_Passed_Buy && BuySetting.FilterBy_MA)
                        ConditionsPassed_Buy = false;


                    if (SellSetting != null)
                    {
                        //Bollinger section             
                        if (SellSetting.FilterBy_Bollinger)
                        {
                            int BollingerIndex = Length_Bollinger - maxLength + i;
                            int MA_SMASIndex = Length_MA_SMAS - maxLength + i;
                            Double MA_Value = MA_SMAS[MA_SMASIndex];
                            Double UpperBandValue = Convert.ToDouble(Bollingers[0].Points[BollingerIndex].YValues[0].ToString());
                            Double LowerBandValue = Convert.ToDouble(Bollingers[1].Points[BollingerIndex].YValues[0].ToString());
                            Double Price_High = DailyData[DailyDataIndex].getHigh();
                            Double Price_Low = DailyData[DailyDataIndex].getLow();

                            BollingerPassed_Sell = SellSetting.MeetsBollingerRequirement_Sell.Invoke
                                (new Double[] { Price_High, Price_Low }, new Double[] { UpperBandValue, LowerBandValue }, MA_Value);
                        }

                        //Force Section
                        if (SellSetting.FilterBy_Force)
                        {
                            int ForceEMAIndex = Length_ForceEMA - maxLength + i;
                            Double ForceValue = ForceIndex_EMA[ForceEMAIndex];
                            ForcePassed_Sell = SellSetting.MeetsForceRequirements_Sell.Invoke(ForceValue);
                        }

                        //Heiken Ashi Section
                        if (SellSetting.FilterBy_HeikenAshi)
                        {
                            int passedcounter = 0;
                            for (int j = 0; j < SellSetting.HeikenAshiStrength; j++)
                            {
                                int HeikenAshiIndex = Length_HeikinAshi - maxLength + i;
                                HeikenAshiStockPoint targetHeikenashiPoint = HeikinAshi[HeikenAshiIndex - j];
                                if (SellSetting.MeetsHeikenAshiRequirement_Sell.Invoke(targetHeikenashiPoint, SellSetting.HeikenAshiType))
                                    passedcounter++;
                            }
                            if (passedcounter == SellSetting.HeikenAshiStrength)
                                HeikenAshiPassed_Sell = true;
                        }

                        //MACD Section
                        if (SellSetting.FilterBy_MACD)
                        {
                            int MACDIndex = Length_MACD_longshortdiff - maxLength + i;
                            int MACDSignalIndex = Length_MACD_signal - maxLength + i;
                            double MacdValue = MACD_long_short_diff[MACDIndex];
                            double MacdSignal = MACD_signalEMA[MACDSignalIndex];
                            MACDPassed_Sell =
                                SellSetting.MeetsMACD_AroundSignal_Requirements_Sell(MacdValue, MacdSignal) &&
                                SellSetting.MeetsMACD_AroundZero_Requirements_Sell(MacdValue, MacdSignal);
                        }

                        if (SellSetting.FilterBy_Stochastic)
                        {
                            int StochasticIndex = Length_Stochastic - maxLength + i;
                            int StochasticSMAIndex = Length_StochasticSMA - maxLength + i;
                            StochasticPassed_Sell = SellSetting.MeetsStochasticRequirements_Sell(this, StochasticIndex, StochasticSMAIndex);
                        }

                        if (SellSetting.FilterBy_RSI)
                        {
                            int RSIIndex = Length_RSI - maxLength + i;
                            RSIPassed_Sell = SellSetting.MeetsRSIRequirements_Sell(this, RSIIndex);
                        }

                        if (SellSetting.FilterBy_Doji)
                        {
                            DojiPassed_Sell = SellSetting.MeetsDojiRequirements_Sell(this, i);
                        }

                        //MA
                        if (SellSetting.FilterBy_MA)
                        {
                            MAXtoY_Passed_Sell = SellSetting.MeetsMA_XtoY_Sell(this, i);
                            MAStrength_Passed_Sell = SellSetting.MeetsMATrendStrengthRequirements_Sell(this, i);
                            MA_Stock_vs_MA_Passed_Sell = SellSetting.MeetsStockvsMA_Sell(this, i);
                        }

                        if (SellSetting.Filterby_ADX)
                        {
                            int currentADXIndex = Length_ADX - maxLength + i;
                            int currentPDIIndex = Length_ADX_PDI - maxLength + i;
                            int currentNDIIndex = Length_ADX_NDI - maxLength + i;

                            Double currentADX = ADX.Points[currentADXIndex].YValues[0];
                            Double currentPDI = ADX_PDI.Points[currentPDIIndex].YValues[0];
                            Double currentNDI = ADX_NDI.Points[currentNDIIndex].YValues[0];

                            ADX_Passed_Sell = SellSetting.MeetsADXRequirements_Sell(currentADX);
                            DI_Passed_Sell = SellSetting.MeetsDIRequirements_Sell(currentPDI, currentNDI);
                            ADXTrending_Passed_Sell = SellSetting.MeetsADXTrendingRequirements_Sell(ADX, currentADXIndex);
                        }
                    }

                    if (SellSetting != null)
                    {
                        if (!BollingerPassed_Sell && SellSetting.FilterBy_Bollinger)
                            ConditionsPassed_Sell = false;

                        if (!ForcePassed_Sell && SellSetting.FilterBy_Force)
                            ConditionsPassed_Sell = false;

                        if (!HeikenAshiPassed_Sell && SellSetting.FilterBy_HeikenAshi)
                            ConditionsPassed_Sell = false;

                        if (!MACDPassed_Sell && SellSetting.FilterBy_MACD)
                            ConditionsPassed_Sell = false;

                        if (!RSIPassed_Sell && SellSetting.FilterBy_RSI)
                            ConditionsPassed_Sell = false;

                        if (!StochasticPassed_Sell && SellSetting.FilterBy_Stochastic)
                            ConditionsPassed_Sell = false;

                        if (!DojiPassed_Sell && SellSetting.FilterBy_Doji)
                            ConditionsPassed_Sell = false;

                        if (!ADX_Passed_Sell && SellSetting.Filterby_ADX)
                            ConditionsPassed_Sell = false;

                        if (!DI_Passed_Sell && SellSetting.Filterby_ADX)
                            ConditionsPassed_Sell = false;

                        if (!ADXTrending_Passed_Sell && SellSetting.Filterby_ADX)
                            ConditionsPassed_Sell = false;

                        if (!MAXtoY_Passed_Sell && SellSetting.FilterBy_MA)
                            ConditionsPassed_Sell = false;

                        if (!MAStrength_Passed_Sell && SellSetting.FilterBy_MA)
                            ConditionsPassed_Sell = false;

                        if (!MA_Stock_vs_MA_Passed_Sell && SellSetting.FilterBy_MA)
                            ConditionsPassed_Sell = false;
                    }
                    else
                    {
                        ConditionsPassed_Sell = false;
                    }

                    if (ConditionsPassed_Buy)
                        customIndicator.addSignal(new Signal(Utilities.Command.Buy, Day, Date, DailyDataCollection[DailyDataIndex].getClose(), Utilities.exchange, symbol));


                    if (ConditionsPassed_Sell)
                        customIndicator.addSignal(new Signal(Utilities.Command.Sell, Day, Date, DailyDataCollection[DailyDataIndex].getClose(), Utilities.exchange, symbol));

                    
                    //Clears redundant signals
                    

                }
                //customIndicator.removeRedundantLastSignals();
                customIndicator.name = Utilities.IndicatorSignalNames.CustomXOver;

                if (customIndicator.signals.Count > 0)
                {
                    customIndicator = ComplexSignalComparisonCrunch(SellSetting, customIndicator);

                    customIndicator.crunchSignalPairs();

                    customIndicator.reportProfits();
                }

                    Indicators.Add(customIndicator);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Should enable all filters");                
            }
            return customIndicator;
            
 
        }
        public Indicator ComplexSignalComparisonCrunch(TradeSettings SellSetting, Indicator customIndicator)
        {
            for (int i = customIndicator.signals.Count - 1; i > 0; i--)
            {
                Signal currentSignal = customIndicator.signals[i];
                Signal previousSignal = customIndicator.signals[i - 1];

                if (currentSignal.dayMod == previousSignal.dayMod + 1 && currentSignal.signal == previousSignal.signal)
                    customIndicator.signals.RemoveAt(i);
            }

            if (SellSetting != null)
                customIndicator.removeRedundantLastSignals();

            //Maximum gain!
            if (Utilities.SelectedExit == Utilities.ExitType.BestProfit)
            {
                for (int i = 0; i < customIndicator.signals.Count; i++)
                {
                    if (customIndicator.signals[i].signal == Utilities.Command.Sell)
                        customIndicator.signals.RemoveAt(i--);
                }

                List<EnhancedSimpleStockPoint> DailyData = this.getDailyData();
                for (int i = 1; i < customIndicator.signals.Count; i++)
                {
                    Signal firstSignal = customIndicator.signals[i - 1];
                    Signal secondSignal = customIndicator.signals[i];

                    int currentMaxIndex = firstSignal.dayMod;

                    for (int j = firstSignal.dayMod; j < secondSignal.dayMod; j++)
                    {
                        if (DailyData[j].getClose() > DailyData[currentMaxIndex].getClose())
                            currentMaxIndex = j;
                    }

                    Signal currentMaxSignal = new Signal(Utilities.Command.Sell, DailyData[currentMaxIndex].getDateMod(), DailyData[currentMaxIndex].getDate(), DailyData[currentMaxIndex].getClose(), Utilities.exchange, firstSignal.symbol);

                    if (currentMaxSignal.dayMod > firstSignal.dayMod && currentMaxSignal.dayMod < secondSignal.dayMod)
                    {
                        customIndicator.addSignal(currentMaxSignal);
                        customIndicator.signals = customIndicator.signals.OrderBy(signal => signal.dayMod).ToList();

                        if (firstSignal.signal == secondSignal.signal)
                            i++;
                    }
                }
            }

            //CutLossExit
            if (Utilities.SelectedExit == Utilities.ExitType.CutLosses)
            {
                double risk = Utilities.riskTolerance;
                double capital = Utilities.capital;

                List<EnhancedSimpleStockPoint> DailyData = this.getDailyData();
                for (int i = 1; i < customIndicator.signals.Count; i=i+2)
                {
                    Signal firstSignal = customIndicator.signals[i - 1];
                    Signal secondSignal = customIndicator.signals[i];

                    if (firstSignal.signal == secondSignal.signal)
                        throw new Exception("FFFFFFFFFUU");

                    int currentSellPoint = firstSignal.dayMod;

                    double boughtPrice = DailyData[currentSellPoint].getClose();
                    double cutoff = DailyData[currentSellPoint].getClose() * (1 - risk / 100);

                    for (int j = firstSignal.dayMod; j < secondSignal.dayMod; j++)
                    {
                        if (DailyData[j].getClose() < cutoff)
                        {
                            currentSellPoint = j;
                            break;
                        }
                    }

                    Signal currentMaxSignal = new Signal(Utilities.Command.Sell, DailyData[currentSellPoint].getDateMod(), DailyData[currentSellPoint].getDate(), DailyData[currentSellPoint].getClose(), Utilities.exchange, firstSignal.symbol);

                    if (currentMaxSignal.dayMod > firstSignal.dayMod && currentMaxSignal.dayMod < secondSignal.dayMod)
                    {
                        customIndicator.addSignal(currentMaxSignal);
                        customIndicator.signals = customIndicator.signals.OrderBy(signal => signal.dayMod).ToList();

                        if (firstSignal.signal == secondSignal.signal)
                            i++;
                    }
                }
            }

            //Maximum Loss!
            if (Utilities.SelectedExit == Utilities.ExitType.WorstLoss)
            {
                for (int i = 0; i < customIndicator.signals.Count; i++)
                {
                    if (customIndicator.signals[i].signal == Utilities.Command.Sell)
                        customIndicator.signals.RemoveAt(i--);
                }

                List<EnhancedSimpleStockPoint> DailyData = this.getDailyData();
                for (int i = 1; i < customIndicator.signals.Count; i++)
                {
                    Signal firstSignal = customIndicator.signals[i - 1];
                    Signal secondSignal = customIndicator.signals[i];

                    int currentMinIndex = firstSignal.dayMod;

                    for (int j = firstSignal.dayMod; j < secondSignal.dayMod; j++)
                    {
                        if (DailyData[j].getClose() < DailyData[currentMinIndex].getClose())
                            currentMinIndex = j;
                    }

                    Signal currentMaxSignal = new Signal(Utilities.Command.Sell, DailyData[currentMinIndex].getDateMod(), DailyData[currentMinIndex].getDate(), DailyData[currentMinIndex].getClose(), Utilities.exchange, firstSignal.symbol);

                    if (currentMaxSignal.dayMod > firstSignal.dayMod && currentMaxSignal.dayMod < secondSignal.dayMod)
                    {
                        customIndicator.addSignal(currentMaxSignal);
                        customIndicator.signals = customIndicator.signals.OrderBy(signal => signal.dayMod).ToList();

                        if (firstSignal.signal == secondSignal.signal)
                            i++;
                    }
                }
            }

            //KeepProfits
            if (Utilities.SelectedExit == Utilities.ExitType.KeepProfits)
            {
                double risk = Utilities.riskTolerance;
                double capital = Utilities.capital;

                List<EnhancedSimpleStockPoint> DailyData = this.getDailyData();
                for (int i = 1; i < customIndicator.signals.Count; i = i + 2)
                {
                    Signal firstSignal = customIndicator.signals[i - 1];
                    Signal secondSignal = customIndicator.signals[i];

                    if (firstSignal.signal == secondSignal.signal)
                        throw new Exception("FFFFFFFFFUU");

                    int currentSellPoint = firstSignal.dayMod;

                    double boughtPrice = DailyData[currentSellPoint].getClose();
                    double cutoff = boughtPrice * (1 - risk / 100);
                    double maxPrice = boughtPrice;
                    //loop between the two signals
                    for (int j = firstSignal.dayMod; j < secondSignal.dayMod; j++)
                    {
                        if (DailyData[j].getClose() > maxPrice)
                            maxPrice = DailyData[j].getClose();
                        cutoff = maxPrice * (1 - risk / 100);

                        if (DailyData[j].getClose() < cutoff)
                        {
                            currentSellPoint = j;
                            break;
                        }
                    }

                    Signal currentMaxSignal = new Signal(Utilities.Command.Sell, DailyData[currentSellPoint].getDateMod(), DailyData[currentSellPoint].getDate(), DailyData[currentSellPoint].getClose(), Utilities.exchange, firstSignal.symbol);

                    if (currentMaxSignal.dayMod > firstSignal.dayMod && currentMaxSignal.dayMod < secondSignal.dayMod)
                    {
                        customIndicator.addSignal(currentMaxSignal);
                        customIndicator.signals = customIndicator.signals.OrderBy(signal => signal.dayMod).ToList();

                        if (firstSignal.signal == secondSignal.signal)
                            i++;
                    }
                }
            }

            customIndicator.sortSignals();
            if (SellSetting != null)
                customIndicator.removeRedundantLastSignals();

            return customIndicator;
        }

        #endregion



        #region Risk Management
        //Calculate a risk managed exit for each buy signal
        public void ManageRisk()
        {
            //TODO:Continue here
            foreach (Indicator indc in Indicators)
            {
                for (int i = 0; i < indc.signals.Count; i++)
                {
                    if (indc.signals[i].signal == Utilities.Command.Buy)
                    {
                        Signal entrySignal = indc.signals[i];
                        //Signal exitSignal = RiskManagement.CalculateMinimalLossExit(this, entrySignal.date, entrySignal.price, 100);
                        Signal exitSignal = RiskManagement.CalculateGreedyBullExit(this, entrySignal.date, entrySignal.price, 100);
                        indc.signals.Add(exitSignal);
                    }
                }
                indc.signals = indc.signals.OrderBy(signal => signal.dayMod).ToList();
            }
        }
        #endregion

        #region Crunch Buy/Sell Signals
        public void CalculateBuySellSignals()
        {
            for (int i = 0; i < Indicators.Count; i++)
            {
                Indicators[i].calculateLongRangeProfits();
            }
            /*
            foreach(DataSet BuySellPattern in BuySellSignals)
            {
                DataSet Output = new DataSet("MACrossOvers");
                Output.Tables.Add(new DataTable("BullCross"));
                Output.Tables["BullCross"].Columns.Add("X Values", typeof(Double));
                Output.Tables["BullCross"].Columns.Add("Y Values", typeof(Double));

                Output.Tables.Add(new DataTable("BearCross"));
                Output.Tables["BearCross"].Columns.Add("X Values", typeof(Double));
                Output.Tables["BearCross"].Columns.Add("Y Values", typeof(Double));
                if (BuySellPattern.DataSetName.Contains("MAXover"))
                {
                    if ((BuySellPattern.Tables["BullCross"].Columns.Count != 0) && (BuySellPattern.Tables["BullCross"].Columns.Count != 0))
                    {
                        int j = 0;
                        //while (BuySellPattern.Tables["BearCross"].Rows[j][0] < BuySellPattern.Tables["BullCross"].Rows[0][0])
                            j++;
                    }
                    foreach (DataTable table in BuySellPattern.Tables)
                    {
                        if (table.TableName == "BullCross") ;//Continue here
                    }
                }
            }
            */
        }
        #endregion


        #endregion

        #region Self Analysis
        public bool selfEvaluation(int startPoint, int endPoint)
        {
           
            if (startPoint >= DailyDataCollection.Count)
            {
                Console.WriteLine("Statistical error, " + symbol + " only has " + DailyDataCollection.Count.ToString() + " entries");
                return false;
            }
            
            double startValue = DailyDataCollection[DailyDataCollection.Count - 1 - startPoint].getClose();
            double endValue = DailyDataCollection[DailyDataCollection.Count - 1 - endPoint].getClose();

            //some stocks bounce off 4 digit zero
            if (startValue == 0)
                return false;

            deltaValueOfShares = endValue - startValue;
            deltaPercentageValueOfShares = (endValue - startValue)*100/startValue;
            Console.WriteLine(symbol + " changed in value by " + deltaValueOfShares.ToString());
            //Console.WriteLine(symbol + " changed in %value by " + deltaPercentageValueOfShares.ToString());
            return true;
        }

        #endregion
    }
}
