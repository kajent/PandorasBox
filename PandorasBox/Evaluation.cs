using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace PandorasBox
{
    public partial class Evaluation : Form
    {
        private Graph StatisticsGraph;

        public Evaluation()
        {            
            InitializeComponent();

            if (Utilities.Portfolio.Count > 0)
                foreach (Series item in Utilities.Portfolio)
                    lBox_Portfolios.Items.Add(item.Name);
        }

        ~Evaluation()
        {
            Console.WriteLine("Destruct!");
        }

       

        private void btn_RecordPortfolio_Click(object sender, EventArgs e)
        {

            bool DuplicateSeriesName = false;

            foreach(Series item in Utilities.Portfolio)
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
                    if (Math.Abs(SingleStockProfit.PercentProfit) < 500)
                        SignalsToDisplay.Add(SingleStockProfit.PercentProfit);
                    else
                        Console.WriteLine(SingleStockProfit.Symbol + " was out of bounds");
                }
                String PortfolioName = txt_PortfolioName.Text;
                Series mySeries = Utilities.Convert_DoubleList_to_Series(SignalsToDisplay, 0, PortfolioName);
                Utilities.Portfolio.Add(mySeries);

                lBox_Portfolios.Items.Clear();
                foreach (Series item in Utilities.Portfolio)
                    lBox_Portfolios.Items.Add(item.Name);
            }
        }

        private void btn_ClearPortfolio_Click(object sender, EventArgs e)
        {
            lBox_Portfolios.Items.Clear();
            Utilities.Portfolio.Clear();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            lBox_Portfolios.Items.Clear();
            this.Close();
        }

        private void btn_EvaluateSelectedStocks_Click(object sender, EventArgs e)
        {
            StatisticsGraph = new Graph();
            StatisticsGraph.Show();
            
            
            Chart StatGraph = StatisticsGraph.getGraph();
            StatGraph.Series.Clear();
            StatGraph.ChartAreas.Add(new ChartArea());

            Legend testLegend = new Legend();
            StatGraph.Legends.Add(testLegend);

            List<Series> newSeriesCollection = new List<Series>();
            newSeriesCollection = Utilities.Portfolio;
            //for(int i = 0; i < newSeriesCollection.Count; i++)
            //    StatGraph.Series.Add(newSeriesCollection[i]);
            /*
            Series newSeriesData = (Series)(Data);
            StatGraph.Series.Add(newSeriesData);
             */ 
        }
    }
}
