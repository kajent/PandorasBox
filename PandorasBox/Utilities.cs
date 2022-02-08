using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Drawing;

namespace PandorasBox
{
    class Utilities
    {
        public static double capital = 10000;
        public static double riskTolerance = 1;
        public static int incosistentData = 0;
        public static List<String> missingData = new List<String>();

        #region Interface variables

        public static int filterDuration = 1;
        public static int filterWindow = 1;

        public static TradeSettings BuySettings;
        public static TradeSettings SellSettings;

        public static int INTERVALQ = 10;//Quick
        public static int INTERVALS = 20;//Short
        public static int INTERVALI = 50;//Intermediate
        public static int INTERVALL = 100;//Long

        public static double BollingerSTDRatio = 2.0;

        public static double RSI_UpperBound = 70;
        public static double RSI_LowerBound = 30;
        public static int RSI_period = 14;

        public static double Stochastic_UpperBound = 70;
        public static double Stochastic_LowerBound = 30;
        public static int Stochastic_period = 14;

        public static int ForceIndex_EMA_period = 14;

        public static int numberOfShares = 1;
        public static double deltaValueOfShares = 1;
        public static double deltaPercentageValueOfShares = 1;

        public static int MACD_longEMA_period = 26;
        public static int MACD_shortEMA_period = 12;        
        public static int MACD_signalEMA_period = 9;

        public static int HeikenAshi_period = 3;


        public static int ADX_period = 14;
        public static int ADX_normal_trend_line = 20;
        public static int ADX_strong_trend_line = 25;
        public static int ADX_extreme_trend_line = 50;
        public static int ADX_smoothing_iterations = 1;

        public static bool greedyExits = false;

        public static string exchange = "Invalid";
        


        public struct StockPoint
        {
            public double x;
            public double y;
        }

        private static Color[] StockColors = {Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.DarkCyan, 
                                                 Color.Brown, Color.Magenta, Color.Black, Color.Azure, Color.BlanchedAlmond};

        public enum FibonacciFlag { BounceUp, BounceDown, Near, CrossOver_BounceUp, CrossOver_BounceDown, NotCalculated};
        public enum Trend{Up, Down, Channel, Irrelevant};
        public enum MACD_around_Zero { Above, Below, Irrelevant};
        public enum MACD_around_Signal { Above, Below, Irrelevant };
        public enum MA_around_MA { Above, Below, Irrelevant };
        public enum Stock_around_MA { Above, Below, Irrelevant };

        public enum TrendInterval {Quick, Short, Intermediate, Long};
        public enum Command { Buy, Sell, OverBought, OverSold, Exit };
        public enum IndicatorSignalNames { QSXover, SIXover, ILXover, MACDXover, MACDShort, MACDLong, HeikenAshi, ForcedEntry, Divergeance, Custom, CustomXOver, ATR };
        public enum BandZones { UpperBand, Middle, LowerBand }
        public enum ForceType { Bullish, Bearish }
        public enum RSIType { Bullish, Bearish }
        public enum StochasticType { Bullish, Bearish }
        public enum HeikenAshiType { Bullish, Bearish, Reversal, None };
        public enum TradeType { Buy, Sell };
        public enum DisplayedIndicator { MACD, RSI, Stochastic, ADX, Force, MovAvg, Divergeance };
        public enum ADXTrend {NoTrend, Normal, Strong, Extreme, Irrelevant };
        public enum ADX_PDIvsNDI { PDI_above_NDI, NDI_above_PDI, Irrelevant };
        public enum ADX_Trending { Trending, NotTrending, Irrelevent }

        public enum ExitType {BestProfit, WorstLoss, CutLosses, KeepProfits, Irrelevant}
        public static ExitType SelectedExit = ExitType.Irrelevant;
        public static List<StocksProfit> ProfitsByPercent_Longs;
        public static List<StocksProfit> ProfitsByPercent_Shorts;
        public static List<Series> Portfolio;

        public static FileStream logFile = new FileStream("c:\\test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        public static StreamWriter logWriter = new StreamWriter(logFile);

        public static string QSXover = "QSMAXover";
        public static string SIXover = "SIMAXover";
        public static string ILXover = "ILMAXover";

        public struct StocksProfit
        {
            public double PercentProfit;
            public String Symbol;

            public StocksProfit(double percentProfits, string symbol)
            {
                PercentProfit = percentProfits;
                Symbol = symbol;
            }
        }

        #endregion

        public static void WriteToLogFile(String message)
        {
            logWriter.WriteLine(message);
            logWriter.Flush();
        }

        public static string getIndicatorSignalNameAsString(IndicatorSignalNames indicator)
        {
            String result;
            switch (indicator)
            {
                case IndicatorSignalNames.QSXover:
                    result = "QSMAXover";
                    break;
                case IndicatorSignalNames.SIXover:
                    result = "SIMAXover";
                    break;
                case IndicatorSignalNames.ILXover:
                    result = "ILMAXover";
                    break;
                case IndicatorSignalNames.HeikenAshi:
                    result = "HeikenAshi";
                    break;
                case IndicatorSignalNames.ForcedEntry:
                    result = "ForcedEntry";
                    break;
                case IndicatorSignalNames.Divergeance:
                    result = "Divergeance";
                    break;
                default:
                    result = "none";
                    break;
            }
            return result;
        }

        #region Error Handling
        public static bool IsAlphaSpaced(String TestString)
        {
            if (TestString.Length == 0)
                return false;
            Regex InvalidText = new Regex("[^a-z A-Z]");
            if (InvalidText.IsMatch(TestString))
                return false;
            else
                return true;
        }

        public static bool IsAlphaSpacedNumeric(String TestString)
        {
            if (TestString.Length == 0)
                return false;
            Regex InvalidText = new Regex("[^a-z A-Z0-9]");
            if (InvalidText.IsMatch(TestString))
                return false;
            else
                return true;
        }

        public static bool IsAlpha(String TestString)
        {
            if (TestString.Length == 0)
                return false;
            Regex InvalidText = new Regex("[^a-zA-Z]");
            if (InvalidText.IsMatch(TestString))
                return false;
            else
                return true;
        }



        public static bool IsAlphaNumeric(String TestString)
        {
            if (TestString.Length == 0)
                return false;
            Regex InvalidText = new Regex("[^a-zA-Z0-9]");
            if (InvalidText.IsMatch(TestString))
                return false;
            else
                return true;
        }

        public static bool IsNumeric(String TestString)
        {
            UInt64 dummy;
            return UInt64.TryParse(TestString, out dummy);
        }

        public static bool IsDecimal(String TestString)
        {
            Decimal dummy;
            return Decimal.TryParse(TestString, out dummy);
        }

        public static bool IsPositiveInt(String TestString)
        {
            UInt64 dummy;
            return UInt64.TryParse(TestString, out dummy);
        }

        public static int SelectProperExchange(List<String> Items, String FilePath)
        {
            for(int i = 0; i < Items.Count; i++)
            {
                String SupportedExchange = Items[i];
                if (FilePath.Contains(SupportedExchange))
                    return i;
            }
            return 0;
        }
        #endregion

        #region Get SimpleStockPoints for given stock datasets
        //Get SimpleStockPoints for a given stock dataset
        public static List<SimpleStockPoint> Convert_DataSet_to_SimpleStock(DataSet StockDataSet)
        {
            String symbol = StockDataSet.Tables[0].Rows[0][0].ToString();
            List<SimpleStockPoint> stockDataResult = new List<SimpleStockPoint>();
            for (int i = 0; i < StockDataSet.Tables[0].Rows.Count; i++)
            {
                int date = i;
                //int date = int.Parse(StockDataSet.Tables[0].Rows[i][1].ToString());
                uint volume = uint.Parse(StockDataSet.Tables[0].Rows[i][6].ToString());
                double close = double.Parse(StockDataSet.Tables[0].Rows[i][5].ToString());
                stockDataResult.Add(new SimpleStockPoint(date, volume, close, symbol));
            }

            return stockDataResult;
        }

        //Get List of SimpleStockPoints for a given list of stock datasets
        public static List<List<SimpleStockPoint>> Convert_DataSetList_to_SimpleStockList(List<DataSet> multiStockDataArray)
        {
            List<List<SimpleStockPoint>> multiStockDataSetResult = new List<List<SimpleStockPoint>>();
            try
            {   
                foreach (DataSet stockDataSet in multiStockDataArray)                    
                    multiStockDataSetResult.Add(Convert_DataSet_to_SimpleStock(stockDataSet));
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "Failed in Converting Stock Data Set to SimpleStock Data Set! Possibly No Data within Time Period");
            }
            return multiStockDataSetResult;
        }
        #endregion

        #region Get series for graphing multiple stocks, or prices and candlestick of a single stock
        //Get List of series for graphing prices of multiple SimpleStockPoints
        public static List<Series> Convert_SimpleStocksList_to_SeriesList(List<List<SimpleStockPoint>> multiSimpleStockDataArray)
        {
            List<Series> multiStockSeriesResult = new List<Series>();
            try
            {
                Random random = new Random();
                for (int i = 0; i < multiSimpleStockDataArray.Count; i++)
                {
                    String symbol = multiSimpleStockDataArray[i][0].getSymbol();
                    multiStockSeriesResult.Add(new Series(symbol));
                    multiStockSeriesResult[i].ChartType = SeriesChartType.Line;
                    multiStockSeriesResult[i].Color = StockColors[i % 10];
                    //multiStockSeriesResult[i].Color = Color.FromArgb(random.Next() % 255, random.Next() %255, random.Next() %255);
                    
                    for (int j = 0; j < multiSimpleStockDataArray[i].Count; j++)
                    {
                        SimpleStockPoint dataForADay = multiSimpleStockDataArray[i][j];
                        multiStockSeriesResult[i].Points.AddXY(dataForADay.getDateMod(), dataForADay.getClose());
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "Failed in Converting Simple Stock Data Set to Series Stock Data Set!");
            }
            return multiStockSeriesResult;
        }
                
        //Get Series for graphing CandleStick Data from EnhancedSimpleStockPoints
        public static Series Convert_EnhancedStockData_to_CandleStickSeries(List<EnhancedSimpleStockPoint> stockData)
        {
            Series result = new Series();
            result.ChartType = SeriesChartType.Candlestick;
            for (int i = 0; i < stockData.Count; i++)
            {
                EnhancedSimpleStockPoint dataForADay = stockData[i];
                result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getHigh());
                result.Points[i].YValues[1] = dataForADay.getLow();
                result.Points[i].YValues[3] = dataForADay.getOpen();
                result.Points[i].YValues[2] = dataForADay.getClose();
            }
            return result;
        }

        //Get Series for graphing Price Data from EnhancedSimpleStockPoints
        public static Series Convert_EnhancedStockData_to_PriceSeries(List<EnhancedSimpleStockPoint> stockData, String Name, SeriesChartType chartType)
        {
            Series result = new Series(Name);
            if (chartType == SeriesChartType.Candlestick)
            {   
                result.ChartType = SeriesChartType.Candlestick;
                result.YValuesPerPoint = 4;
                //result.ChartType = SeriesChartType.Line;
                for (int i = 0; i < stockData.Count; i++)
                {
                    EnhancedSimpleStockPoint dataForADay = stockData[i];
                    //result.Points.AddXY(dataForADay.getDate(), dataForADay.getClose());
                    result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getHigh());
                    result.Points[i].YValues[1] = dataForADay.getLow();
                    result.Points[i].YValues[3] = dataForADay.getOpen();
                    result.Points[i].YValues[2] = dataForADay.getClose();
                }
            }
            else if (chartType == SeriesChartType.Line)
            {
                result.ChartType = SeriesChartType.Line;
                for (int i = 0; i < stockData.Count; i++)
                {
                    EnhancedSimpleStockPoint dataForADay = stockData[i];
                    result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getClose());
                }
            }
            return result;
        }

        //Get Series for graphing Price Data from EnhancedSimpleStockPoints
        public static Series Convert_HeikenAshiStockData_to_PriceSeries(List<HeikenAshiStockPoint> stockData, String Name, SeriesChartType chartType)
        {
            Series result = new Series(Name);
            if (chartType == SeriesChartType.Candlestick)
            {
                result.ChartType = SeriesChartType.Candlestick;
                result.YValuesPerPoint = 4;
                //result.ChartType = SeriesChartType.Line;
                for (int i = 0; i < stockData.Count; i++)
                {
                    EnhancedSimpleStockPoint dataForADay = stockData[i];
                    //result.Points.AddXY(dataForADay.getDate(), dataForADay.getClose());
                    result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getHigh());
                    result.Points[i].YValues[1] = dataForADay.getLow();
                    result.Points[i].YValues[3] = dataForADay.getOpen();
                    result.Points[i].YValues[2] = dataForADay.getClose();
                }
            }
            else if (chartType == SeriesChartType.Line)
            {
                result.ChartType = SeriesChartType.Line;
                for (int i = 0; i < stockData.Count; i++)
                {
                    EnhancedSimpleStockPoint dataForADay = stockData[i];
                    result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getClose());
                }
            }
            return result;
        }

        //Get Series for graphing Volume Data from EnhancedSimpleStockPoints
        public static Series Convert_EnhancedStockData_to_VolumeSeries(List<EnhancedSimpleStockPoint> stockData, String Name)
        {
            Series result = new Series(Name);
            result.ChartType = SeriesChartType.Stock;
            for (int i = 0; i < stockData.Count; i++)
            {
                EnhancedSimpleStockPoint dataForADay = stockData[i];
                result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getVolume());
            }
            return result;
        }

        //Get Series for graphing Volume Data from EnhancedSimpleStockPoints
        public static Series Convert_EnhancedStockData_to_VolumeUpSeries(List<EnhancedSimpleStockPoint> stockData, String Name)
        {
            Series result = new Series(Name);
            result.ChartType = SeriesChartType.Stock;
            for (int i = 0; i < stockData.Count; i++)
            {
                EnhancedSimpleStockPoint dataForADay = stockData[i];
                if (dataForADay.getDirection() == 1)
                {
                    result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getVolume());
                }
                else
                {
                    result.Points.AddXY(dataForADay.getDateMod(), 0);
                }
            }
            return result;
        }

        //Get Series for graphing Volume Data from EnhancedSimpleStockPoints
        public static Series Convert_EnhancedStockData_to_VolumeDownSeries(List<EnhancedSimpleStockPoint> stockData, String Name)
        {
            Series result = new Series(Name);
            result.ChartType = SeriesChartType.Stock;
            for (int i = 0; i < stockData.Count; i++)
            {
                EnhancedSimpleStockPoint dataForADay = stockData[i];
                if (dataForADay.getDirection() <= 0)
                {
                    result.Points.AddXY(dataForADay.getDateMod(), dataForADay.getVolume());
                }
                else
                {
                    result.Points.AddXY(dataForADay.getDateMod(), 0);
                }
            }
            return result;
        }

        //Get number of entries above and below
        public static Object GetWinningLosingPercentagesFromSeries(Series series)
        {
            int AboveZero = 0;
            int BelowZero = 0;
            for (int i = 0; i < series.Points.Count; i++)
            {
                if (series.Points[i].YValues[0] > 0)
                    AboveZero++;
                if (series.Points[i].YValues[0] < 0)
                    BelowZero++;
            }
            return new Object[] { AboveZero, BelowZero, series.Points.Count };
        }

        public static List<StockPoint> Convert_EnhancedStockData_to_StockPoints(List<EnhancedSimpleStockPoint> stockData)
        {
            List<StockPoint> StockPoints = new List<StockPoint>();

            for (int i = 0; i < stockData.Count; i++)
            {
                StockPoint newPoint = new StockPoint ();
                newPoint.x = stockData[i].getDateMod();
                newPoint.y = stockData[i].getClose();

                StockPoints.Add(newPoint);
            }

            return StockPoints;
        }

        #endregion

        #region Get EnhancedSimplePoints for a given stock dataset
        //Get List of EnhancedSimplePoints for Graphing from given DataSet
        public static List<EnhancedSimpleStockPoint> Convert_DataSet_to_EnhancedStock_ForGraphing(DataSet StockDataSet)
        {
            String symbol = StockDataSet.Tables[0].Rows[0][0].ToString();
            List<EnhancedSimpleStockPoint> stockDataResult = new List<EnhancedSimpleStockPoint>();
            for (int i = 0; i < StockDataSet.Tables[0].Rows.Count; i++)
            {
                int date = i;
                //int date = int.Parse(StockDataSet.Tables[0].Rows[i][1].ToString());
                uint volume = uint.Parse(StockDataSet.Tables[0].Rows[i][6].ToString());
                double open = double.Parse(StockDataSet.Tables[0].Rows[i][2].ToString());
                double high = double.Parse(StockDataSet.Tables[0].Rows[i][3].ToString());
                double low = double.Parse(StockDataSet.Tables[0].Rows[i][4].ToString());
                double close = double.Parse(StockDataSet.Tables[0].Rows[i][5].ToString());
                stockDataResult.Add(new EnhancedSimpleStockPoint(date, volume, open, high, low, close, symbol));
            }

            return stockDataResult;
        }

        //Get List of EnhancedSimplePoints for Analysis from given DataSet
        public static List<EnhancedSimpleStockPoint> Convert_DataSet_to_EnhancedStock_ForAnalyzing(DataSet StockDataSet)
        {
            String symbol = StockDataSet.Tables[0].Rows[0][0].ToString();
            List<EnhancedSimpleStockPoint> stockDataResult = new List<EnhancedSimpleStockPoint>();
            for (int i = 0; i < StockDataSet.Tables[0].Rows.Count; i++)
            {
                int date = int.Parse(StockDataSet.Tables[0].Rows[i][1].ToString());
                uint volume = uint.Parse(StockDataSet.Tables[0].Rows[i][6].ToString());
                double open = double.Parse(StockDataSet.Tables[0].Rows[i][2].ToString());
                double high = double.Parse(StockDataSet.Tables[0].Rows[i][3].ToString());
                double low = double.Parse(StockDataSet.Tables[0].Rows[i][4].ToString());
                double close = double.Parse(StockDataSet.Tables[0].Rows[i][5].ToString());
                stockDataResult.Add(new EnhancedSimpleStockPoint(date, volume, open, high, low, close, symbol));
            }

            return stockDataResult;
        }
        #endregion

        #region Convert DataSets
        public static Series Convert_DoubleList_to_Series(List<Double> doubleList, int Gap, String Name)
        {
            Series result = new Series(Name);
            result.ChartType = SeriesChartType.Line;
            for (int i = 0; i < doubleList.Count; i++)
            { 
                //0 indexed mod for the gap
                result.Points.AddXY(i + Gap - 1, doubleList[i]);
            }
            return result;
        }

        public static List<Double> Convert_Series_to_DoubleList(Series input)
        {
            List<Double> output = new List<double>();
            for (int i = 0; i < input.Points.Count; i++)
            {
                Double Xvalue = input.Points[i].YValues[0];
                output.Add(Xvalue);
            }
            return output;
        }

        public static Series Convert_StockPoints_to_Series(List<StockPoint> StockData, String Name)
        {
            Series result = new Series(Name);
            result.ChartType = SeriesChartType.Line;
            for (int i = 0; i < StockData.Count; i++)
            {
                result.Points.AddXY(StockData[i].x,StockData[i].y);
            }
            return result;
        }
        
        public static List<Series> Convert_DoubleListArray_to_SeriesList(List<List<Double>> doubleListArray, List<int> Gaps, List<String> Names)
        {
            List<Series> result = new List<Series>();
            for (int i = 0; i < doubleListArray.Count; i++)
            {
                Series intermediateResult = new Series();
                intermediateResult = Convert_DoubleList_to_Series(doubleListArray[i], Gaps[i], Names[i]);
                result.Add(intermediateResult);
            }
            return result;
        }

        public static Series Convert_DoublePairList_to_Series(List<List<Double>> DPList, String Name)
        {
            Series result = new Series(Name);
            result.ChartType = SeriesChartType.Point;
            for (int i = 0; i < DPList.Count; i++)
            {
                result.Points.AddXY(i, DPList[i]);
            }
            return result;
        }

        public static Series Convert_DataSet_to_Series(DataSet DataSet, String TableName, String Name, 
            SeriesChartType ChartType, MarkerStyle Style, Color MarkerColor, Color LineColor)
        {
            if (DataSet == null)
                return null;

            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = LineColor;
            Series.MarkerStyle = Style;
            Series.MarkerColor = LineColor;
            Series.MarkerSize = 10;

            for (int i = 0; i < DataSet.Tables[TableName].Rows.Count; i++)
            {
                Double X = Double.Parse(DataSet.Tables[TableName].Rows[i][0].ToString());
                Double Y = Double.Parse(DataSet.Tables[TableName].Rows[i][1].ToString());
                Series.Points.Add(new DataPoint(X,Y));
            }
            return Series;
        }

        public static Series Convert_DataSet_Evaluations_to_Series(DataSet DataSet, String TableName, String Name,
           SeriesChartType ChartType, MarkerStyle Style, Color MarkerColor, Color LineColor)
        {
            if (DataSet == null)
                return null;

            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = LineColor;
            Series.MarkerStyle = Style;
            Series.MarkerColor = LineColor;
            Series.MarkerSize = 10;

            for (int i = 0; i < DataSet.Tables[TableName].Rows.Count; i++)
            {
                Double X = Double.Parse(DataSet.Tables[TableName].Rows[i][1].ToString());
                Double Y = Double.Parse(DataSet.Tables[TableName].Rows[i][2].ToString());
                Series.Points.Add(new DataPoint(X, Y));
            }
            return Series;
        }

        public static Series Convert_YValue_to_SeriesLine(Double YValue, String Name, SeriesChartType ChartType, 
            Color Color, Double MinX, Double MaxX)
        {
            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = Color;            
            Series.Points.Add(new DataPoint(MinX, YValue));
            Series.Points.Add(new DataPoint(MaxX, YValue));
            Series.IsVisibleInLegend = false;
            return Series;
        }

        public static Series Convert_StockPoints_to_Series(List<Utilities.StockPoint> StockPoints, String Name, SeriesChartType ChartType, Color Color)
        {
            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = Color;            
            for (int i = 0; i < StockPoints.Count; i++)
            {
                Double X = StockPoints[i].x;
                Double Y = StockPoints[i].y;
                Series.Points.Add(new DataPoint(X, Y));
            }
            Series.IsVisibleInLegend = false;
            return Series;
        }

        public static List<String> Convert_DataSet_to_StringList(DataSet DataSet, int ValueColumn)
        {
            List<String> output = new List<String>();
            for(int i = 0; i < DataSet.Tables[0].Rows.Count; i++)
                output.Add(DataSet.Tables[0].Rows[i][ValueColumn].ToString());
            return output;            
        }

        public static List<Int32> Convert_DataSet_to_IDList(DataSet DataSet, int ValueColumn)
        {
            List<Int32> output = new List<Int32>();
            for (int i = 0; i < DataSet.Tables[0].Rows.Count; i++)
                output.Add(Convert.ToInt32(DataSet.Tables[0].Rows[i][ValueColumn].ToString()));
            return output;
        }
        #endregion

        #region Convert Indicators
        public static Series Convert_Indicator_To_Series(Indicator indicator, String Name, SeriesChartType ChartType, MarkerStyle Style, Color LineColor)
        {
            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = LineColor;
            Series.MarkerStyle = Style;            
            Series.MarkerSize = 10;
            foreach (Signal signal in indicator.signals)
            {                
                Double X = signal.dayMod;
                Double Y = signal.price;
                DataPoint newPoint = new DataPoint(X,Y);
                newPoint.MarkerColor = Color.Red;
                newPoint.MarkerStyle = MarkerStyle.Cross;

                if (signal.signal == Utilities.Command.Buy)
                    newPoint.MarkerColor = Color.Gold;
                else if (signal.signal == Utilities.Command.Sell)
                    newPoint.MarkerColor = Color.Black;
                Series.Points.Add(newPoint);
            }
            //TODO:Continue here!
            
            return Series;
        }

        public static Series Convert_Shorts_To_Profit_Series(Indicator indicator, String Name, SeriesChartType ChartType, MarkerStyle Style, Color LineColor)
        {
            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = LineColor;
            Series.MarkerStyle = Style;
            Series.MarkerSize = 10;

            
            foreach (ShortTrade Short in indicator.shorts)
            {
                bool negative = false;
                Double X = Short.dayMod;
                Double Y = Short.getProfit();
                if (Y < 0)
                {
                    Y = Y * (-1);
                    negative = true;
                }

                DataPoint newPoint = new DataPoint(X, Y);
                
                if (negative)
                    newPoint.Color = Color.Red;
                Series.Points.Add(newPoint);
            }
            return Series;
        }

        public static Series Convert_Longs_To_Profit_Series(Indicator indicator, String Name, SeriesChartType ChartType, MarkerStyle Style, Color LineColor)
        {
            Series Series = new Series(Name);
            Series.ChartType = ChartType;
            Series.Color = LineColor;
            Series.MarkerStyle = Style;
            Series.MarkerSize = 10;

            
            foreach (LongTrade Long in indicator.longs)
            {
                bool negative = false;
                Double X = Long.dayMod;
                Double Y = Long.getProfit();
                if (Y < 0)
                {
                    Y = Y * (-1);
                    negative = true;
                }

                DataPoint newPoint = new DataPoint(X, Y);
                if (negative)
                    newPoint.Color = Color.Black;
                Series.Points.Add(newPoint);
            }
            return Series;
        }     
        #endregion

       

        #region Conver Magnitude of Simple Stock Points to Percentages
        public static List<List<SimpleStockPoint>> Convert_SimpleMagnitudeLists_to_SimplePercentLists(List<List<SimpleStockPoint>> StockList)
        {
            List<List<SimpleStockPoint>> Result = new List<List<SimpleStockPoint>>();
            for (int i = 0; i < StockList.Count; i++)
                Result.Add(Convert_SimpleMagnitude_to_SimplePercent(StockList[i]));
            return Result;
        }

        public static List<SimpleStockPoint> Convert_SimpleMagnitude_to_SimplePercent(List<SimpleStockPoint> Stock)
        {            
            double startValue = Stock[0].getClose();
            for(int i = 0; i < Stock.Count; i++)
                Stock[i].setClose((Stock[i].getClose()- startValue)*100/startValue);
            return Stock;
        }
        #endregion

        #region Stock Max/Min Values
        public static double getStockMaxValue(List<EnhancedSimpleStockPoint> stockData)
        {
            double ymax = 0;
            for (int i = 0; i < stockData.Count; i++)
                if (ymax < stockData[i].getHigh())
                    ymax = stockData[i].getHigh();
            return ymax;
        }

        public static double getStockMinValue(List<EnhancedSimpleStockPoint> stockData)
        {
            double ymin = double.MaxValue;
            for (int i = 0; i < stockData.Count; i++)
                if (ymin > stockData[i].getLow())
                    ymin = stockData[i].getLow();
            return ymin;
        }

        public static double getStockMaxValue(List<EnhancedSimpleStockPoint> stockData, int limit)
        {
            double ymax = 0;
            for (int i = stockData.Count - limit; i < stockData.Count; i++)
                if (ymax < stockData[i].getHigh())
                    ymax = stockData[i].getHigh();
            return ymax;
        }

        public static double getStockMinValue(List<EnhancedSimpleStockPoint> stockData, int limit)
        {
            double ymin = double.MaxValue;
            for (int i = stockData.Count - limit; i < stockData.Count; i++)
                if (ymin > stockData[i].getLow())
                    ymin = stockData[i].getLow();
            return ymin;
        }

        public static double getStockMinValue_ForAnalysis(List<EnhancedSimpleStockPoint> stockData)
        {
            double ymin = double.MaxValue;
            for (int i = 0; i < stockData.Count; i++)
                if (ymin > stockData[i].getLow())
                    ymin = stockData[i].getLow();
            return ymin;
        }

        public static double getStockMinValue(List<SimpleStockPoint> stockData)
        {
            double ymin = double.MaxValue;
            for (int i = 0; i < stockData.Count; i++)
                if (ymin > stockData[i].getClose())
                    ymin = stockData[i].getClose();
            return ymin;
        }

        public static double getStockMinValue(List<List<SimpleStockPoint>> stockDataSet)
        {
            double ymin = double.MaxValue;
            for (int i = 0; i < stockDataSet.Count; i++)
            {
                double ymin2 = getStockMinValue(stockDataSet[i]);
                if (ymin > ymin2)
                    ymin = ymin2;
            }
            return ymin;
        }

        public static double getLocalStockMinValue(DataPointCollection localStockData, int start, int limit)
        {
            double ymin = double.MaxValue;
            if (localStockData[0].YValues.Length == 4)
            {
                for (int i = start; i < start + limit; i++)
                {
                    double ymin2 = Convert.ToDouble(localStockData[i].YValues[1]);
                    if (ymin > ymin2)
                        ymin = ymin2;

                    if (i == localStockData.Count - 1)
                        break;
                }
            }
            else
            {
                for (int i = start; i < start + limit; i++)
                {
                    double ymin2 = Convert.ToDouble(localStockData[i].YValues[0]);
                    if (ymin > ymin2)
                        ymin = ymin2;
                    if (i == localStockData.Count - 1)
                        break;
                }
            }
            return ymin;
        }

        public static double getLocalStockMaxValue(DataPointCollection localStockData, int start, int limit)
        {
            double ymax = 0;
            if (localStockData[0].YValues.Length == 4)
            {
                for (int i = start; i < start + limit; i++)
                {
                    double ymax2 = Convert.ToDouble(localStockData[i].YValues[0]);
                    if (ymax < ymax2)
                        ymax = ymax2;
                    if (i == localStockData.Count - 1)
                        break;
                }
            }
            else
            {
                for (int i = start; i < start + limit; i++)
                {
                    double ymax2 = Convert.ToDouble(localStockData[i].YValues[0]);
                    if (ymax < ymax2)
                        ymax = ymax2;
                    if (i == localStockData.Count - 1)
                        break;
                }
            }
            return ymax;
        }


        #endregion

        #region Get Exponential Moving Average
        public static List<Double> Convert_DoubleList_to_EMA_List(List<Double> input, int period)
        {
            if (input.Count < period)
                return new List<double>();
            List<Double> result_EMA = new List<double>(input.Count);

            double runningAvg = 0;
            double esf = (Double)2 / (period + 1);
            for (int i = 0; i < period; i++)
            {
                runningAvg += input[i];
            }

            runningAvg = runningAvg / period;
            //Initialize EMA with SMA
            result_EMA.Add(runningAvg);

            for (int i = (int)period; i < input.Count; i++)
            {
                double EMA_Today = (input[i] * esf) +
                    (result_EMA[result_EMA.Count - 1] * (1 - esf));
                result_EMA.Add(EMA_Today);
            }

            return result_EMA;
        }

        public static List<Double> Convert_DoubleList_to_SMA_List(List<Double> input, int period)
        {
            if (input.Count < period)
                return new List<double>();
            List<Double> result_SMA = new List<double>(input.Count - period + 1);

            for (int i = period; i < input.Count; i++)
            {
                double sma_entry = 0;
                for (int j = 0; j < period; j++)
                {
                    sma_entry += input[i - j];
                }
                sma_entry /= period;
                
                result_SMA.Add(sma_entry);
            }

            return result_SMA;
        }

        #endregion

        #region Get Wilder Smoothing
        public static List<Double> Convert_DoubleList_to_WilderSmoothedTR_List(List<Double> input, int period)
        {
            List<Double> TR = new List<double>(input.Count);

            double TR1 = 0;
            
            for (int i = 0; i < period; i++)
                TR1 += input[i];
            TR1 = TR1 / period;
         
            //Initialize TR with TR1
            TR.Add(TR1);

            for (int i = period; i < input.Count; i++)
            {
                double TR_Previous = TR[TR.Count - 1];
                double TR_Current = (TR_Previous * (Double)(period - 1) + input[i]) / period;
                TR.Add(TR_Current);
            }

            return TR;
        }
           
        public static List<Double> Convert_DoubleList_to_X14(List<Double> input, int period)
        {
            List<Double> X14 = new List<double>(input.Count);

            double X1 = 0;

            for (int i = 0; i < period; i++)
                X1 += input[i];
            X1 = X1;

            //Initialize TR with TR1
            X14.Add(X1);

            for (int i = period; i < input.Count; i++)
            {
                double X1_Previous = X14[X14.Count - 1];
                double X1_Current = (X1_Previous - X1_Previous/period + input[i]);
                X14.Add(X1_Current);
            }

            return X14;
        }

        public static List<Double> Convert_DoubleList_to_XI14(List<Double> TR14, List<Double> DM14)
        {
            List<Double> XI14 = new List<double>(TR14.Count);

            for (int i = 0; i < DM14.Count; i++)
                XI14.Add(100 * DM14[i] / TR14[i]);

            return XI14;
        }

        public static List<Double> Convert_DoubleList_to_WilderSmoothedADX_List(List<Double> DX, int period)
        {
            List<Double> ADX = new List<double>(DX.Count);

            double DX1 = 0;

            for (int i = 0; i < period; i++)
                DX1 += DX[i];
            DX1 = DX1 / period;

            //Initialize ADX with DX1
            ADX.Add(DX1);

            for (int i = period; i < DX.Count; i++)
            {
                double DX_Previous = ADX[ADX.Count - 1];
                double DX_Current = (DX_Previous * (period-1) + DX[i])/period;
                ADX.Add(DX_Current);
            }

            return ADX;
        }


        #endregion

    
        public static Utilities.Trend GetTrendDirection_of_DoubleList(List<Double> input, int range, int index)
        {
            Utilities.Trend Trend = Trend.Irrelevant;

            if (range == 0 || index > input.Count - 1)
                return Trend;

            Double CurrentValue = (input[index]);
            Double Average = 0;

            for (int i = index - range + 1; i <= index; i++)
                Average += input[i];

            Average /= range;

            if (Average < CurrentValue)
                Trend = Trend.Up;
            if (Average > CurrentValue)
                Trend = Trend.Down;
            if (Average == CurrentValue)
                Trend = Trend.Channel;
            return Trend;
        }

        public static Double GetTrendStrength_of_DoubleList(List<Double> input, int range, int index)
        {
            Double Strength = 0;
            if (range == 0 || index >= input.Count)
                return Strength;

            Double CurrentValue = (input[index]);
            Double Average = 0;

            for (int i = index - range + 1; i <= index; i++)
                Average += input[i];

            Average /= range;

            Strength = (CurrentValue - Average) * 100 / CurrentValue;
            return Strength;
        }

        public static Series GetTrends(Series inputSeries, int range, int ignoreZone, String name)
        {
            Series peaks = new Series(name);
            double localMaxY = double.MinValue;
            double localMaxX = double.MinValue;
            double lastX = inputSeries.Points[0].XValue;
            for (int i = 0; i < inputSeries.Points.Count; i++)
            {
                double yValue = inputSeries.Points[i].YValues[0];
                double xValue = inputSeries.Points[i].XValue;

                if (yValue >= localMaxY /* && Math.Abs(lastX - xValue) > ignoreZone*/)
                {
                    localMaxX = xValue;
                    lastX = xValue;
                    localMaxY = yValue;
                }

                if (i % range == 0 && localMaxX >= 0)
                {
                    peaks.Points.Add(new DataPoint(localMaxX, localMaxY));
                    localMaxY = double.MinValue;
                }
            }
            return peaks;
        }

        public static int getDayOfWeekDifference(DayOfWeek dow1, DayOfWeek dow2)
        {
            int count = 0;

            while (dow1 != dow2)
            {
                dow2--;
                count++;
            }
            return count;
        }

    }


}
