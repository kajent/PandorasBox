using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace PandorasBox
{
    public partial class DataImporter : Form
    {
        private List<String> Exchanges = new List<String> { "Invalid", "NASDAQ", "NYSE" };
        private List<String> DailyDataFiles = new List<String>();

        public DataImporter()
        {            
            InitializeComponent();
            //  DO NOT RUN THIS UNNECESSARILY 
            /*
            SecureDBOperations.getInstance().dropSelectedDate("20100711", "NYSE");         */   
            cmbBox_Exchange.DataSource = Exchanges;
        }

        #region Browsing Buttons
        private void btn_browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseFolder = new FolderBrowserDialog();            
            if (browseFolder.ShowDialog() == DialogResult.Cancel)
                return;
            else
            {
                try
                {
                    
                    txt_ArchiveFolderPath.Text = browseFolder.SelectedPath;
                }
                catch (Exception)
                {
                    MessageBox.Show("Error opening file", "File Error");
                }
            }
        }

        private void btn_BrowseSymbols_Click(object sender, EventArgs e)
        {
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Filter = "CSV Files (*.csv)|*.csv";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;
            else
            {
                try
                {
                    txt_SymbolsFilePath.Text = browseFile.FileName;
                }
                catch (Exception)
                {
                    MessageBox.Show("Error opening file", "File Error");
                }
            }
        }

        private void btn_BrowseDailyData_Click(object sender, EventArgs e)
        {
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Multiselect = true;
            browseFile.Filter = "Text Files (*.txt)|*.txt";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;
            else
            {
                try
                {
                    DailyDataFiles.Clear();
                    foreach (String fileName in browseFile.FileNames)
                    {
                        DailyDataFiles.Add(fileName);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error opening file", "File Error");
                }
            }
        }
        #endregion

        #region Clear Database Contents
        private void btn_ClearStocks_Click(object sender, EventArgs e)
        {
            Thread oClearStockThread = new Thread(new ThreadStart(SecureDBOperations.getInstance().batchStockEmpty));
            oClearStockThread.Start();
        }

        private void btn_DropStocks_Click(object sender, EventArgs e)
        {
            Thread oDropStockThread = new Thread(new ThreadStart(SecureDBOperations.getInstance().batchStockDrop));
            oDropStockThread.Start();
        }
        
        private void btn_ClearWeeklyStocks_Click(object sender, EventArgs e)
        {
            Thread oClearWeeklyStockThread = new Thread(new ThreadStart(SecureDBOperations.getInstance().batchWeeklyStockEmpty));
            oClearWeeklyStockThread.Start();
        }
        #endregion

        private void btn_SingleStockAnalyzer_Click(object sender, EventArgs e)
        {
            
            String exchange = cmbBox_Exchange.Items[cmbBox_Exchange.SelectedIndex].ToString();
            if (exchange != "Invalid")
            {
                SingleStockAnalyzer singleStockAnalyzer = new SingleStockAnalyzer(exchange);
                singleStockAnalyzer.Show();
            }
        }

        #region Import Buttons
        private void btn_ImportFiles_Click(object sender, EventArgs e)
        {
            if (txt_ArchiveFolderPath.Text != "")
            {
                String folderPath = txt_ArchiveFolderPath.Text;
                cmbBox_Exchange.SelectedIndex = Utilities.SelectProperExchange(Exchanges, folderPath);
                String exchange = cmbBox_Exchange.Items[cmbBox_Exchange.SelectedIndex].ToString();
                if (exchange != "Invalid")
                {
                    Object[] importArgs = new Object[] { exchange, folderPath };
                    Thread oImportStocksThread = new Thread(new ParameterizedThreadStart(SecureDBOperations.getInstance().batchCSVImport));
                    oImportStocksThread.Start(importArgs);
                }
            }
        }
        private void btn_LoadSymbols_Click(object sender, EventArgs e)
        {
            if (File.Exists(txt_SymbolsFilePath.Text))
            {
                Thread oImportSymbolsThread = new Thread(new ParameterizedThreadStart(SecureDBOperations.getInstance().batchSymbolImport));
                oImportSymbolsThread.Start(txt_SymbolsFilePath.Text);
            }
            else
                MessageBox.Show("Specified File Does not Exist!");
        }


        #endregion

        private void btn_LoadSingleDay_Click(object sender, EventArgs e)
        {
            foreach(String DailyDataFile in DailyDataFiles)
            if (File.Exists(DailyDataFile))
            {
                String filePath = DailyDataFile;
                cmbBox_Exchange.SelectedIndex = Utilities.SelectProperExchange(Exchanges, filePath);
                String exchange = cmbBox_Exchange.Items[cmbBox_Exchange.SelectedIndex].ToString();
                if (exchange != "Invalid")
                {
                    Object[] importArgs = new Object[] { exchange, filePath };
                    Thread oImportSingleDaysDataThread = new Thread(new ParameterizedThreadStart(SecureDBOperations.getInstance().batchSingleDayImport));
                    oImportSingleDaysDataThread.Start(importArgs);
                }

                
            }
            else
                MessageBox.Show("Specified File Does not Exist!");

        }

        private void btn_CreateWeeklyData_Click(object sender, EventArgs e)
        {            
            String exchange = cmbBox_Exchange.Items[cmbBox_Exchange.SelectedIndex].ToString();
            if (exchange != "Invalid")
            {                
                Thread oConvertDailyToWeeklyThread = new Thread(new ParameterizedThreadStart(SecureDBOperations.getInstance().convertAllDailyToWeeklyForGivenExchange));
                oConvertDailyToWeeklyThread.Start(exchange);
            }
        }

        

    }
}
