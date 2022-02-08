using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows.Forms.DataVisualization.Charting;

namespace PandorasBox
{
    public partial class SingleStockAnalyzer : Form
    {
        public string exchange = null;
        private int timePeriod = 10;
        private int timeOffset = 0;
        private int filterDelay = 500;
        private SeriesChartType selectedChartType = SeriesChartType.Line;
        private StockCollection selectedStocks;
        private int delay = 10;

        private int Graph_WindowXMin = 0;
        private double Graph_WindowYMin = 0;
        private double Graph_WindowYMax = 0;
        private int Graph_WindowSize = 100;
        private int singleStartDate = 0;
        private int singleEndDate = 99999999;
        private Signal buySignal;
        private Signal sellSignal;
        
        private Graph AnalysisGraph;
        private Graph StatisticsGraph;
        private Chart MarketGraph;

        static readonly object masterlock = new object();

        #region Constructors
        public SingleStockAnalyzer()
        {
            InitializeComponent();            
        }

        public SingleStockAnalyzer(String targetExchange)
        {
            AnalysisGraph = new Graph();
            AnalysisGraph.Show();

            MarketGraph = AnalysisGraph.getGraph();
            InitializeComponent();
            exchange = targetExchange;
            InitializeComboBoxes();
            Utilities.exchange = targetExchange;
            Utilities.ProfitsByPercent_Longs = new List<Utilities.StocksProfit>();
            Utilities.ProfitsByPercent_Shorts = new List<Utilities.StocksProfit>();

            Utilities.Portfolio = new List<Series>();

            /*
            DataSet testDataset = new DataSet("Test DataSet");
            testDataset.Tables.Add(new DataTable());
            double[] xValues = {1,2,3,4,5};
            double[] yValues = {10,20,30,40,50};
            testDataset.Tables[0].Columns.Add("X values", typeof(Double));
            testDataset.Tables[0].Columns.Add("Y values", typeof(Double));
            testDataset.Tables[0].Rows.Add(xValues[0], yValues[0]);
            testDataset.Tables[0].Rows.Add(xValues[1], yValues[1]);
            //testDataset.Tables[0].Rows.Add(xValues[2], yValues[2]);
            testDataset.Tables[0].Rows.Add(xValues[3], yValues[3]);
            testDataset.Tables[0].Rows.Add(xValues[4], yValues[4]);

            MarketGraph.Series.Add(new Series("testSeries"));
            MarketGraph.Series[0].ChartType = SeriesChartType.Point;
            MarketGraph.Series[0].MarkerStyle = MarkerStyle.Cross;
            MarketGraph.Series[0].MarkerSize = 10;

            MarketGraph.DataSource = testDataset;
            MarketGraph.Series[0].XValueMember = testDataset.Tables[0].Columns[0].ToString();
            MarketGraph.Series[0].YValueMembers = testDataset.Tables[0].Columns[1].ToString();
            MarketGraph.DataBind();
            */
        }
        #endregion

        #region Accessors
        public void setClipRequest(int newClipRequest)
        {
            if (Graph_WindowXMin <= newClipRequest)
                Graph_WindowXMin = newClipRequest;
        }

        #endregion

        #region Initialization
        private void InitializeComboBoxes()
        {
            cmbBox_MACDaroundZero.DataSource = Enum.GetValues(typeof(Utilities.MACD_around_Zero));
            cmbBox_TrendDirection.DataSource = Enum.GetValues(typeof(Utilities.Trend));
            cmbBox_Interval.DataSource = Enum.GetValues(typeof(Utilities.TrendInterval));
            cmbBox_MACross_QtoS.DataSource = Enum.GetValues(typeof(Utilities.MA_around_MA));
            cmbBox_MACross_StoI.DataSource = Enum.GetValues(typeof(Utilities.MA_around_MA));
            cmbBox_MACross_ItoL.DataSource = Enum.GetValues(typeof(Utilities.MA_around_MA));
            cmbBox_TargetBand.DataSource = Enum.GetValues(typeof(Utilities.BandZones));
            cmbBox_HeikenAshi.DataSource = Enum.GetValues(typeof(Utilities.HeikenAshiType));
            cmbBox_Force.DataSource = Enum.GetValues(typeof(Utilities.ForceType));
            cmbBox_RSI.DataSource = Enum.GetValues(typeof(Utilities.RSIType));
            cmbBox_Stochastic.DataSource = Enum.GetValues(typeof(Utilities.StochasticType));
            cmbBox_MACDaroundZero.DataSource = Enum.GetValues(typeof(Utilities.MACD_around_Zero));
            cmbBox_MACDaroundSignal.DataSource = Enum.GetValues(typeof(Utilities.MACD_around_Signal));
            cmbBox_DisplayedIndicator.DataSource = Enum.GetValues(typeof(Utilities.DisplayedIndicator));
            cmbBox_ADXStrength.DataSource = Enum.GetValues(typeof(Utilities.ADXTrend));
            cmbBox_PDI_vs_NDI.DataSource = Enum.GetValues(typeof(Utilities.ADX_PDIvsNDI));
            cmbBox_ADX_Trending.DataSource = Enum.GetValues(typeof(Utilities.ADX_Trending));
            cmbBox_ExitType.DataSource = Enum.GetValues(typeof(Utilities.ExitType));
            cmbBox_Stock_to_MA.DataSource = Enum.GetValues(typeof(Utilities.Stock_around_MA));


            TTip_Doji_DragonFly.SetToolTip(chkBox_Doji_DragonFly, "At the top of the market, it becomes a variation of the Hanging Man. \n At the bottom of a trend, it becomes a specific Hammer. An extensively long shadow on a \n Dragonfly Doji at the bottom of a trend is very bullish.");
            TTip_Doji_GraveStone.SetToolTip(chkBox_Doji_GraveStone, "A Gravestone Doji, at the top of the trend, is a specific version \n of the Shooting Star. At the bottom, it is a variation of the Inverted Hammer.");
            TTip_Doji_Star.SetToolTip(btn_FilterByDoji, "Upon seeing a Doji in an overbought or oversold condition, an extremely high probability reversal situation \n becomes evident. Overbought or oversold conditions can be defined using other indicators \n such as stochastics, When a Doji appears, it is demonstrating that there is indecision now \n occurring at an extreme portion of a trend. This indecision can be portrayed \n in a few variations of the Doji.");
            
            
            
            MarketGraph.MouseClick += new MouseEventHandler(MarketGraph_MouseClick);
            MarketGraph.MouseWheel += new MouseEventHandler(MarketGraph_Scroll);

            //DataSet savedData = SecureDBOperations.getInstance().loadFavoriteStockSets();
            //lBox_FavoriteStocks.Invoke(new dlgt_UpdateListBox(this.updateSavedFavorites), savedData);

            loadEvaluations();
        }

        void MarketGraph_MouseClick(object sender, MouseEventArgs e)
        {
            if (chkBox_Scroll.Checked)
            {
                //MessageBox.Show(e.Button.ToString());
                //throw new NotImplementedException();
                int localStart = (int)MarketGraph.ChartAreas[0].AxisX.ScaleView.Position;
                DataPointCollection Points = MarketGraph.Series[0].Points;
                Graph_WindowYMin = Utilities.getLocalStockMinValue(Points, localStart, Graph_WindowSize);
                Graph_WindowYMax = Utilities.getLocalStockMaxValue(Points, localStart, Graph_WindowSize);
                MarketGraph.ChartAreas[0].AxisY.Minimum = Graph_WindowYMin;
                MarketGraph.ChartAreas[0].AxisY.Maximum = Graph_WindowYMax;
            }
        }

        void MarketGraph_Scroll(object sender, MouseEventArgs e)
        {            
            //if (Graph_WindowSize > 10)
            //    Graph_WindowSize--;
          
            MessageBox.Show("Need to figure out this scroll thing");
            //MarketGraph.ChartAreas[i].AxisX.ScaleView
            //throw new NotImplementedException();
        }

        #endregion

        #region Delegates
        public delegate void dlgt_UpdateGridDataView(DataSet dataSet);

        public delegate void dlgt_UpdateGraph(Object data);

        public delegate void dlgt_FormEnabled(bool Value);

        public delegate void dlgt_Filter(Object Args);

        #endregion

        #region Invoked by External Thread

        private void invk_MainFormEnabled(bool Value)
        {
            this.Enabled = Value;
        }

        private void invk_ExchangeListUpdate(DataSet newDataSet)
        {
            if (newDataSet != null)
            {
                dGrid_ExchangeData.DataSource = null;
                dGrid_ExchangeData.DataSource = newDataSet.Tables[0];
                dGrid_ExchangeData.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            else 
            {
                dGrid_ExchangeData.DataSource = null;
                MessageBox.Show("Null DataSet Recieved");
            }
        }

        private void invk_MarketGraphUpdate(Object Data)
        {
            switch (selectedChartType)
            {
                    //TODO: This case is no longer needed
                case SeriesChartType.Candlestick:
                    {
                        DataSet fullData = Data as DataSet;
                        DataSet croppedData = fullData;// ImplementOffSet(fullData);
                        buildCandleStickChart(croppedData, fullData);

                        #region Analysis
                        List<Object[]> Dummy = new List<object[]>();
                        FinancialCalculations_MovingAverages("Price", Dummy);
                        #endregion
                    }
                    break;
                case SeriesChartType.Line:
                    {
                        List<DataSet> fullDataSet = Data as List<DataSet>;

                        if (fullDataSet.Count > 1)
                            buildMultiStockPriceChart(fullDataSet);
                        else
                        {
                            DataSet fullData = fullDataSet[0];
                            DataSet croppedData = fullData;// ImplementOffSet(fullData);
                            buildSingleStockPriceChart(croppedData, fullData);

                            #region Analysis
                            List<Object[]> Dummy = new List<object[]>();
                            FinancialCalculations_MovingAverages("Price", Dummy);
                            #endregion
                        }
                    }
                    break;
                default:
                    MessageBox.Show("No Chart Type Selected!");
                    break;
            }
        }

        private void invk_AnalyzedMarketGraphupdate(Object Data)
        {
            Stock targetStock = Data as Stock;
            buildAnalyzedStockChart(targetStock);         
        }

        private void invk_MarketGraphDataAddition(Object Data)
        {
            MarketGraph.Series.Clear();
            Series newSeriesData = (Series)(Data);
            MarketGraph.Series.Add(newSeriesData);
            MarketGraph.Series[MarketGraph.Series.Count - 1].ChartArea = "Price";

        }
        
        private void invk_StatGraphUpdate(Object Data)
        {
            StatisticsGraph = new Graph();
            StatisticsGraph.Show();
            
            
            Chart StatGraph = StatisticsGraph.getGraph();
            StatGraph.Series.Clear();
            //StatGraph.ChartAreas.Add(new ChartArea());

            //StatGraph.ChartAreas[1].Name = "f";

            // StatGraph.ChartAreas[0].Position.X = 50;
            // StatGraph.ChartAreas[0].Position.Y = 50;
            //StatGraph.ChartAreas[1].Position.Height = 80;
            //StatGraph.ChartAreas[1].Position.Width = 80;


            Legend testLegend = new Legend();
            StatGraph.Legends.Add(testLegend);


            
            List<Series> newSeriesCollection = new List<Series>();
            newSeriesCollection = (List<Series>)(Data);
            for (int i = 0; i < newSeriesCollection.Count; i++)
            {
                Series S1 = new Series(newSeriesCollection[i].Name);
                S1.ChartType = SeriesChartType.Line;
                for (int j = 0; j < newSeriesCollection[i].Points.Count; j++)
                    S1.Points.Add(newSeriesCollection[i].Points[j]);
 
                StatGraph.Series.Add(S1);
            }

            //StatGraph.Height = 1000;fd
            

            if (chkBox_Scroll.Checked)
            {
                StatGraph.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                StatGraph.ChartAreas[0].AxisX.ScaleView.Size = 20;
            }

            for (int i = 0; i < StatGraph.Series.Count; i++)
            {
                Object results = Utilities.GetWinningLosingPercentagesFromSeries(StatGraph.Series[i]);
                int Above = (Int32)((results as Object[])[0]);
                int Below = (Int32)((results as Object[])[1]);
                int Total = (Int32)((results as Object[])[2]);

                Title newTitle = new Title();
                newTitle.Text = StatGraph.Series[i].Name.ToString() + " ; % above is  " + (100 * Above / Total).ToString() + " ; % below is " + (100 * Below / Total).ToString() + ";";

                StatGraph.Titles.Add(newTitle);
            }

            /*
            Series newSeriesData = (Series)(Data);
            StatGraph.Series.Add(newSeriesData);
             */ 
        }
        #endregion

        #region Threads Start Here
        //Run as separate thread
        public void parallelExchangeExtraction(object transferData)
        {
            DataSet newData = SecureDBOperations.getInstance().batchExchangeExtract(transferData);
            dGrid_ExchangeData.Invoke(new dlgt_UpdateGridDataView(this.invk_ExchangeListUpdate),newData);
        }

        //TODO:complete this
        public void parallelPortfolioEvaluation()
        {
            int start = (Int32)numBox_EvalStart.Value;
            int end = (Int32)numBox_EvalEnd.Value;

            selectedStocks.evaluatePorfolio(start, end);            
        }

        public void parallelAutoPortfolioEvaluation(object parameters)
        {
            
            Monitor.Enter(masterlock);
            {
                int start = (int)(((Object[])parameters)[0]);
                int end = (int)(((Object[])parameters)[1]);

                selectedStocks.evaluatePorfolio(start, end);
            }
            Monitor.Exit(masterlock);
        }

        public void parallelSingleStockExtraction(object parameters)
        {
            DataSet newData = SecureDBOperations.getInstance().singleStockDataExtract(parameters);            
            dGrid_ExchangeData.Invoke(new dlgt_UpdateGraph(this.invk_MarketGraphUpdate), newData);
        }

        public void parallelMultiStockExtraction(object parameters)
        {
            List<DataSet> newData = SecureDBOperations.getInstance().multiStockDataExtract(parameters);
            dGrid_ExchangeData.Invoke(new dlgt_UpdateGraph(this.invk_MarketGraphUpdate), newData);
        }

        public void parallelMultiStockAnalysis(object parameters)
        {
            Monitor.Enter(masterlock);
            {
                Utilities.ProfitsByPercent_Longs = new List<Utilities.StocksProfit>();
                Utilities.ProfitsByPercent_Shorts = new List<Utilities.StocksProfit>();

                Invoke(new dlgt_FormEnabled(this.invk_MainFormEnabled), false);
                List<DataSet> fullData = SecureDBOperations.getInstance().multiStockDataExtract(parameters);
                List<String> validDates = Utilities.Convert_DataSet_to_StringList(SecureDBOperations.getInstance().singleStockDates(parameters),0);
                List<DataSet> croppedData = fullData;//  new List<DataSet>();
                //This causes cropped data to be used instead of all data in db
                //foreach (DataSet stockData in fullData)
                //    croppedData.Add(ImplementOffSet(stockData));
                MultiStockAnalysis(croppedData, validDates);
                Invoke(new dlgt_FormEnabled(this.invk_MainFormEnabled), true);
            }
            Monitor.Exit(masterlock);
            //Monitor.Pulse(masterlock);
            
        }

        public void parallelSuperAnalysis()
        {
            Auto_Filter();
            Auto_SelectAll();
            Auto_Analyze(300, 0);
            //Auto_FilterByCrossOver_Click(Utilities.TrendInterval.Intermediate, Utilities.TrendInterval.Long);
            //Auto_FilterByCrossOver_Click(Utilities.TrendInterval.Short, Utilities.TrendInterval.Intermediate);
            //Auto_FilterByCrossOver_Click(Utilities.TrendInterval.Quick, Utilities.TrendInterval.Short);
            Auto_FilterByMovingAverage(Utilities.Trend.Up, Utilities.TrendInterval.Long, 1, 30);
            Auto_FilterByMovingAverage(Utilities.Trend.Up, Utilities.TrendInterval.Intermediate, 1, 30);
            Auto_FilterByMovingAverage(Utilities.Trend.Up, Utilities.TrendInterval.Short, 0, 30);
            Auto_FilterByMovingAverage(Utilities.Trend.Up, Utilities.TrendInterval.Quick, 0, 30);


            /*
            for (int i = 0; i < 5; i++)
            {
                Auto_Filter();
                Auto_SelectAll();
                Auto_Analyze(fibstart - i, offset - i);
                Auto_Fibonacci();
                Auto_EvaluateProjection(fibstart -i, offset - i, 3);            
            }
            */
        }

        //TODO:Complete this -- MISING DATA!?!?!?
        public void parallelFutureProjection()
        {
            /* TEMPORARILY DISABLED
            int startPoint = timePeriod - timeOffset -1;
            List<EnhancedSimpleStockPoint> dailyData = selectedStocks.getStockList()[0].getDailyData();
            int forcedEntrydate = dailyData[startPoint].getDate();
            List<Double> profits = new List<double>();

            Auto_SelectAll();
            Auto_Analyze(timePeriod, 0);

            
            Thread.Sleep(1000);
            lock (masterlock)
            {
                List<Stock> stocks = selectedStocks.getStockList();
                for (int i = stocks.Count - 1; i >= 0; i--)
                {
                    double netResult = stocks[i].ForcedStart(forcedEntrydate, startPoint);
                    profits.Add(netResult);
                    Series force = stocks[i].GrabSpecificIndicator(Utilities.IndicatorSignalNames.ForcedEntry);
                }
            }            
            */


            /*
            foreach (Stock stock in selectedStocks.getStockList())
            {
                //Something wrong here
                double netResult = stock.ForcedStart(forcedEntrydate, startPoint);
                profits.Add(netResult);
                Series force = stock.GrabSpecificIndicator(Utilities.IndicatorSignalNames.ForcedEntry);
                //Invoke(new dlgt_UpdateGraph(this.invk_MarketGraphDataAddition), force );
            }
            
            
            profits.Sort();*/

            /*
            List<Utilities.StocksProfit> profits = Utilities.ProfitsByPercent_Longs;
            List<Utilities.StocksProfit> profits2 = new List<Utilities.StocksProfit>();
            List<Double> SignalsToDisplay = new List<double>();
            var lengths = from element in profits
                          orderby element.PercentProfit
                          select element;

            foreach( Utilities.StocksProfit SingleStockProfit in lengths)
            {
                profits2.Add(SingleStockProfit);
                if (Math.Abs(SingleStockProfit.PercentProfit) < 200)
                    SignalsToDisplay.Add(SingleStockProfit.PercentProfit);
                else
                    Console.WriteLine(SingleStockProfit.Symbol + " was out of bounds");
            }
            String PortfolioName = txt_PortfolioName.Text;
            Series mySeries = Utilities.Convert_DoubleList_to_Series(SignalsToDisplay, 0, PortfolioName);
            */
            Invoke(new dlgt_UpdateGraph(this.invk_StatGraphUpdate), Utilities.Portfolio);
            Console.WriteLine(Utilities.incosistentData.ToString() + " stocks are inconsistent");
            
        }

        public void parallelReactiveAnalysis(Object args)
        {
            int period = (int)(args as Object[])[0];
            int offset = (int)(args as Object[])[1];

            if (dGrid_ExchangeData.SelectedRows.Count == 0)
                Auto_SelectAll();
            Auto_Analyze(period, offset);
        }


        #endregion

        #region Data Processing Buttons

        #region FilterExchange
        private void btn_FilterExchange_Click(object sender, EventArgs e)
        {
            List<Object> transferData = new List<Object>();
            transferData.Add(exchange);
            
            if (chkBox_VolumeFilter.Checked)
                if (UInt64.Parse(numBox_MaxVolume.Value.ToString()) >= UInt64.Parse(numBox_MinVolume.Value.ToString()))
                    transferData.Add(" volume BETWEEN " + numBox_MinVolume.Value + " AND " + numBox_MaxVolume.Value);
                else
                {
                    MessageBox.Show("Invalid set of volume parameters!");
                    return;
                }

            if (chkBox_PriceFilter.Checked)
                if (Decimal.Parse(numBox_MaxPrice.Value.ToString()) >= Decimal.Parse(numBox_MinPrice.Value.ToString()))
                    transferData.Add(" close BETWEEN " + numBox_MinPrice.Value.ToString() + " AND " + numBox_MaxPrice.Value.ToString());
                else
                {
                    MessageBox.Show("Invalid set of price parameters!");
                    return;
                }
            if (txt_StockFilter.Text != "")
            {
                transferData.Add(" symbol = '" + txt_StockFilter.Text.ToString()+"'");
            }

                        
            Thread oFilterExchangeThread = new Thread(new ParameterizedThreadStart(parallelExchangeExtraction));
            oFilterExchangeThread.Start(transferData);
        }
        #endregion

        #region Simple stock processing
        private void btn_ProcessSelectedStocks_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedStocksEntries = dGrid_ExchangeData.SelectedRows;
            if (selectedStocksEntries.Count > 0)
            {
                timePeriod = (Int32)numBox_GraphPeriod.Value;
                timeOffset = (Int32)numBox_GraphOffset.Value;

                List<String> targetSymbols = new List<String>();
                foreach (DataGridViewRow entry in selectedStocksEntries)
                {
                    targetSymbols.Add(entry.Cells[0].Value.ToString());
                }

                switch (selectedChartType)
                {
                    case SeriesChartType.Candlestick:
                        {
                            Object parameters = new Object[] { exchange, targetSymbols[0], timePeriod, timePeriod - timeOffset, chkBox_Weekly.Checked, singleStartDate, singleEndDate };
                            Thread oGraphSingleStockThread = new Thread(new ParameterizedThreadStart(parallelSingleStockExtraction));
                            oGraphSingleStockThread.Start(parameters);
                        }
                        break;
                    case SeriesChartType.Line:
                        {
                            Object parameters = new Object[] { exchange, targetSymbols, timePeriod, timePeriod - timeOffset, chkBox_Weekly.Checked, singleStartDate, singleEndDate };
                            Thread oGraphMultiStockThread = new Thread(new ParameterizedThreadStart(parallelMultiStockExtraction));
                            oGraphMultiStockThread.Start(parameters);
                        }
                        break;
                    default:
                        MessageBox.Show("No Chart Type Selected!");
                        break;
                }
            }
            else
            {
                MessageBox.Show("No Rows Selected!");
            }
        }
        #endregion

        #region Mass Analysis
        private void btn_Analysis_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedStocksEntries = dGrid_ExchangeData.SelectedRows;
            if (selectedStocksEntries.Count > 0)
            {
                timePeriod = (Int32)numBox_GraphPeriod.Value;
                timeOffset = (Int32)numBox_GraphOffset.Value;

                List<String> targetSymbols = new List<String>();
                foreach (DataGridViewRow entry in selectedStocksEntries)
                {
                    targetSymbols.Add(entry.Cells[0].Value.ToString());
                }

                //NOTE: Offset is not used in full analysis
                Object parameters = new Object[] { exchange, targetSymbols, timePeriod, timePeriod - timeOffset, chkBox_Weekly.Checked, singleStartDate, singleEndDate };
                Thread oAnalyzeMultiStockThread = new Thread(new ParameterizedThreadStart(parallelMultiStockAnalysis));
                oAnalyzeMultiStockThread.Start(parameters);
            }
            else
            {
                MessageBox.Show("No Rows Selected!");
            }
        }

        //simulate button click!
        private void btn_Analysis_Click(int period, int offset)
        {
            DataGridViewSelectedRowCollection selectedStocksEntries = dGrid_ExchangeData.SelectedRows;
            if (selectedStocksEntries.Count > 0)
            {
                timePeriod = period;
                timeOffset = offset;

                List<String> targetSymbols = new List<String>();
                foreach (DataGridViewRow entry in selectedStocksEntries)
                {
                    targetSymbols.Add(entry.Cells[0].Value.ToString());
                }

                //NOTE: Offset is not used in full analysis
                Object parameters = new Object[] { exchange, targetSymbols, timePeriod, timePeriod - timeOffset, chkBox_Weekly.Checked, singleStartDate, singleEndDate  };
                Thread oAnalyzeMultiStockThread = new Thread(new ParameterizedThreadStart(parallelMultiStockAnalysis));
                oAnalyzeMultiStockThread.Start(parameters);
            }
            else
            {
                MessageBox.Show("No Rows Selected!");
            }
        }        
        #endregion

        #region Remove a Stock from Collection
        private void btn_RemoveStock_Click(object sender, EventArgs e)
        {
            if (selectedStocks != null)
            {
                DataGridViewSelectedRowCollection selectedStocksEntries = dGrid_ExchangeData.SelectedRows;
                String symbol = selectedStocksEntries[0].Cells[0].Value.ToString();
                selectedStocks.removeStock(symbol);

                List<String> targetSymbols = selectedStocks.getListOfSymbols();
                if (targetSymbols.Count > 0)
                {
                    List<Object> transferData = new List<Object>();
                    transferData.Add(exchange);
                    transferData.Add(targetSymbols);
                    DataSet newData = SecureDBOperations.getInstance().batchExchangeSymbolFilteredExtract(transferData);
                    dGrid_ExchangeData.Invoke(new dlgt_UpdateGridDataView(this.invk_ExchangeListUpdate), newData);
                    Console.WriteLine("Result is " + selectedStocks.Size().ToString() + " Stocks");
                }
                else
                    Console.WriteLine("Filter resulted in empty data set!");
                    //MessageBox.Show("Filter resulted in empty data set!");
            }
            else
                MessageBox.Show("No stocks have been selected");
        }
        #endregion

        

        #region Add Stock Collection to Favorites
        private void btn_AddToFavorites_Click(object sender, EventArgs e)
        {
            List<String> CurrentSymbolSet = new List<String>();
            for (int i = 0; i < dGrid_ExchangeData.Rows.Count; i++)
            {
                CurrentSymbolSet.Add("STOCK_" + exchange + "_" + dGrid_ExchangeData.Rows[i].Cells[0].Value);
            }

            Favorites myFavorites = new Favorites(CurrentSymbolSet, this);
            myFavorites.Show();
        }
        #endregion

        #region Evaluate Portfolio
        //Depracated
        private void btn_EvaluatePorfolio_Click(object sender, EventArgs e)
        {
            if (selectedStocks != null)
            {
                Thread oEvaluationThread = new Thread(new ThreadStart(parallelPortfolioEvaluation));
                oEvaluationThread.Start();
            }
            else
                MessageBox.Show("No stocks have been selected");
        }

        //simulate offset using start offset and end offset
        //after somethings been analyzed from x-y, we check the performance from x-start to y-offset
        private void btn_EvaluatePorfolio_Click(int start_offset, int end_offset)
        {
            if (selectedStocks != null)
            {
                Object parameters = new Object[] {start_offset,end_offset};
                Thread oEvaluationThread = new Thread(new ParameterizedThreadStart(parallelAutoPortfolioEvaluation));
                oEvaluationThread.Start(parameters);
            }
            else
                MessageBox.Show("No stocks have been selected");
        }
        #endregion
        
        #endregion

        #region Automation
        #region SuperAnalysis
        private void btn_SuperAnalyze_Click(object sender, EventArgs e)
        {
            Thread oSuperEvaluationThread = new Thread(new ThreadStart(parallelSuperAnalysis));
            oSuperEvaluationThread.Start();
        }
        #endregion

        #region Automated Filter by Price/Volume Click
        //click the filter exchange button, price and volume filters must be entered manually
        public void Auto_Filter()
        {
            Monitor.Enter(masterlock);
            {
                btn_FilterExchange_Click(null, null);
                Thread.Sleep(filterDelay*2);
            }
            Monitor.Exit(masterlock);
        }
        #endregion

        #region Automated Filter by Fibonnaci Click
        public void Auto_Fibonacci()
        {
            Monitor.Enter(masterlock);
            {
                btn_Fibonnaci_Click(null, null);
                Thread.Sleep(filterDelay);
            }
            Monitor.Exit(masterlock);
        }
        #endregion

        #region Automated Select All
        public void Auto_SelectAll()
        {
            Monitor.Enter(masterlock);
            {
                dGrid_ExchangeData.SelectAll();
                Thread.Sleep(filterDelay*2);
            }
            Monitor.Exit(masterlock);
        }        
        #endregion

        #region Auto Remove Stocks
        private String Auto_RemoveStocks(List<String> symbols)
        {
            String Results = "";

            if (selectedStocks != null)
            {
                //DataGridViewSelectedRowCollection selectedStocksEntries = dGrid_ExchangeData.SelectedRows;
                //String symbol = selectedStocksEntries[0].Cells[0].Value.ToString();


                //Console.WriteLine("Originally " + selectedStocks.Size() + " stocks");
                Results += "Originally " + selectedStocks.Size() + " stocks\n";
                
                foreach (String symbol in symbols)
                {
                    //Console.WriteLine("Auto removing " + symbol);
                    Results += "Auto removing " + symbol + "\n";
                    selectedStocks.removeStock(symbol);
                }
                //Console.WriteLine("Removed stocks with insufficient data, now we have " + selectedStocks.Size() + " stocks");
                Results += "Removed stocks with insufficient data, now we have " + selectedStocks.Size() + " stocks\n";

                List<String> targetSymbols = selectedStocks.getListOfSymbols();
                if (targetSymbols.Count > 0)
                {
                    List<Object> transferData = new List<Object>();
                    transferData.Add(exchange);
                    transferData.Add(targetSymbols);
                    DataSet newData = SecureDBOperations.getInstance().batchExchangeSymbolFilteredExtract(transferData);
                    dGrid_ExchangeData.Invoke(new dlgt_UpdateGridDataView(this.invk_ExchangeListUpdate), newData);
                    //Console.WriteLine("Result is " + selectedStocks.Size().ToString() + " Stocks");
                }
                else
                    Console.WriteLine("Filter resulted in empty data set!");
                //MessageBox.Show("Filter resulted in empty data set!");
            }
            else
                MessageBox.Show("No stocks have been selected");

            return Results;
        }
        #endregion

        #region Automated Analyze Click, period and offset supplied
        //
        public void Auto_Analyze(int period, int offset)
        {
            if (Monitor.TryEnter(masterlock))
            {
                Monitor.Exit(masterlock);
                btn_Analysis_Click(period, offset);
                Thread.Sleep(filterDelay);
            }
            else
            {
                btn_Analysis_Click(period, offset);
                Thread.Sleep(filterDelay);
            }
        }
        #endregion

        #region Automated Evaluation By Projection
        //Pass period, 
        public void Auto_EvaluateProjection(int start, int end, int offset)
        {
            if (selectedStocks.Size() != 0)
            {
                Thread.Sleep(filterDelay*2);
                dGrid_ExchangeData.SelectAll();
                Thread.Sleep(filterDelay);
                btn_Analysis_Click(start, end - offset);
                Thread.Sleep(filterDelay);

                Monitor.Enter(masterlock);
                Monitor.Exit(masterlock);

                btn_EvaluatePorfolio_Click(offset, 0);
            }
            else
                Utilities.WriteToLogFile((start).ToString() + " to " + (end).ToString() + " resulted in an empty set +\n");
        }
        #endregion

        #region Automated Moving Average CrossOver FilterClick
        private void Auto_FilterByCrossOver_Click(Utilities.TrendInterval intervalTrendingAbove, Utilities.TrendInterval intervalTrendingBelow)
        {
            //TODO:Hotfix, 
            Monitor.Enter(masterlock);
            {
                genericFilter(selectedStocks.getStocksCrossingOver, new Object[]{intervalTrendingAbove, intervalTrendingBelow});                
                Thread.Sleep(filterDelay);
            }
            Monitor.Exit(masterlock);
        }
        #endregion

        #region Automated Moving Average Strength Filter Click
        private void Auto_FilterByMovingAverage(Utilities.Trend trend, Utilities.TrendInterval trendInterval, Double minStrength, Double maxStrength)
        {
            //TODO:HOTFIX add exponential boolean
            Monitor.Enter(masterlock);
            {
                if (maxStrength >= minStrength)
                    genericFilter(selectedStocks.getStocksTrendingOver, new Object[] { trend, trendInterval, maxStrength, minStrength });
                Thread.Sleep(filterDelay);
            }
            Monitor.Exit(masterlock);
        }

        #endregion
        #endregion

        #region Working with Graph Data
        #region Graph Type Selection
        private void rdobtn_CandleStick_CheckedChanged(object sender, EventArgs e)
        {
            selectedChartType = SeriesChartType.Candlestick;
            //We now work with candlestick charts
            //box_MovingAverages.Enabled = false;
            
        }

        private void rdobtn_PriceVolume_CheckedChanged(object sender, EventArgs e)
        {
            selectedChartType = SeriesChartType.Line;
            //box_MovingAverages.Enabled = true;
        }
        #endregion

        #region Build Chart
        private void buildCandleStickChart(DataSet croppedData, DataSet fullData)
        {
            if (fullData.Tables[0].Rows.Count == 0 || croppedData.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("No Data within this time Period");
                return;
            }
            List<EnhancedSimpleStockPoint> stockData = Utilities.Convert_DataSet_to_EnhancedStock_ForGraphing(fullData);
            List<EnhancedSimpleStockPoint> stockDataFocusRegion = Utilities.Convert_DataSet_to_EnhancedStock_ForGraphing(croppedData);
                        
            String symbol = stockData[0].getSymbol();
            double minStockValue = Utilities.getStockMinValue(stockData);
            PrepareMarketGraph(symbol, minStockValue);
            LoadCandleStickMarketGraphData(stockData, stockDataFocusRegion);
           
        }

        private void buildSingleStockPriceChart(DataSet croppedData, DataSet fullData)
        {
            if (fullData.Tables[0].Rows.Count == 0 || croppedData.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("No Data within this time Period");
                return;
            }
            List<SimpleStockPoint> stockData = Utilities.Convert_DataSet_to_SimpleStock(fullData);
            List<SimpleStockPoint> stockDataFocusRegion = Utilities.Convert_DataSet_to_SimpleStock(croppedData);
            
            String symbol = stockData[0].getSymbol();
            double minStockValue = Utilities.getStockMinValue(stockData);
            PrepareMarketGraph(symbol, minStockValue);
            LoadPriceMarketGraphData(stockData, stockDataFocusRegion);
        }

        private void buildMultiStockPriceChart(List<DataSet> newDataSets)
        {
            List<List<SimpleStockPoint>> multiSimpleStockDataSet = Utilities.Convert_DataSetList_to_SimpleStockList(newDataSets);
            Utilities.Convert_SimpleMagnitudeLists_to_SimplePercentLists(multiSimpleStockDataSet);
            List<Series> multiStockDataSet = Utilities.Convert_SimpleStocksList_to_SeriesList(multiSimpleStockDataSet);
            double minStockValue = Utilities.getStockMinValue(multiSimpleStockDataSet);
            #region Prepare Market Graph
            MarketGraph.Series.Clear();
            MarketGraph.Titles.Clear();
            MarketGraph.Legends.Clear();
            MarketGraph.ChartAreas.Clear();

            ChartArea chartArea_Price = new ChartArea("Price");

            Legend legend = new Legend("Legend");
            MarketGraph.Legends.Add(legend);
            MarketGraph.Legends[0].LegendStyle = LegendStyle.Row;
            MarketGraph.Legends[0].Position.X = 70;
            MarketGraph.Legends[0].Position.Y = 2;
            MarketGraph.Legends[0].Position.Height = 10;
            MarketGraph.Legends[0].Position.Width = 40;

            MarketGraph.ChartAreas.Add(chartArea_Price);

            MarketGraph.ChartAreas["Price"].Position.X = 0;
            MarketGraph.ChartAreas["Price"].Position.Y = 10;
            MarketGraph.ChartAreas["Price"].Position.Height = 60;
            MarketGraph.ChartAreas["Price"].Position.Width = 100;
            
            MarketGraph.ChartAreas["Price"].AxisY2.Enabled = AxisEnabled.True;
            MarketGraph.ChartAreas["Price"].AxisY.Minimum = minStockValue - minStockValue * 0.1;
            MarketGraph.ChartAreas["Price"].AxisY2.Minimum = MarketGraph.ChartAreas[0].AxisY.Minimum;
            MarketGraph.ChartAreas["Price"].AxisX.Minimum = 0;
            MarketGraph.ChartAreas["Price"].AxisX.MajorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Price"].AxisX.MinorGrid.Enabled = true;
            MarketGraph.ChartAreas["Price"].AxisX.MinorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Price"].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            MarketGraph.ChartAreas["Price"].AxisY2.MajorGrid.LineColor = Color.LightBlue;
            #endregion


            for (int i = 0; i < multiStockDataSet.Count; i++)
            {
                MarketGraph.Series.Add(multiStockDataSet[i]);
                MarketGraph.Series[i].ChartArea = "Price";
                //MarketGraph.Legends.Add(multiStockDataSet[i].Legend);
            }
        }
        #endregion

        #region Prepare MarketGraph
        private void PrepareMarketGraph(String symbol, double minStockValue)
        {
            MarketGraph.Series.Clear();
            MarketGraph.Titles.Clear();
            MarketGraph.Legends.Clear();
            MarketGraph.ChartAreas.Clear();
            

            Series series_StockPrice = new Series("Price");
            Series series_StockPriceOriginal = new Series("PriceOOS");            
            Series series_Volume = new Series("Volume");
            ChartArea chartArea_Price = new ChartArea("Price");
            ChartArea chartArea_Volume = new ChartArea("Volume");
            ChartArea chartArea_Indicator1 = new ChartArea("Indicator1");
            //ChartArea chartArea_Indicator2 = new ChartArea("Indicator2");
            ChartArea chartArea_Indicator3 = new ChartArea("Indicator3");
            ChartArea chartArea_Indicator4 = new ChartArea("Indicator4");
            
            Legend legend = new Legend("Legend");
            MarketGraph.Legends.Add(legend);
            MarketGraph.Legends[0].LegendStyle = LegendStyle.Row;
            MarketGraph.Legends[0].Position.X = 70;
            MarketGraph.Legends[0].Position.Y = 1;
            MarketGraph.Legends[0].Position.Height = 8;
            MarketGraph.Legends[0].Position.Width = 40;

            int priceY = 1;
            int priceHeight = 30;
            int volumeY = priceHeight + priceY + 1;
            int volumeHeight = 10;
            int indicator1Y = volumeY + volumeHeight + 1;
            int indicator1Height = 15;
            //int indicator2Y = indicator1Y + indicator1Height + 1;
            int indicator2Height = 30;

            int indicator3Y = indicator1Y + indicator2Height + 1;
            int indicator3Height = 15;
            int indicator4Y = indicator3Y + indicator1Height + 1;
            int indicator4Height = 15;


            /*
            //TO UNDO: TEMPORARY OVERRIDE
            series_Volume.ChartType = SeriesChartType.Line;
            series_StockPrice.ChartType = SeriesChartType.Line;
            */
            
            series_StockPrice.ChartType = selectedChartType;
            series_StockPriceOriginal.ChartType = selectedChartType;
            series_Volume.ChartType = SeriesChartType.Stock;
            series_Volume.Color = Color.Blue;
            series_Volume.IsVisibleInLegend = false;

            MarketGraph.Titles.Add(symbol);
            MarketGraph.ChartAreas.Add(chartArea_Price);
            MarketGraph.ChartAreas.Add(chartArea_Volume);
            MarketGraph.ChartAreas.Add(chartArea_Indicator1);
            //MarketGraph.ChartAreas.Add(chartArea_Indicator2);
            MarketGraph.ChartAreas.Add(chartArea_Indicator3);
            //MarketGraph.ChartAreas.Add(chartArea_Indicator4);

            MarketGraph.Legends.Add(new Legend("Legend of Indicator1"));
            MarketGraph.Legends["Legend of Indicator1"].DockedToChartArea = "Indicator1";
            //MarketGraph.Legends.Add(new Legend("Legend of Indicator2"));
            //MarketGraph.Legends["Legend of Indicator2"].DockedToChartArea = "Indicator2";
            MarketGraph.Legends.Add(new Legend("Legend of Indicator3"));
            MarketGraph.Legends["Legend of Indicator3"].DockedToChartArea = "Indicator3";
            
            Title Title1 = new Title();
            Title1.Name = "Title of Indicator1";
            MarketGraph.Titles.Add(Title1);
            MarketGraph.Titles["Title of Indicator1"].DockedToChartArea = "Indicator1";

            //Title Title2 = new Title();
            //Title2.Name = "Title of Indicator2";
            //MarketGraph.Titles.Add(Title2);
            //MarketGraph.Titles["Title of Indicator2"].DockedToChartArea = "Indicator2";

            Title Title3 = new Title();
            Title3.Name = "Title of Indicator3";
            MarketGraph.Titles.Add(Title3);
            MarketGraph.Titles["Title of Indicator3"].DockedToChartArea = "Indicator3";

            //Title Title4 = new Title();
            //Title4.Name = "Title of Indicator4";
            //MarketGraph.Titles.Add(Title4);
            //MarketGraph.Titles["Title of Indicator4"].DockedToChartArea = "Indicator4";

            //MarketGraph.Legends.Add(new Legend("Legend of Indicator4"));
            //MarketGraph.Legends["Legend of Indicator4"].DockedToChartArea = "Indicator4";

            MarketGraph.Series.Add(series_StockPriceOriginal);
            MarketGraph.Series.Add(series_StockPrice);
            MarketGraph.Series.Add(series_Volume);

            MarketGraph.ChartAreas["Volume"].AlignWithChartArea = "Price";

            MarketGraph.ChartAreas["Price"].Position.X = 0;
            MarketGraph.ChartAreas["Price"].Position.Y = priceY;
            MarketGraph.ChartAreas["Price"].Position.Height = priceHeight;
            MarketGraph.ChartAreas["Price"].Position.Width = 100;

            MarketGraph.ChartAreas["Volume"].Position.X = 0;
            MarketGraph.ChartAreas["Volume"].Position.Y = volumeY;
            MarketGraph.ChartAreas["Volume"].Position.Height = volumeHeight;
            MarketGraph.ChartAreas["Volume"].Position.Width = 100;

            MarketGraph.Series["Price"].ChartArea = "Price";
            MarketGraph.Series["PriceOOS"].ChartArea = "Price";
            MarketGraph.Series["Volume"].ChartArea = "Volume";
            MarketGraph.Series["Volume"].ChartType = SeriesChartType.Column;
            


            MarketGraph.ChartAreas["Price"].AxisY2.Enabled = AxisEnabled.True;
            MarketGraph.ChartAreas["Price"].AxisY.Minimum = minStockValue;
            MarketGraph.ChartAreas["Price"].AxisY2.Minimum =0;
            MarketGraph.ChartAreas["Price"].AxisX.Minimum = 0;
            MarketGraph.ChartAreas["Price"].AxisX.MajorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Price"].AxisX.MinorGrid.Enabled = true;
            MarketGraph.ChartAreas["Price"].AxisX.MinorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Price"].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            MarketGraph.ChartAreas["Price"].AxisY2.MajorGrid.LineColor = Color.LightBlue;

            MarketGraph.ChartAreas["Volume"].AxisY2.Enabled = AxisEnabled.True;
            MarketGraph.ChartAreas["Volume"].AxisY.Minimum = 0;
            MarketGraph.ChartAreas["Volume"].AxisY2.Minimum = 0;
            MarketGraph.ChartAreas["Volume"].AxisX.Minimum = 0;
            MarketGraph.ChartAreas["Volume"].AxisX.MajorGrid.Enabled = false;
            MarketGraph.ChartAreas["Volume"].AxisX.MinorGrid.Enabled = false;
            MarketGraph.ChartAreas["Volume"].AxisY.MinorGrid.Enabled = false;
            MarketGraph.ChartAreas["Volume"].AxisY.MajorGrid.Enabled = false;
            MarketGraph.ChartAreas["Volume"].AxisY2.MajorGrid.LineColor = Color.LightBlue;

            /*------------------*/

            MarketGraph.ChartAreas["Indicator1"].AxisY2.Enabled = AxisEnabled.True;
            //MarketGraph.ChartAreas["Indicator1"].AxisY.Minimum = 0;
            //MarketGraph.ChartAreas["Indicator1"].AxisY2.Minimum = 0;
            //MarketGraph.ChartAreas["Indicator1"].AxisY.Minimum = 0;
            MarketGraph.ChartAreas["Indicator1"].AxisX.Minimum = 0;
            //MarketGraph.ChartAreas["Indicator1"].AxisY.Maximum = 100;

            //MarketGraph.Series[2].ChartType = SeriesChartType.Candlestick;

            MarketGraph.ChartAreas["Indicator1"].AxisX.MinorGrid.Enabled = true;
            MarketGraph.ChartAreas["Indicator1"].AxisX.MinorGrid.LineColor = Color.LightBlue;            
            MarketGraph.ChartAreas["Indicator1"].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            MarketGraph.ChartAreas["Indicator1"].AxisY.MajorGrid.Enabled = true;
            MarketGraph.ChartAreas["Indicator1"].AxisY.MajorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator1"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            MarketGraph.ChartAreas["Indicator1"].AlignWithChartArea = "Price";            

            MarketGraph.ChartAreas["Indicator1"].Position.X = 0;
            MarketGraph.ChartAreas["Indicator1"].Position.Y = indicator1Y;
            MarketGraph.ChartAreas["Indicator1"].Position.Height = indicator2Height;
            MarketGraph.ChartAreas["Indicator1"].Position.Width = 100;
            /*
            MarketGraph.ChartAreas["Indicator2"].AxisY2.Enabled = AxisEnabled.False;
            MarketGraph.ChartAreas["Indicator2"].AxisX.Minimum = 0;

            MarketGraph.ChartAreas["Indicator2"].AxisX.MinorGrid.Enabled = true;
            MarketGraph.ChartAreas["Indicator2"].AxisX.MinorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator2"].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            MarketGraph.ChartAreas["Indicator2"].AxisY.MajorGrid.Enabled = true;
            MarketGraph.ChartAreas["Indicator2"].AxisY.MajorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator2"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            MarketGraph.ChartAreas["Indicator2"].AlignWithChartArea = "Price";      
            

            MarketGraph.ChartAreas["Indicator2"].Position.X = 0;
            MarketGraph.ChartAreas["Indicator2"].Position.Y = indicator2Y;
            MarketGraph.ChartAreas["Indicator2"].Position.Height = indicator2Height;
            MarketGraph.ChartAreas["Indicator2"].Position.Width = 100;
            */
            MarketGraph.Legends[0].Enabled = false;
            MarketGraph.Legends[1].Enabled = false;
            MarketGraph.Legends[2].Enabled = false;
            //MarketGraph.Legends[3].Enabled = false;            
            
            MarketGraph.ChartAreas["Indicator3"].AxisX.Minimum = 0;
            MarketGraph.ChartAreas["Indicator3"].AxisX.MajorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator3"].AxisX.MinorGrid.Enabled = true;
            MarketGraph.ChartAreas["Indicator3"].AxisX.MinorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator3"].AlignWithChartArea = "Price";

            MarketGraph.ChartAreas["Indicator3"].Position.X = 0;
            MarketGraph.ChartAreas["Indicator3"].Position.Y = indicator3Y;
            MarketGraph.ChartAreas["Indicator3"].Position.Height = indicator3Height;
            MarketGraph.ChartAreas["Indicator3"].Position.Width = 100;

            /*
            MarketGraph.BackColor = Color.Gray;
            MarketGraph.ChartAreas[0].BackColor = Color.Gray;
            MarketGraph.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.White;
            MarketGraph.ChartAreas[0].AxisX.MinorTickMark.LineColor = Color.White;
            MarketGraph.ChartAreas[0].AxisX.InterlacedColor = Color.DarkGray;

            MarketGraph.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.White;
            MarketGraph.ChartAreas[0].AxisY.MinorTickMark.LineColor = Color.White;
            MarketGraph.ChartAreas[0].AxisY.InterlacedColor = Color.DarkGray;
            */

            /*
            MarketGraph.ChartAreas["Indicator4"].AxisX.Minimum = 0;
            MarketGraph.ChartAreas["Indicator4"].AxisX.MajorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator4"].AxisX.MinorGrid.Enabled = true;
            MarketGraph.ChartAreas["Indicator4"].AxisX.MinorGrid.LineColor = Color.LightBlue;
            MarketGraph.ChartAreas["Indicator4"].AlignWithChartArea = "Price";

            MarketGraph.ChartAreas["Indicator4"].Position.X = 0;
            MarketGraph.ChartAreas["Indicator4"].Position.Y = indicator4Height;
            MarketGraph.ChartAreas["Indicator4"].Position.Height = indicator4Y;
            MarketGraph.ChartAreas["Indicator4"].Position.Width = 100;
            */

            if(chkBox_Scroll.Checked)
                for (int i = 0; i < MarketGraph.ChartAreas.Count; i++)
                {
                    MarketGraph.ChartAreas[i].AxisX.ScrollBar.Enabled = true;
                    //MarketGraph.ChartAreas[i].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.ResetZoom;  
                }
            else
                for (int i = 0; i < MarketGraph.ChartAreas.Count; i++)
                {
                    MarketGraph.ChartAreas[i].AxisX.ScrollBar.Enabled = false;
                }
        }
        #endregion

        #region Load Data on to Market Graph

        private void LoadCandleStickMarketGraphData(List<EnhancedSimpleStockPoint> stockData, List<EnhancedSimpleStockPoint> stockDataFocusRegion)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                EnhancedSimpleStockPoint dataForADay = stockData[i];
                MarketGraph.Series["PriceOOS"].Points.AddXY(dataForADay.getDateMod(), dataForADay.getHigh());
                MarketGraph.Series["PriceOOS"].Points[i].YValues[1] = dataForADay.getLow();
                MarketGraph.Series["PriceOOS"].Points[i].YValues[3] = dataForADay.getOpen();
                MarketGraph.Series["PriceOOS"].Points[i].YValues[2] = dataForADay.getClose();

                MarketGraph.Series["Volume"].Points.AddXY(dataForADay.getDateMod(), dataForADay.getVolume());
            }

            for (int i = 0; i < stockDataFocusRegion.Count; i++)
            {
                EnhancedSimpleStockPoint dataForADayFocus = stockDataFocusRegion[i];
                MarketGraph.Series["Price"].Points.AddXY(dataForADayFocus.getDateMod(), dataForADayFocus.getHigh());
                MarketGraph.Series["Price"].Points[i].YValues[1] = dataForADayFocus.getLow();
                MarketGraph.Series["Price"].Points[i].YValues[3] = dataForADayFocus.getOpen();
                MarketGraph.Series["Price"].Points[i].YValues[2] = dataForADayFocus.getClose();
            }

            
        }

        private void LoadPriceMarketGraphData(List<SimpleStockPoint> stockData, List<SimpleStockPoint> stockDataFocusRegion)
        {
            for (int i = 0; i < stockData.Count; i++)
            {
                SimpleStockPoint dataForADay = stockData[i];
                MarketGraph.Series["PriceOOS"].Points.AddXY(dataForADay.getDateMod(), dataForADay.getClose());
                MarketGraph.Series["Volume"].Points.AddXY(dataForADay.getDateMod(), dataForADay.getVolume());
            }

            for (int i = 0; i < stockDataFocusRegion.Count; i++)
            {
                SimpleStockPoint dataForADay = stockDataFocusRegion[i];
                MarketGraph.Series["Price"].Points.AddXY(dataForADay.getDateMod(), dataForADay.getClose());
            }
        }
        #endregion

        /* Perform an analysis of each stock one at a time.
         * Convert stocks to series form (price, volume). Collect MA DataSets and analyze them. This establishes
         * trend, and cross overs, and fibonacci line levels. Collect and Plot MA cross overs. Collect and
         * Plot Fibonacci lines
         */ 
        #region Analysis
        private void buildAnalyzedStockChart(Stock stock)
        {
            List<Series> graphData = stock.ConvertAllDataToSeriesData();
            Series PriceCandle = graphData[0];
            Series Volume = graphData[1];
            Series PriceLine = graphData[2];

            String symbol = stock.getSymbol();
            double minStockValue = stock.getMinStockValue();
            PrepareMarketGraph(symbol, minStockValue);

            MarketGraph.Series.Clear();
            MarketGraph.Series.Add(PriceCandle);
            MarketGraph.Series.Add(Volume);
            MarketGraph.Series.Add(PriceLine);

            MarketGraph.Series[0].ChartArea = "Price";
            MarketGraph.Series[1].ChartArea = "Volume";
            MarketGraph.Series[2].ChartArea = "Price";
            MarketGraph.Series["Volume"].ChartType = SeriesChartType.Column;
            MarketGraph.Series["Volume"].IsVisibleInLegend = false;        

            MarketGraph.Visible = chkBox_AnalyzedGraph.Checked;

            //List<Object[]> AnalyzedResults = AnalyzeWithinMarketGraph(graphData);
            stock.ProcessAnalyzedResults(PriceLine);

           
            

            #region Risk Management
            if (chkBox_RiskManagement.Checked)
            {
                RiskManagement.ManageRiskForStock(stock);
            }
            #endregion

            #region Moving Average CrossOvers
            if (chkBox_MAX_Entry.Checked)
            {
                //TODO:HOTFIX!!
                //List<Series> MACrossOvers = stock.CollectMACrossOvers();

                //foreach (Series CrossOver in MACrossOvers)
                //    MarketGraph.Series.Add(CrossOver);
            }
            #endregion
            

            #region Fibonacci
            /*
            List<Series> FibonacciLines = stock.CollectFibonacciSeries();
            foreach (Series FibonacciLine in FibonacciLines)
                MarketGraph.Series.Add(FibonacciLine);
            */
            #endregion

            
            #region Bollinger Bands
            if (chkBox_Bollinger.Checked)
            {
                List<Series> BollingerBands = stock.CollectBollingerBands();
                if (BollingerBands != null)
                    foreach (Series BollingerBand in BollingerBands)
                        MarketGraph.Series.Add(BollingerBand);
            }
            #endregion

            #region MACD
            if (chkBox_MACD_onPrice.Checked)
            {
                MarketGraph.Series.Add(stock.getMACD_ShortEMA());
                MarketGraph.Series.Add(stock.getMACD_LongEMA());
            }

            switch ((Utilities.DisplayedIndicator)cmbBox_DisplayedIndicator.SelectedValue)
            {
                case Utilities.DisplayedIndicator.ADX:
                    {
                        Series[] Test = stock.CollectADX();

                        if (chkBox_ADX_Indicator.Checked)
                        {
                            MarketGraph.Series.Add(Test[0]);
                            MarketGraph.Series["ADX"].ChartArea = "Indicator1";
                        }
                        
                        if (chkBox_ADX_Directionals.Checked)
                        {
                            MarketGraph.Series.Add(Test[1]);
                            MarketGraph.Series.Add(Test[2]);
                            MarketGraph.Series["-PDI"].ChartArea = "Indicator1";
                            MarketGraph.Series["-MDI"].ChartArea = "Indicator1";
                        }

                        if (chkBox_ADX_TrueRange.Checked)
                        {
                            MarketGraph.Series.Add(Test[3]);
                            MarketGraph.Series["-ATR"].ChartArea = "Indicator1";
                        }
                    }
                    break;
                case Utilities.DisplayedIndicator.MACD:
                    {
                        MarketGraph.Series.Add(stock.getMACD_Diff_EMA());
                        MarketGraph.Series.Add(stock.getMACD_Signal_EMA());
                        MarketGraph.Series.Add(stock.getMACD_HistoGram());

                        MarketGraph.Series["MACD"].ChartArea = "Indicator1";
                        MarketGraph.Series["9-EMA-Signal"].ChartArea = "Indicator1";
                        MarketGraph.Series["MACD-Histogram"].ChartArea = "Indicator1";

                        MarketGraph.Series["MACD"].Legend = "Legend of Indicator1";
                        MarketGraph.Series["9-EMA-Signal"].Legend = "Legend of Indicator1";
                        MarketGraph.Series["MACD-Histogram"].Legend = "Legend of Indicator1";
                        MarketGraph.Titles["Title of Indicator1"].Text = "MACD";
                                         
                    }
                    break;
                case Utilities.DisplayedIndicator.RSI:
                    {
                        Series RSI = stock.CollectRSI();
                        if (RSI != null)
                        {
                            MarketGraph.Series.Add(RSI);
                            MarketGraph.Series["RSI"].ChartArea = "Indicator1";
                            MarketGraph.Titles["Title of Indicator1"].Text = "RSI";
                        }
                    }
                    break;
                case Utilities.DisplayedIndicator.Stochastic:
                    {
                        Series StochasticK = stock.getStochastic();
                        Series StochasticD = stock.getStochasticEMA();
                        if (StochasticD != null && StochasticK != null)
                        {
                            MarketGraph.Series.Add(StochasticK);
                            MarketGraph.Series.Add(StochasticD);
                            MarketGraph.Series["StochasticK"].ChartArea = "Indicator1";
                            MarketGraph.Series["StochasticD"].ChartArea = "Indicator1";
                            MarketGraph.Titles["Title of Indicator1"].Text = "Stochastic";
                        }
                    }
                    break;
                case Utilities.DisplayedIndicator.Force:
                    {
                    }
                    break;
                case Utilities.DisplayedIndicator.MovAvg:
                    {
                        MarketGraph.ChartAreas["Indicator1"].AxisY.Minimum = minStockValue;
                        List<Series> MovingAverages = stock.getMovingAverages();
                        Series SMAQ = MovingAverages[0];
                        Series SMAS = MovingAverages[1];
                        Series SMAI = MovingAverages[2];
                        Series SMAL = MovingAverages[3];
                        Series EMAQ = MovingAverages[4];
                        Series EMAS = MovingAverages[5];
                        Series EMAI = MovingAverages[6];
                        Series EMAL = MovingAverages[7];

                        if (chkBox_SMAQ.Checked)
                        {
                            MarketGraph.Series.Add(SMAQ);
                            MarketGraph.Series["SMAQ"].ChartArea = "Indicator1";
                        }

                        if (chkBox_SMAS.Checked)
                        {
                            MarketGraph.Series.Add(SMAS);
                            MarketGraph.Series["SMAS"].ChartArea = "Indicator1";
                        }

                        if (chkBox_SMAI.Checked)
                        {
                            MarketGraph.Series.Add(SMAI);
                            MarketGraph.Series["SMAI"].ChartArea = "Indicator1";
                        }

                        if (chkBox_SMAL.Checked)
                        {
                            MarketGraph.Series.Add(SMAL);
                            MarketGraph.Series["SMAL"].ChartArea = "Indicator1";
                        }

                        if (chkBox_EMAQ.Checked)
                        {
                            MarketGraph.Series.Add(EMAQ);
                            MarketGraph.Series["EMAQ"].ChartArea = "Indicator1";
                        }

                        if (chkBox_EMAS.Checked)
                        {
                            MarketGraph.Series.Add(EMAS);
                            MarketGraph.Series["EMAS"].ChartArea = "Indicator1";
                        }

                        if (chkBox_EMAI.Checked)
                        {
                            MarketGraph.Series.Add(EMAI);
                            MarketGraph.Series["EMAI"].ChartArea = "Indicator1";
                        }

                        if (chkBox_EMAL.Checked)
                        {
                            MarketGraph.Series.Add(EMAL);
                            MarketGraph.Series["EMAL"].ChartArea = "Indicator1";
                        }
                    }
                    break;
                case Utilities.DisplayedIndicator.Divergeance:
                    {
                        MarketGraph.Series.Add(stock.CollectLocalMax());
                        MarketGraph.Series["MaxDV"].ChartArea = "Indicator1";

                        MarketGraph.Series.Add(stock.CollectLocalMin());
                        MarketGraph.Series["MinDV"].ChartArea = "Indicator1";
                    }
                    break;
                default: 
                    {
                        throw new Exception("ffffffffuuuuuuu");
                    }
                    break;
            }

            /*
            MarketGraph.Series.Add(stock.getMACD_Diff_EMA());
            MarketGraph.Series.Add(stock.getMACD_Signal_EMA());
            MarketGraph.Series.Add(stock.getMACD_HistoGram());

            MarketGraph.Series["MACD"].ChartArea = "Indicator1";
            MarketGraph.Series["9-EMA-Signal"].ChartArea = "Indicator1";
            MarketGraph.Series["MACD-Histogram"].ChartArea = "Indicator1";

            MarketGraph.Series["MACD"].Legend = "Legend of Indicator1";
            MarketGraph.Series["9-EMA-Signal"].Legend = "Legend of Indicator1";
            MarketGraph.Series["MACD-Histogram"].Legend = "Legend of Indicator1";            
            MarketGraph.Titles["Title of Indicator1"].Text = "MACD";

            //TODO:Continue here
            Series MACDCrossOver = stock.CollectMACDCrossOverSignals();
            Series MACDLongs = stock.CollectMACDLongs();
            Series MACDShorts = stock.CollectMACDShorts();
            */
              
             
            //Can be done via custom signals now
            /*
            if (MACDCrossOver != null && chkBox_MACDSignals_onPrice.Checked == true)
            {
                MarketGraph.Series.Add(MACDCrossOver);

                MarketGraph.Series.Add(MACDLongs);
                MarketGraph.Series[MarketGraph.Series.Count - 1].YAxisType = AxisType.Secondary;

                MarketGraph.Series.Add(MACDShorts);
                MarketGraph.Series[MarketGraph.Series.Count - 1].YAxisType = AxisType.Secondary;
            }
            **/
            #endregion

            #region Force Index
            MarketGraph.Series.Add(stock.getForceIndexEMA());
            MarketGraph.Series["Force Index EMA 13"].ChartArea = "Indicator3";
            MarketGraph.Titles["Title of Indicator3"].Text = "Force Index";
            #endregion


            //hard coded disablled channels
            if (false)
            //if (chkBox_EMAS.Checked)
            {
                Series upperchannel = stock.getEnvelopeUpperchannel();
                Series lowerchannel = stock.getEnvelopeUpperchannel();
                if (upperchannel != null && lowerchannel != null)
                {
                    MarketGraph.Series.Add(stock.getEnvelopeUpperchannel());
                    MarketGraph.Series.Add(stock.getEnvelopeLowerchannel());
                    MarketGraph.Series["EnvU"].ChartArea = "Price";
                    MarketGraph.Series["EnvL"].ChartArea = "Price";
                }
            }

            /*
            if (chkBox_RSI.Checked)
            {
                Series RSI = stock.CollectRSI();
                if (RSI != null)
                {
                    MarketGraph.Series.Add(RSI);
                    MarketGraph.Series["RSI"].ChartArea = "Indicator1";
                    MarketGraph.Titles["Title of Indicator1"].Text = "RSI";
                }
            }
            else if (chkBox_Stochastic.Checked)
            {
                Series StochasticK = stock.getStochastic();
                Series StochasticD = stock.getStochasticEMA();
                if (StochasticD != null && StochasticK != null)
                {
                    MarketGraph.Series.Add(StochasticK);
                    MarketGraph.Series.Add(StochasticD);
                    MarketGraph.Series["StochasticK"].ChartArea = "Indicator1";
                    MarketGraph.Series["StochasticD"].ChartArea = "Indicator1";
                    MarketGraph.Titles["Title of Indicator1"].Text = "Stochastic";
                }
            }           
            */

            if (chkBox_HeikinAshi.Checked)
            {
                MarketGraph.Series.Add(stock.getHeikinAshi());
                MarketGraph.Series["Heikin-Ashi"].ChartArea = "Price";
                //MarketGraph.Series["Heikin-Ashi"].Enabled = false;
                MarketGraph.Series[0].Enabled = false;

                //Can be done via custom signals now
                //Series HeikenAshiCrossOver = stock.GrabSpecificIndicator(Utilities.IndicatorSignalNames.HeikenAshi);
                //MarketGraph.Series.Add(HeikenAshiCrossOver);
            }

            /*
            if (true)
            {
                Series[] Test = stock.CollectADX();
                MarketGraph.Series.Add(Test[0]);
                MarketGraph.Series.Add(Test[1]);
                MarketGraph.Series.Add(Test[2]);
                MarketGraph.Series["ADX"].ChartArea = "Indicator1";
                MarketGraph.Series["PDI"].ChartArea = "Indicator1";
                MarketGraph.Series["MDI"].ChartArea = "Indicator1";
            }   
             */ 


            #region Custom
            if (/*Utilities.BuySettings != null && Utilities.BuySettings.SomeConditionSet() &&*/ chkBox_CustomSignals.Checked)
            {
                Series Custom = stock.GrabSpecificIndicator(Utilities.IndicatorSignalNames.CustomXOver);
                Series CustomLongs = stock.CollectCustomLongs();
                Series CustomShorts = stock.CollectCustomShorts();

                
                Custom.Name = "Custom";
                MarketGraph.Series.Add(Custom);
                MarketGraph.Series["Custom"].ChartArea = "Price";


                if (chkBox_ProfitsOnLongs.Checked)
                {
                    MarketGraph.Series.Add(CustomLongs);
                    MarketGraph.Series[MarketGraph.Series.Count - 1].YAxisType = AxisType.Secondary;
                }

                if (chkBox_ProfitsOnShorts.Checked)
                {
                    MarketGraph.Series.Add(CustomShorts);
                    MarketGraph.Series[MarketGraph.Series.Count - 1].YAxisType = AxisType.Secondary;
                }
            }

            #endregion

            Graph_WindowXMin = 0;
            for (int i = 0; i < MarketGraph.Series.Count; i++)
            {
                //if (MarketGraph.Series[i].Points.Count > 0)
                //    setClipRequest((int)Convert.ToInt32(MarketGraph.Series[i].Points[0].XValue));
            }

            
            if(chkBox_Scroll.Checked)
                for (int i = 0; i < MarketGraph.ChartAreas.Count; i++)
                {
                    if(MarketGraph.Series[0].Points.Count - Graph_WindowXMin > Graph_WindowSize)
                        MarketGraph.ChartAreas[i].AxisX.ScaleView.Size = Graph_WindowSize;
                    MarketGraph.ChartAreas[i].AxisX.ScaleView.Position = Graph_WindowXMin;
                    MarketGraph.ChartAreas[i].AxisX.Minimum = Graph_WindowXMin;
                }

            //Series temp = MarketGraph.Series[0];
            //MarketGraph.Series.RemoveAt(0);
            //MarketGraph.Series.Add(temp);
            
        }

        /* Use MSChart to perform MA calculations for selected MA's
         * Return the collection of DataSet's representing those MA's
         */
        private List<Object[]> AnalyzeWithinMarketGraph(List<Series> graphData)
        {
            List<Object []> AnalyzedResults = new List<Object []>();
           
            FinancialCalculations_MovingAverages(graphData[0].Name, AnalyzedResults);

            return AnalyzedResults;
        }        

        private void MultiStockAnalysis(List<DataSet> StocksDataSet, List<String> ValidDates)
        {
            if (chkBox_MaintainList.Checked)
            {
                StockCollection tempSelectedStocks = new StockCollection(StocksDataSet, selectedChartType);

                if (StocksDataSet[0].Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("No Data within this time Period");
                    return;
                }

                foreach (Stock targetStock in tempSelectedStocks.getStockList())
                {
                    Console.Write("Analyzing " + targetStock.getSymbol() + "...");
                    Invoke(new dlgt_UpdateGraph(this.invk_AnalyzedMarketGraphupdate), targetStock);
                    Thread.CurrentThread.Join(delay);
                    Console.WriteLine("Done!");

                }
            }
            else
            {
                selectedStocks = new StockCollection(StocksDataSet, selectedChartType);
                String insufficientDataRemoval = "";
                if (selectedStocks.Size() > 1)
                    insufficientDataRemoval = Auto_RemoveStocks(selectedStocks.getStocksByInsufficientData(timePeriod, timeOffset));
                selectedStocks.ValidDates = ValidDates;
                //selectedStocks.HardCodedStockValidator();

                if (StocksDataSet[0].Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("No Data within this time Period");
                    return;
                }

                foreach (Stock targetStock in selectedStocks.getStockList())
                {
                    Console.Write("Analyzing " + targetStock.getSymbol() + "...");
                    Invoke(new dlgt_UpdateGraph(this.invk_AnalyzedMarketGraphupdate), targetStock);
                    Thread.CurrentThread.Join(delay);
                    Console.WriteLine("Done!");

                }
                Console.WriteLine(insufficientDataRemoval);
                Console.WriteLine("Result is " + selectedStocks.Size().ToString() + " Stocks");
            }
            
        }

        /*
        private DataSet ImplementOffSet(DataSet FullSymbolData)
        {
            DataSet OffsetData = FullSymbolData.Copy();
            if (OffsetData.Tables[0].Rows.Count <= timeOffset)
            {
                Console.WriteLine("Offset implementation failed for " + OffsetData.Tables[0].Rows[0][0].ToString());
                return FullSymbolData;
            }

            while (OffsetData.Tables[0].Rows.Count > FullSymbolData.Tables[0].Rows.Count - timeOffset)
                OffsetData.Tables[0].Rows.RemoveAt(OffsetData.Tables[0].Rows.Count - 1);
            return OffsetData;
        }
        */
        #endregion

        #region Technical Calculations On Graph
        private void FinancialCalculations_MovingAverages(String InputSeries, List<Object[]> AnalyzedResults)
        {            
            //if (selectedChartType == SeriesChartType.Line)
            //{
                #region Calculate Moving Averages
                if (chkBox_SMAQ.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALQ)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.MovingAverage, Utilities.INTERVALQ.ToString(), InputSeries, "SMAQ", Color.HotPink));

                if (chkBox_SMAS.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALS)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.MovingAverage, Utilities.INTERVALS.ToString(), InputSeries, "SMAS", Color.Crimson));

                if (chkBox_SMAI.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALI)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.MovingAverage, Utilities.INTERVALI.ToString(), InputSeries, "SMAI", Color.Red));

                if (chkBox_SMAL.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALL)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.MovingAverage, Utilities.INTERVALL.ToString(), InputSeries, "SMAL", Color.Brown));
                #endregion

                #region Calculate Exponential Moving Average
                if (chkBox_EMAQ.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALQ)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.ExponentialMovingAverage, Utilities.INTERVALQ.ToString(), InputSeries, "EMAQ", Color.HotPink));

                if (chkBox_EMAS.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALS)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.ExponentialMovingAverage, Utilities.INTERVALS.ToString(), InputSeries, "EMAS", Color.Crimson));

                if (chkBox_EMAI.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALI)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.ExponentialMovingAverage, Utilities.INTERVALI.ToString(), InputSeries, "EMAI", Color.Red));

                if (chkBox_EMAL.Checked && MarketGraph.Series[InputSeries].Points.Count >= Utilities.INTERVALL)
                    AnalyzedResults.Add(FinancialDataCrunch(FinancialFormula.ExponentialMovingAverage, Utilities.INTERVALL.ToString(), InputSeries, "EMAL", Color.Brown));
                #endregion
            //}
        }

        private Object[] FinancialDataCrunch(FinancialFormula Technique, String Interval, String Input, String Output, Color Color)
        {
            MarketGraph.DataManipulator.FinancialFormula(Technique, Interval, Input, Output);
            MarketGraph.Series[Output].ChartArea = "Price";
            MarketGraph.Series[Output].ChartType = SeriesChartType.Line;
            MarketGraph.Series[Output].Color = Color;
            DataSet MA = MarketGraph.DataManipulator.ExportSeriesValues(Output);
            return new Object[] { MA, Output };
        }

        
        #endregion
        #endregion

        #region Called from External Forms
        public void FilterBySelectedFavorites(DataSet newData)
        {
            dGrid_ExchangeData.Invoke(new dlgt_UpdateGridDataView(this.invk_ExchangeListUpdate), newData);
        }

        #endregion

        #region All Filters        

        #region Moving Average Filter, specific trend, direction, and strength of trend
        private void btn_FilterMA_Click(object sender, EventArgs e)
        {
            //TODO:HOTFIX add exponential boolean
            Utilities.Trend specifiedTrend = (Utilities.Trend)Enum.Parse(typeof(Utilities.Trend), cmbBox_TrendDirection.Text);
            Utilities.TrendInterval specifiedInterval = (Utilities.TrendInterval)Enum.Parse(typeof(Utilities.TrendInterval), cmbBox_Interval.Text);
            Double maxStrength = Double.Parse(numBox_TrendStrengthMax.Value.ToString());
            Double minStrength = Double.Parse(numBox_TrendStrengthMin.Value.ToString());
            Boolean exponential = chkBox_MA_Exponential.Checked;

            if (maxStrength >= minStrength)
                genericFilter(selectedStocks.getStocksTrendingOver, new Object[] { specifiedTrend, specifiedInterval, maxStrength, minStrength, exponential });
        }
        #endregion

        #region Moving Average Filter, make sure prices are within a certain Gap from from MA
        private void btn_PriceGapFromMA_Click(object sender, EventArgs e)
        {
            Double Gap = (Double)numBox_PriceGap.Value;
            Utilities.TrendInterval specifiedInterval = (Utilities.TrendInterval)Enum.Parse(typeof(Utilities.TrendInterval), cmbBox_Interval.Text);
            if (chkBox_PriceAboveMA.Checked == true || chkBox_PriceBelowMA.Checked == true)
                genericFilter(selectedStocks.getStocksByRangeWithinMA, new Object[] { specifiedInterval, Gap, chkBox_PriceAboveMA.Checked, chkBox_PriceBelowMA.Checked });
        }
        #endregion

        #region Moving Average CrossOver Filter
        private void btn_FilterByCrossOver_Click(object sender, EventArgs e)
        {
            Utilities.MA_around_MA QtoS = (Utilities.MA_around_MA)Enum.Parse(typeof(Utilities.MA_around_MA), cmbBox_MACross_QtoS.Text);
            Utilities.MA_around_MA StoI = (Utilities.MA_around_MA)Enum.Parse(typeof(Utilities.MA_around_MA), cmbBox_MACross_StoI.Text);
            Utilities.MA_around_MA ItoL = (Utilities.MA_around_MA)Enum.Parse(typeof(Utilities.MA_around_MA), cmbBox_MACross_ItoL.Text);
            Boolean exponential = chkBox_MA_Exponential.Checked;
            genericFilter(selectedStocks.getStocksCrossingOver, new Object[] { QtoS, StoI, ItoL, exponential });
        }
        #endregion

        #region Fibonnaci Filter
        //TODO:FAILED FILTER
        private void btn_Fibonnaci_Click(object sender, EventArgs e)
        {
            if (selectedStocks != null)
            {
                selectedStocks.getStocksByFibonacciStyle(Utilities.FibonacciFlag.BounceUp);

                List<String> targetSymbols = selectedStocks.getListOfSymbols();
                if (targetSymbols.Count > 0)
                {
                    List<Object> transferData = new List<Object>();
                    transferData.Add(exchange);
                    transferData.Add(targetSymbols);
                    DataSet newData = SecureDBOperations.getInstance().batchExchangeSymbolFilteredExtract(transferData);
                    dGrid_ExchangeData.Invoke(new dlgt_UpdateGridDataView(this.invk_ExchangeListUpdate), newData);
                    Console.WriteLine("Result is " + selectedStocks.Size().ToString() + " Stocks");
                }
                else
                    Console.WriteLine("Filter resulted in empty data set!");
                //MessageBox.Show("Filter resulted in empty data set!");
            }
            else
                MessageBox.Show("No stocks have been selected");
        }
        #endregion

        #region RSI Filter
        private void btn_FilterByRSI_Click(object sender, EventArgs e)
        {
            Utilities.RSIType rsiType = (Utilities.RSIType)Enum.Parse(typeof(Utilities.RSIType), cmbBox_RSI.Text);
            genericFilter(selectedStocks.getStocksByRSI, new Object[] {rsiType});            
        }
        #endregion

        #region Doji Filter
        private void btn_FilterByDoji_Star_Click(object sender, EventArgs e)
        {
            bool dojiStar = chkBox_Doji_Star.Checked;
            bool dojiDragonFly = chkBox_Doji_DragonFly.Checked;
            bool dojiGraveStone = chkBox_Doji_GraveStone.Checked;
            bool dojiPierce = chkBox_Doji_Piercing.Checked;
            bool dojiBearishEngulf = chkBox_Doji_Bearish_Engulfing.Checked;
            bool dojiBullishEngulf = chkBox_Doji_Bullish_Engulfing.Checked;
            bool dojiDarkCloud = chkBox_DarkCloud.Checked;

            genericFilter(selectedStocks.getStocksByDojiExclusive, new Object[] {dojiStar, dojiDragonFly, dojiGraveStone, dojiPierce, dojiBearishEngulf, dojiBullishEngulf, dojiDarkCloud});
        }
        #endregion

        #region Kangaroo Filter
        private void btn_FilterByKangaroo_Click(object sender, EventArgs e)
        {
            genericFilter(selectedStocks.getStocksBySouthernKangarooTail, new Object[] { });
        }
        #endregion

        #region Stochastic Filter
        private void btn_FilterByStochastic_Click(object sender, EventArgs e)
        {
            Utilities.StochasticType stochasticType = (Utilities.StochasticType)Enum.Parse(typeof(Utilities.StochasticType), cmbBox_Stochastic.Text);

            genericFilter(selectedStocks.getStocksByStochastic, new Object[] { stochasticType });
        }
        #endregion

        #region Force filter
        private void btn_FilterByForce_Click(object sender, EventArgs e)
        {
            Utilities.ForceType forceType = (Utilities.ForceType)Enum.Parse(typeof(Utilities.ForceType), cmbBox_Force.Text);

            genericFilter(selectedStocks.getStocksByForce, new Object[] { forceType });
        }
        #endregion

        #region Heikenashi Filter
        private void btn_FilterByHeikinAshi_Click(object sender, EventArgs e)
        {            
            Utilities.HeikenAshiType desiredSignal = (Utilities.HeikenAshiType)Enum.Parse(typeof(Utilities.HeikenAshiType), cmbBox_HeikenAshi.Text);
            int strength = (int)numBox_HeikinAshiStrength.Value;

            genericFilter(selectedStocks.getStocksByHeikenAshi, new Object[] { desiredSignal, strength });
        }
        #endregion

        #region MACD Filter
        private void btn_FilterByMACD_Click(object sender, EventArgs e)
        {
            Utilities.MACD_around_Signal macdAroundSignal = (Utilities.MACD_around_Signal)cmbBox_MACDaroundSignal.SelectedValue;
            Utilities.MACD_around_Zero macdAroundZero = (Utilities.MACD_around_Zero)Enum.Parse(typeof(Utilities.MACD_around_Zero), cmbBox_MACDaroundZero.Text);

            genericFilter(selectedStocks.getStocksByMACD, new Object[] { macdAroundSignal, macdAroundZero });
        }
        #endregion

        #region Bollinger Filter
        private void btn_FilterByBollinger_Click(object sender, EventArgs e)
        {
            Utilities.BandZones targetZone = (Utilities.BandZones)Enum.Parse(typeof(Utilities.BandZones), cmbBox_TargetBand.Text);

            genericFilter(selectedStocks.getStocksByBollinger, new Object[] { targetZone });
        }
        #endregion

        private void genericFilter(dlgt_Filter filter, Object args)
        {
            if (selectedStocks != null)
            {
                filter.Invoke(args);
                List<String> targetSymbols = selectedStocks.getListOfSymbols();
                if (targetSymbols.Count > 0)
                {
                    List<Object> transferData = new List<Object>();
                    transferData.Add(exchange);
                    transferData.Add(targetSymbols);
                    DataSet newData = SecureDBOperations.getInstance().batchExchangeSymbolFilteredExtract(transferData);
                    dGrid_ExchangeData.Invoke(new dlgt_UpdateGridDataView(this.invk_ExchangeListUpdate), newData);
                    Console.WriteLine("Result is " + selectedStocks.Size().ToString() + " Stocks");
                }
                else
                    Console.WriteLine("Filter resulted in empty data set!");
                //MessageBox.Show("Filter resulted in empty data set!");
            }
            else
                MessageBox.Show("No stocks have been selected");
        }


        #endregion


        #region Form Reactive Elements

        private void chkBox_ReactiveAnalysis_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBox_ReactiveAnalysis.Checked)
            {
                dGrid_ExchangeData.Enabled = false;
                btn_FilterByRSI.Enabled = false;
            }
            else
            {
                dGrid_ExchangeData.Enabled = true;
                btn_FilterByRSI.Enabled = true;
            }

        }

        #region RSI Form Elements
        private void numBox_RSI_Interval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.RSI_period = (Int32)numBox_RSI_Interval.Value;
            ReactiveAnalysis();
        }

        private void numBox_RSI_UpperBound_ValueChanged(object sender, EventArgs e)
        {
            Utilities.RSI_UpperBound = (Int32)numBox_RSI_UpperBound.Value;
            ReactiveAnalysis();
        }

        private void numBox_RSI_LowerBound_ValueChanged(object sender, EventArgs e)
        {
            Utilities.RSI_LowerBound = (Int32)numBox_RSI_LowerBound.Value;
            ReactiveAnalysis();
        }
        #endregion

        #region Stochastic Form Elements
        private void numBox_Stochastic_Interval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.Stochastic_period = (Int32)numBox_Stochastic_Interval.Value;
            ReactiveAnalysis();
        }

        private void numBox_Stochastic_UpperBound_ValueChanged(object sender, EventArgs e)
        {
            Utilities.Stochastic_UpperBound = (Int32)numBox_Stochastic_Interval.Value;
            ReactiveAnalysis();
        }

        private void numBox_Stochastic_LowerBound_ValueChanged(object sender, EventArgs e)
        {
            Utilities.Stochastic_LowerBound = (Int32)numBox_Stochastic_Interval.Value;
            ReactiveAnalysis();
        }
        #endregion

        #region MACD Form Elements
        private void numBox_MACD_LongEMA_ValueChanged(object sender, EventArgs e)
        {
            Utilities.MACD_longEMA_period = (Int32)numBox_MACD_LongEMA.Value;
            ReactiveAnalysis();
        }

        private void numBox_MACD_ShortEMA_ValueChanged(object sender, EventArgs e)
        {
            Utilities.MACD_shortEMA_period = (Int32)numBox_MACD_ShortEMA.Value;
            ReactiveAnalysis();
        }

        private void numBox_MACD_SignalEMA_ValueChanged(object sender, EventArgs e)
        {
            Utilities.MACD_signalEMA_period = (Int32)numBox_MACD_SignalEMA.Value;
            ReactiveAnalysis();
        }
        #endregion

        #region Force Form Elements
        private void numBox_Force_Interval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.ForceIndex_EMA_period = (Int32)numBox_Force_Interval.Value;
            ReactiveAnalysis();
        }
        #endregion


        #region Form Heikenashi Elements
        private void chkBox_HeikinAshi_CheckedChanged(object sender, EventArgs e)
        {
            ReactiveAnalysis();
        }
        #endregion


        #region Moving Average Form Elements
        private void numBox_QInterval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.INTERVALQ = (int)numBox_QInterval.Value;
            numBox_SInterval.Minimum = numBox_QInterval.Value + 1;
            ReactiveAnalysis();
        }

        private void numBox_SInterval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.INTERVALS = (int)numBox_SInterval.Value;
            numBox_QInterval.Maximum = numBox_SInterval.Value - 1;
            numBox_IInterval.Minimum = numBox_SInterval.Value + 1;
            ReactiveAnalysis();
        }

        private void numBox_IInterval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.INTERVALI = (int)numBox_IInterval.Value;
            numBox_SInterval.Maximum = numBox_IInterval.Value - 1;
            numBox_LInterval.Minimum = numBox_IInterval.Value + 1;
            ReactiveAnalysis();
        }

        private void numBox_LInterval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.INTERVALL = (int)numBox_LInterval.Value;
            numBox_IInterval.Maximum = numBox_LInterval.Value - 1;
            ReactiveAnalysis();
        }
        #endregion

        #region Bollinger Form elements
        private void numBox_BollingerSTD_ValueChanged(object sender, EventArgs e)
        {
            Utilities.BollingerSTDRatio = (Double)numBox_BollingerSTD.Value;
            ReactiveAnalysis();
        }

        private void chkBox_Bollinger_CheckedChanged(object sender, EventArgs e)
        {
            chkBox_SMAS.Checked = true;
        }
        #endregion

        #region HeikinAshi Form Elements
        private void numBox_HeikinAshiStrength_ValueChanged(object sender, EventArgs e)
        {
            Utilities.HeikenAshi_period = (int)numBox_HeikinAshiStrength.Value;
            ReactiveAnalysis();
        }
        #endregion

        #region ADX Form Elements
        private void numBox_ADX_Interval_ValueChanged(object sender, EventArgs e)
        {
            Utilities.ADX_period = (int)numBox_ADX_Interval.Value;
            ReactiveAnalysis();
        }
        #endregion

        #region Other Form Elements

        private void chkBox_Weekly_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBox_Weekly.Checked)
            {
                numBox_GraphPeriod.Value = numBox_GraphPeriod.Value / 5;
                numBox_GraphOffset.Value = numBox_GraphOffset.Value / 5;
            }
            else
            {
                numBox_GraphPeriod.Value = numBox_GraphPeriod.Value * 5;
                numBox_GraphOffset.Value = numBox_GraphOffset.Value * 5;
            }
        }
        

        private void numBox_GraphOffset_ValueChanged(object sender, EventArgs e)
        {
            ReactiveAnalysis();
        }

        

        private void numBox_Capital_ValueChanged(object sender, EventArgs e)
        {
            Utilities.capital = (double)numBox_Capital.Value;
        }

        private void numBox_RiskTolerance_ValueChanged(object sender, EventArgs e)
        {
            Utilities.riskTolerance = (double)numBox_RiskTolerance.Value;
        }

        private void numBox_FilterDuration_ValueChanged(object sender, EventArgs e)
        {
            Utilities.filterDuration = (int)numBox_FilterDuration.Value;
            //ReactiveAnalysis();
        }

        //TODO:Finish
        private void date_Start_ValueChanged(object sender, EventArgs e)
        {
            if (date_Start.Value.CompareTo(date_Buy.Value) > 0)
                date_Buy.Value = date_Start.Value;

            if (date_Start.Value.CompareTo(date_Sell.Value) > 0)
                date_Sell.Value = date_Start.Value;

            int day = date_Start.Value.Day;
            int month = date_Start.Value.Month;
            int year = date_Start.Value.Year;
            singleStartDate = year * 10000 + month * 100 + day;
        }

        //TODO:Finish
        private void date_End_ValueChanged(object sender, EventArgs e)
        {
            if (date_End.Value.CompareTo(date_Sell.Value) < 0)
                date_Sell.Value = date_End.Value;

            if (date_End.Value.CompareTo(date_Buy.Value) < 0)
                date_Buy.Value = date_End.Value;

            int day = date_End.Value.Day;
            int month = date_End.Value.Month;
            int year = date_End.Value.Year;
            singleEndDate = year * 10000 + month * 100 + day;
        }

        private void numBox_FilterWindow_ValueChanged(object sender, EventArgs e)
        {
            Utilities.filterWindow = (int)numBox_FilterWindow.Value;
            //ReactiveAnalysis();
        }

        #endregion




        #endregion


        private void ReactiveAnalysis()
        {
            if (chkBox_ReactiveAnalysis.Checked)
            {
                timePeriod = (Int32)numBox_GraphPeriod.Value;
                timeOffset = (Int32)numBox_GraphOffset.Value;
                Object transferData = new Object[] {timePeriod, timeOffset};
                Thread oReactionThread = new Thread(new ParameterizedThreadStart(parallelReactiveAnalysis));
                oReactionThread.Start(transferData);
                oReactionThread.Join();
            }
        }
        

        private void btn_FilterByStock_Click(object sender, EventArgs e)
        {

        }

        

        //TODO:Finish
        private void btn_SingleStockAnalyze_Click(object sender, EventArgs e)
        {
            int day = date_Buy.Value.Day;
            int month = date_Buy.Value.Month;
            int year = date_Buy.Value.Year;
            int buyDate = year * 10000 + month * 100 + day;

            Series buySell = new Series("BuySell");
            buySell.MarkerColor = Color.Green;
            buySell.Color = Color.Green;
            buySell.MarkerStyle = MarkerStyle.Cross;
            buySell.ChartType = SeriesChartType.Line;
            buySell.MarkerSize = 10;

            if (selectedStocks != null && selectedStocks.getStockList().Count == 1)
            {
                Object objPoint = selectedStocks.getStockList()[0].getStockPointByDate(buyDate);
                if (objPoint == null)
                {
                    Console.WriteLine("There was an error");
                    return;
                }
                EnhancedSimpleStockPoint selectedPoint = (EnhancedSimpleStockPoint)(objPoint as Object[])[0];
                int buyX = (int)(objPoint as Object[])[1];
                buySignal = new Signal(Utilities.Command.Buy, buyDate, buyDate, selectedPoint.getClose(), Utilities.exchange, selectedPoint.getSymbol());
                buySell.Points.Add(new DataPoint(buyX, buySignal.price));
            }

            day = date_Sell.Value.Day;
            month = date_Sell.Value.Month;
            year = date_Sell.Value.Year;
            int sellDate = year * 10000 + month * 100 + day;

            if (selectedStocks != null && selectedStocks.getStockList().Count == 1)
            {
                Object objPoint = selectedStocks.getStockList()[0].getStockPointByDate(sellDate);
                EnhancedSimpleStockPoint selectedPoint = (EnhancedSimpleStockPoint)(objPoint as Object[])[0];
                int sellX = (int)(objPoint as Object[])[1];
                //TODO:CONTINUE HEREERHERE
                sellSignal = new Signal(Utilities.Command.Sell, sellDate, sellDate, selectedPoint.getClose(), Utilities.exchange, selectedPoint.getSymbol());
                buySell.Points.Add(new DataPoint(sellX,sellSignal.price));
            }

            for (int i = 0; i < MarketGraph.Series.Count; i++)
            {
                if (MarketGraph.Series[i].Name == "BuySell")
                {
                    MarketGraph.Series.RemoveAt(i);
                    break;
                }
            }

            MarketGraph.Series.Add(buySell);
            MarketGraph.Series["BuySell"].ChartArea = "Price";

        }

        

        private void SingleStockAnalyzer_Load(object sender, EventArgs e)
        {

        }

        private void btn_Filter_Universal_Click(object sender, EventArgs e)
        {
            if (chkBox_Bollinger.Checked)
            {
                btn_FilterByBollinger_Click(null, null);
            }

            if (chkBox_HeikinAshi.Checked)
            {
                btn_FilterByHeikinAshi_Click(null, null);
            }

            if (chkBox_Force.Checked)
            {
                btn_FilterByForce_Click(null, null);
            }

            if (chkBox_MACD.Checked)
            {
                btn_FilterByMACD_Click(null, null);
            }

            if (chkBox_RSI.Checked)
            {
                btn_FilterByRSI_Click(null, null);
            }

            if (chkBox_Stochastic.Checked)
            {
                btn_FilterByStochastic_Click(null, null);
            }

            if (chkBox_MovingAverages.Checked)
            {
                btn_FilterByCrossOver_Click(null, null);
                btn_FilterMA_Click(null, null);
            }

            if (chkBox_Doji.Checked)
            {
                btn_FilterByDoji_Star_Click(null, null);
            }

        }

        private void btn_CaptureBuySettings_Click(object sender, EventArgs e)
        {
            bool dojiStar = chkBox_Doji_Star.Checked;
            bool dojiDragonFly = chkBox_Doji_DragonFly.Checked;
            bool dojiGraveStone = chkBox_Doji_GraveStone.Checked;
            bool dojiPierce = chkBox_Doji_Piercing.Checked;
            bool dojiBearishEngulf = chkBox_Doji_Bearish_Engulfing.Checked;
            bool dojiBullishEngulf = chkBox_Doji_Bullish_Engulfing.Checked;
            bool dojiDarkCloud = chkBox_DarkCloud.Checked;
            int HeikenAshiStrength = (Int32)numBox_HeikinAshiStrength.Value;
            bool[] DojiConditions = new bool[] {dojiStar, dojiDragonFly, dojiGraveStone, dojiPierce, dojiBearishEngulf, dojiBullishEngulf, dojiDarkCloud };

            Utilities.BandZones BollingerBandRange = (Utilities.BandZones) cmbBox_TargetBand.SelectedValue ;
            Utilities.ForceType ForceType = (Utilities.ForceType) cmbBox_Force.SelectedValue;
            Utilities.RSIType RSIType = (Utilities.RSIType) cmbBox_RSI.SelectedValue;            
            Utilities.StochasticType StochasticType = (Utilities.StochasticType) cmbBox_Stochastic.SelectedValue;
            Utilities.MACD_around_Signal MACD_Around_Signal = (Utilities.MACD_around_Signal) cmbBox_MACDaroundSignal.SelectedValue;
            Utilities.MACD_around_Zero MACD_Around_Zero = (Utilities.MACD_around_Zero) cmbBox_MACDaroundZero.SelectedValue;
            Utilities.HeikenAshiType HeikenAshiType = (Utilities.HeikenAshiType) cmbBox_HeikenAshi.SelectedValue;

            Utilities.MA_around_MA MACross_QtoS = (Utilities.MA_around_MA)cmbBox_MACross_QtoS.SelectedValue;
            Utilities.MA_around_MA MACross_StoI = (Utilities.MA_around_MA)cmbBox_MACross_StoI.SelectedValue;
            Utilities.MA_around_MA MACross_ItoL = (Utilities.MA_around_MA)cmbBox_MACross_ItoL.SelectedValue;
            Utilities.Stock_around_MA StockCrossMA = (Utilities.Stock_around_MA)cmbBox_Stock_to_MA.SelectedValue;

            
            Utilities.ADXTrend ADXTrend = (Utilities.ADXTrend)cmbBox_ADXStrength.SelectedValue;
            Utilities.ADX_PDIvsNDI ADX_PDIvsNDI = (Utilities.ADX_PDIvsNDI)cmbBox_PDI_vs_NDI.SelectedValue;
            Utilities.ADX_Trending ADXTrending = (Utilities.ADX_Trending)cmbBox_ADX_Trending.SelectedValue;

            Utilities.Trend MA_specifiedTrend = (Utilities.Trend)Enum.Parse(typeof(Utilities.Trend), cmbBox_TrendDirection.Text);
            Utilities.TrendInterval MA_specifiedInterval = (Utilities.TrendInterval)Enum.Parse(typeof(Utilities.TrendInterval), cmbBox_Interval.Text);
            Double MA_maxStrength = Double.Parse(numBox_TrendStrengthMax.Value.ToString());
            Double MA_minStrength = Double.Parse(numBox_TrendStrengthMin.Value.ToString());

            List<Object> MA_Conditions = new List<Object>() { MA_specifiedTrend, MA_specifiedInterval, MA_maxStrength, MA_minStrength, MACross_QtoS, MACross_StoI, MACross_ItoL, StockCrossMA };

            Utilities.BuySettings = new TradeSettings(Utilities.TradeType.Buy,
                new object[] { BollingerBandRange, ForceType, RSIType, StochasticType, MACD_Around_Signal, MACD_Around_Zero, HeikenAshiType,
                    ADXTrend, ADX_PDIvsNDI, DojiConditions , ADXTrending, MA_Conditions, HeikenAshiStrength });

            Utilities.BuySettings.FilterBy_Bollinger = chkBox_Bollinger.Checked;
            Utilities.BuySettings.FilterBy_Force = chkBox_Force.Checked; 
            Utilities.BuySettings.FilterBy_MACD = chkBox_MACD.Checked; 
            Utilities.BuySettings.FilterBy_MA = chkBox_MovingAverages.Checked; 
            Utilities.BuySettings.FilterBy_RSI = chkBox_RSI.Checked; 
            Utilities.BuySettings.FilterBy_Stochastic = chkBox_Stochastic.Checked;
            Utilities.BuySettings.FilterBy_HeikenAshi = chkBox_HeikinAshi.Checked;
            Utilities.BuySettings.FilterBy_Doji = chkBox_Doji.Checked;
            Utilities.BuySettings.Filterby_ADX = chkBox_ADX.Checked;
            Utilities.BuySettings.FilterBy_MA = chkBox_MovingAverages.Checked;
            Utilities.BuySettings.MA_exponential = chkBox_MA_Exponential.Checked;
            Console.WriteLine("Custom Buy Setting Captured");

        }

        private void btn_CaptureSellSettings_Click(object sender, EventArgs e)
        {
            bool dojiStar = chkBox_Doji_Star.Checked;
            bool dojiDragonFly = chkBox_Doji_DragonFly.Checked;
            bool dojiGraveStone = chkBox_Doji_GraveStone.Checked;
            bool dojiPierce = chkBox_Doji_Piercing.Checked;
            bool dojiBearishEngulf = chkBox_Doji_Bearish_Engulfing.Checked;
            bool dojiBullishEngulf = chkBox_Doji_Bullish_Engulfing.Checked;
            bool dojiDarkCloud = chkBox_DarkCloud.Checked;
            int HeikenAshiStrength = (Int32)numBox_HeikinAshiStrength.Value;
            bool[] DojiConditions = new bool[] { dojiStar, dojiDragonFly, dojiGraveStone, dojiPierce, dojiBearishEngulf, dojiBullishEngulf, dojiDarkCloud };

            Utilities.BandZones BollingerBandRange = (Utilities.BandZones)cmbBox_TargetBand.SelectedValue;
            Utilities.ForceType ForceType = (Utilities.ForceType)cmbBox_Force.SelectedValue;
            Utilities.RSIType RSIType = (Utilities.RSIType)cmbBox_RSI.SelectedValue;
            Utilities.StochasticType StochasticType = (Utilities.StochasticType)cmbBox_Stochastic.SelectedValue;
            Utilities.MACD_around_Signal MACD_Around_Signal = (Utilities.MACD_around_Signal)cmbBox_MACDaroundSignal.SelectedValue;
            Utilities.MACD_around_Zero MACD_Around_Zero = (Utilities.MACD_around_Zero)cmbBox_MACDaroundZero.SelectedValue;
            Utilities.HeikenAshiType HeikenAshiType = (Utilities.HeikenAshiType)cmbBox_HeikenAshi.SelectedValue;

            Utilities.MA_around_MA MACross_QtoS = (Utilities.MA_around_MA)cmbBox_MACross_QtoS.SelectedValue;
            Utilities.MA_around_MA MACross_StoI = (Utilities.MA_around_MA)cmbBox_MACross_StoI.SelectedValue;
            Utilities.MA_around_MA MACross_ItoL = (Utilities.MA_around_MA)cmbBox_MACross_ItoL.SelectedValue;
            Utilities.Stock_around_MA StockCrossMA = (Utilities.Stock_around_MA)cmbBox_Stock_to_MA.SelectedValue;
            
            Utilities.ADXTrend ADXTrend = (Utilities.ADXTrend)cmbBox_ADXStrength.SelectedValue;
            Utilities.ADX_PDIvsNDI ADX_PDIvsNDI = (Utilities.ADX_PDIvsNDI)cmbBox_PDI_vs_NDI.SelectedValue;
            Utilities.ADX_Trending ADXTrending = (Utilities.ADX_Trending)cmbBox_ADX_Trending.SelectedValue;

            Utilities.Trend MA_specifiedTrend = (Utilities.Trend)Enum.Parse(typeof(Utilities.Trend), cmbBox_TrendDirection.Text);
            Utilities.TrendInterval MA_specifiedInterval = (Utilities.TrendInterval)Enum.Parse(typeof(Utilities.TrendInterval), cmbBox_Interval.Text);
            Double MA_maxStrength = Double.Parse(numBox_TrendStrengthMax.Value.ToString());
            Double MA_minStrength = Double.Parse(numBox_TrendStrengthMin.Value.ToString());

            List<Object> MA_Conditions = new List<Object>(){MA_specifiedTrend, MA_specifiedInterval, MA_maxStrength, MA_minStrength, MACross_QtoS, MACross_StoI, MACross_ItoL, StockCrossMA};

            Utilities.SellSettings = new TradeSettings(Utilities.TradeType.Sell,
                new object[] { BollingerBandRange, ForceType, RSIType, StochasticType, MACD_Around_Signal, MACD_Around_Zero, HeikenAshiType,
                    ADXTrend, ADX_PDIvsNDI, DojiConditions , ADXTrending, MA_Conditions, HeikenAshiStrength });
                   
            Utilities.SellSettings.FilterBy_Bollinger = chkBox_Bollinger.Checked;
            Utilities.SellSettings.FilterBy_Force = chkBox_Force.Checked;
            Utilities.SellSettings.FilterBy_MACD = chkBox_MACD.Checked;
            Utilities.SellSettings.FilterBy_MA = chkBox_MovingAverages.Checked;
            Utilities.SellSettings.FilterBy_RSI = chkBox_RSI.Checked;
            Utilities.SellSettings.FilterBy_Stochastic = chkBox_Stochastic.Checked;
            Utilities.SellSettings.FilterBy_HeikenAshi = chkBox_HeikinAshi.Checked;
            Utilities.SellSettings.FilterBy_Doji = chkBox_Doji.Checked;
            Utilities.SellSettings.Filterby_ADX = chkBox_ADX.Checked;
            Utilities.SellSettings.FilterBy_MA = chkBox_MovingAverages.Checked;
            Utilities.SellSettings.MA_exponential = chkBox_MA_Exponential.Checked;
            Console.WriteLine("Custom Sell Setting Captured");
        }

        private void btn_ClearTradeSettings_Click(object sender, EventArgs e)
        {
            Utilities.BuySettings = null;
            Utilities.SellSettings = null;
            Console.WriteLine("Custom Trade Settings Cleared"); 
        }

        private void btn_AutoTune_Click(object sender, EventArgs e)
        {
            if (chkBox_RSI.Checked)
            {
                for (int i = 0; i < 10; i++)
                {
                    numBox_RSI_Interval.Value++;
                    //Thread.Sleep(500);
                    //Thread t = new Thread();
                    //t.joi
                }
            }
        }

        private void date_Buy_ValueChanged(object sender, EventArgs e)
        {

        }

        private void date_Sell_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btn_FilterByADX_Click(object sender, EventArgs e)
        {
            Utilities.ADXTrend ADXTrend = (Utilities.ADXTrend)cmbBox_ADXStrength.SelectedValue;
            Utilities.ADX_PDIvsNDI PDIvsNDI = (Utilities.ADX_PDIvsNDI)cmbBox_PDI_vs_NDI.SelectedValue;
            Utilities.ADX_Trending ADXTrending = (Utilities.ADX_Trending)cmbBox_ADX_Trending.SelectedValue;
            
            genericFilter(selectedStocks.getStocksByADX, new Object[] { ADXTrend, PDIvsNDI, ADXTrending });
        }

        private void numBox_ADX_IterativeSmoothing_ValueChanged(object sender, EventArgs e)
        {
            Utilities.ADX_smoothing_iterations = (int)(numBox_ADX_IterativeSmoothing.Value);
        }

        private void cmbBox_ExitType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Utilities.SelectedExit = (Utilities.ExitType)cmbBox_ExitType.SelectedValue;
        }

        private void btn_Evaluation_Click(object sender, EventArgs e)
        {
            Evaluation myEvaluation = new Evaluation();
            myEvaluation.Show();
        }

        //Clear all saved portfolios      
        private void btn_ClearPortfolio_Click(object sender, EventArgs e)
        {
            SecureDBOperations.getInstance().clearEvaluations();
            //lBox_LoadedPortfolios.Items.Clear();
            //Utilities.Portfolio.Clear(); //********
            loadEvaluations();
        }

        //Record to Saved Portfolios
        private void btn_Record_to_Saved_Portfolio_Click(object sender, EventArgs e)
        {
            if (txt_PortfolioName.Text == "")
                return;
            //SecureDBOperations.getInstance().setupEvaluations();
            bool DuplicateSeriesName = false;

            foreach (Series item in Utilities.Portfolio)
                if (item.Name == txt_PortfolioName.Text)
                    DuplicateSeriesName = true;

            if (!DuplicateSeriesName)
            {

                List<Utilities.StocksProfit> profits = Utilities.ProfitsByPercent_Longs;
                List<Utilities.StocksProfit> profits2 = new List<Utilities.StocksProfit>();
                List<Double> SignalsToDisplay = new List<double>();
                var lengths = from element in profits
                              orderby element.PercentProfit
                              select element;

                foreach (Utilities.StocksProfit SingleStockProfit in lengths)
                {
                    profits2.Add(SingleStockProfit);
                    if (Math.Abs(SingleStockProfit.PercentProfit) < 200)
                        SignalsToDisplay.Add(SingleStockProfit.PercentProfit);
                    else
                    {
                        SignalsToDisplay.Add(200);
                        Console.WriteLine(SingleStockProfit.Symbol + " was out of bounds");
                    }
                }
                String PortfolioName = txt_PortfolioName.Text;
                Series mySeries = Utilities.Convert_DoubleList_to_Series(SignalsToDisplay, 1, PortfolioName);
                Utilities.Portfolio.Add(mySeries);

                SecureDBOperations.getInstance().insertEvaluationsData(new object[] {PortfolioName, SignalsToDisplay});

                lBox_LoadedPortfolios.Items.Clear();
                foreach (Series item in Utilities.Portfolio)
                    lBox_LoadedPortfolios.Items.Add(item.Name);

                loadEvaluations();

                for (int i = 0; i < profits2.Count; i++)
                {
                    Console.WriteLine(profits2[i].Symbol + " , " + profits2[i].PercentProfit);
                }
            }
        }

        

        private void btn_RemovePortfolio_Click(object sender, EventArgs e)
        {
            int selectedIndex = lBox_LoadedPortfolios.SelectedIndex;

            if (lBox_LoadedPortfolios.Items.Count > 0)
            {
                Utilities.Portfolio.RemoveAt(selectedIndex);
                lBox_LoadedPortfolios.Items.RemoveAt(selectedIndex);
                lBox_LoadedPortfolios.Update();
            }
        }

        private void loadEvaluations()
        {
            SecureDBOperations.getInstance().setupEvaluations();
            DataSet SavedEvaluations = SecureDBOperations.getInstance().loadEvaluations();
            lBox_SavedPortfolios.DataSource = SavedEvaluations.Tables[0];
            lBox_SavedPortfolios.DisplayMember = "name";
            lBox_SavedPortfolios.ValueMember = "id";
        }

            

        private void btn_Add_to_Portfolios_Click(object sender, EventArgs e)
        {
            //Make sure item we're about to push over doesn't already exist in the portfolio
            bool DuplicateSeriesName = false;
            foreach (Series item in Utilities.Portfolio)
                if (item.Name == lBox_SavedPortfolios.Text)
                    DuplicateSeriesName = true;            

            if (!DuplicateSeriesName)
            {
                int selectedID = Convert.ToInt32(lBox_SavedPortfolios.SelectedValue);
                DataSet selectedData = SecureDBOperations.getInstance().getEvaluationsData(new Object[] { selectedID });
                Series selectedSeries = Utilities.Convert_DataSet_Evaluations_to_Series(selectedData, "Table", lBox_SavedPortfolios.Text, SeriesChartType.Line, MarkerStyle.Diamond, Color.Black, Color.Red);

                Utilities.Portfolio.Add(selectedSeries);
                lBox_LoadedPortfolios.Items.Add(lBox_SavedPortfolios.Text);
            }               
        }

        private void btn_Evaluate_Portfolio_Click(object sender, EventArgs e)
        {
            invk_StatGraphUpdate(Utilities.Portfolio);           
        }
               
        private void btn_ClearPortfolio(object sender, EventArgs e)
        {
            lBox_LoadedPortfolios.Items.Clear();
            Utilities.Portfolio.Clear();
        }

        private void btn_Remove_from_Saved_Click(object sender, EventArgs e)
        {
            int selectedID = Convert.ToInt32(lBox_SavedPortfolios.SelectedValue);
            int selectedIndex = lBox_SavedPortfolios.SelectedIndex;

            if (selectedIndex >= 0)
            {
                SecureDBOperations.getInstance().RemoveEvaluationsData(new Object[] { selectedID });

                DataSet SavedEvaluations = SecureDBOperations.getInstance().loadEvaluations();
                lBox_SavedPortfolios.DataSource = SavedEvaluations.Tables[0];
                lBox_SavedPortfolios.DisplayMember = "name";
                lBox_SavedPortfolios.ValueMember = "id";
                lBox_SavedPortfolios.Update();
            }

        }


    }
}
