using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace PandorasBox
{
    public partial class Favorites : Form
    {
        private List<String> _stocks;
        private SingleStockAnalyzer _parentStockAnalyzer;

        public Favorites()
        {
            InitializeComponent();
        }

        public Favorites(List<String> SelectedStocks, SingleStockAnalyzer StockAnalyzer)
        {
            _parentStockAnalyzer = StockAnalyzer;
            InitializeComponent();
            LoadList(SelectedStocks);
        }

        private void LoadList(List<String> StockSet)
        {
            lBox_TargetStocks.DataSource = StockSet;
            _stocks = StockSet;

            Thread oLoadStockSetsThread = new Thread(new ThreadStart(parallelStockSetsLoad));
            oLoadStockSetsThread.Start() ;
        }

        
        

        #region delegates

        public delegate void dlgt_UpdateListBox(DataSet dataSet);

        #endregion

        #region multithreaded db calls

        public void parallelStockSave(object TransferData)
        {
            SecureDBOperations.getInstance().saveFavoriteStocks(TransferData);
            DataSet newSavedData = SecureDBOperations.getInstance().loadFavoriteStockSets();
            lBox_FavoriteStocks.Invoke(new dlgt_UpdateListBox(this.updateSavedFavorites), newSavedData);
        }

        public void parallelStockSetsLoad()
        {
            DataSet savedData = SecureDBOperations.getInstance().loadFavoriteStockSets();
            lBox_FavoriteStocks.Invoke(new dlgt_UpdateListBox(this.updateSavedFavorites), savedData);
        }

        public void parallelDropFavs()
        {
            SecureDBOperations.getInstance().dropFavorites();
            DataSet newSavedData = SecureDBOperations.getInstance().loadFavoriteStockSets();
            lBox_FavoriteStocks.Invoke(new dlgt_UpdateListBox(this.updateSavedFavorites), newSavedData);            
        }

        public void parallelStocksLoad(object TransferData)
        {
            DataSet loadData = SecureDBOperations.getInstance().loadSelectedFavoriteStocks(TransferData);
            lBox_TargetStocks.Invoke(new dlgt_UpdateListBox(this.updateTargets), loadData);
            _stocks = Utilities.Convert_DataSet_to_StringList(loadData, 1);
        }

        #endregion

        #region buttons
        private void btn_SaveStocks_Click(object sender, EventArgs e)
        {
            if (Utilities.IsAlphaSpacedNumeric(txt_StockSetName.Text) &&
                txt_StockSetDescription.Text.Length > 0 &&
                lBox_TargetStocks.Items.Count > 0 &&
                !lBox_FavoriteStocks.Items.Contains(txt_StockSetName.Text))
            {

                String StockSetName = txt_StockSetName.Text;
                String StockSetDescription = txt_StockSetDescription.Text;
                List<String> SelectedStocks = lBox_TargetStocks.DataSource as List<String>;

                Object TransferData = new Object[] { StockSetName, StockSetDescription, SelectedStocks };

                Thread oSaveStockSetThread = new Thread(new ParameterizedThreadStart(parallelStockSave));
                oSaveStockSetThread.Start(TransferData);
            }
        }

        private void updateSavedFavorites(DataSet savedFavorites)
        {
            lBox_FavoriteStocks.DataSource = Utilities.Convert_DataSet_to_StringList(savedFavorites, 1);
        }

        private void updateTargets(DataSet selectedFavorites)
        {
            lBox_TargetStocks.DataSource = Utilities.Convert_DataSet_to_StringList(selectedFavorites, 1);
        }

        private void btn_DropFavorites_Click(object sender, EventArgs e)
        {
            Thread oDropFavsThread = new Thread(new ThreadStart(parallelDropFavs));
            oDropFavsThread.Start();
            
        }

        private void btn_LoadStocks_Click(object sender, EventArgs e)
        {
            if (lBox_FavoriteStocks.SelectedItem != null)
            {
                Thread oLoadSelectedStocksThread = new Thread(new ParameterizedThreadStart(parallelStocksLoad));
                oLoadSelectedStocksThread.Start(lBox_FavoriteStocks.SelectedValue.ToString());
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void btn_UseTargetStocks_Click(object sender, EventArgs e)
        {
            if (_stocks.Count > 0)
            {
                if (_stocks[0].Contains(_parentStockAnalyzer.exchange.ToUpper()))
                {
                    List<Object> transferData = new List<Object>();
                    transferData.Add(_parentStockAnalyzer.exchange);

                    List<String> symbols = new List<string>();
                    foreach (String stock in _stocks)
                        symbols.Add(stock.Split('_')[2]);
                    transferData.Add(symbols);
                    DataSet favData = SecureDBOperations.getInstance().batchExchangeSymbolFilteredExtract(transferData);
                    _parentStockAnalyzer.FilterBySelectedFavorites(favData);
                    this.Close();
                }
                else
                    MessageBox.Show("Stockset selected does not belong to current exchange");
            }
            else
                MessageBox.Show("Stockset is empty");
        }
        #endregion
    }
}
