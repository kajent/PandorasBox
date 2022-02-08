using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.IO;

namespace PandorasBox
{
    class SecureDBOperations
    {
        private static SecureDBOperations instance;
        static readonly object masterlock = new object();

        private SecureDBOperations()
        {
        }

        public static SecureDBOperations getInstance()
        {
            lock (masterlock)
            {
                if (instance == null)
                {
                    instance = new SecureDBOperations();
                    return instance;
                }
                else
                {
                    return instance;
                }
            }
        }

        #region Importer

        public void batchSymbolImport(object filePath)
        {
            lock (masterlock)
            {
                Console.Write("Importing " + filePath as String + "...");
                Console.Out.Flush();
                DataBase.importSymbols(DataBase.getInstance().DBConnection, filePath as String);
                Console.WriteLine("Done!");
            }
        }

        public void batchCSVImport(object CSVImportArgs)
        {
            String exchange = ((CSVImportArgs as Object[])[0] as String);
            String folderPath = ((CSVImportArgs as Object[])[1] as String);

            lock (masterlock)
            {
                String[] filePaths = Directory.GetFiles(folderPath as String);
                foreach (String file in filePaths)
                {
                    if (file.Substring(file.Length - 4, 4) == ".csv")
                    {
                        Console.Write("Import " + file + " ...");
                        Console.Out.Flush();
                        DataBase.importCSV(DataBase.getInstance().DBConnection, file, exchange);
                        Console.WriteLine("Done!");
                        Console.Out.Flush();
                    }
                }
            }
        }

        public void batchSingleDayImport(object FileImportArgs)
        {
            lock (masterlock)
            {
                String exchange = ((FileImportArgs as Object[])[0] as String);
                String file = ((FileImportArgs as Object[])[1] as String);

                StreamReader SR = File.OpenText(file);
                String entry;
                entry = SR.ReadLine();

                while (entry != null)
                {
                    String[] splitEntry = entry.Split(',');
                    if (splitEntry.Length == 7)
                    {

                        String symbol = splitEntry[0];
                        String date = splitEntry[1];
                        String open = splitEntry[2];
                        String high = splitEntry[3];
                        String low = splitEntry[4];
                        String close = splitEntry[5];
                        String volume = splitEntry[6];
                        DataBase.importSingleDaysData(DataBase.getInstance().DBConnection, symbol, date, open, high, low, close, volume, exchange);
                    }
                    else
                        Console.WriteLine("Corrupt entry encountered!");
                    entry = SR.ReadLine();
                }
                Console.WriteLine("Import Complete!!!");
            }
        }

        public void batchStockDrop()
        {
            lock (masterlock)
            {
                DataBase.dropAllTables(DataBase.getInstance().DBConnection);
            }
        }

        public void batchStockEmpty()
        {
            lock (masterlock)
            {
                DataBase.emptyAllTables(DataBase.getInstance().DBConnection);
            }
        }

        public void batchWeeklyStockEmpty()
        {
            lock (masterlock)
            {
                DataBase.emptyAllWeeklyTables(DataBase.getInstance().DBConnection);
            }
        }

        #endregion

        public DataSet batchExchangeExtract(object transferData)
        {
            DataSet result = null;
            List<Object> transferredData = transferData as List<Object>;
            string exchange = transferredData[0] as String;
            List<String> filters = new List<string>();
            for (int i = 1; i < transferredData.Count; i++)
            {
                filters.Add(transferredData[i] as String);
            }
            
            lock (masterlock)
            {
                result = DataBase.getExchangeData(DataBase.getInstance().DBConnection, exchange, filters as List<String>);
            }
            return result;
        }

        public DataSet batchExchangeSymbolFilteredExtract(object transferData)
        {   //transferData = {String exchange, List<String> symbols}
            DataSet result = null;
            List<Object> transferredData = transferData as List<Object>;
            string exchange = transferredData[0] as String;
            List<String> requestedSymbols = transferredData[1] as List<String>;

            lock (masterlock)
            {
                result = DataBase.getStockSymbolsByName(DataBase.getInstance().DBConnection, exchange, requestedSymbols);
            }
            return result;
        }

        public DataSet singleStockDataExtract(Object parameters)
        {
            String exchange = (parameters as Object[])[0] as String;
            String symbol = (parameters as Object[])[1] as String;
            String dateLimit = (parameters as Object[])[2].ToString();
            String offset = (parameters as Object[])[3].ToString();
            String dateStart = (parameters as Object[])[5].ToString();
            String dateEnd = (parameters as Object[])[6].ToString();
            DataSet result = null;

            bool weekly = Convert.ToBoolean((parameters as Object[])[4].ToString());
            lock (masterlock)
            {
                if (!weekly)                    
                    {                        
                        result = DataBase.getSingleStockData(DataBase.getInstance().DBConnection, symbol, dateLimit, exchange, offset, dateStart, dateEnd);
                    }
                else
                    {
                        result = DataBase.getSingleWeeklyStockData(DataBase.getInstance().DBConnection, symbol, dateLimit, exchange, offset);
                    }
            }
           
            return result;
        }

        public DataSet singleStockDates(Object parameters)
        {
            String exchange = (parameters as Object[])[0] as String;
            //String symbol = (parameters as Object[])[1] as String;
            String symbol = "AAPL";
            String dateLimit = (parameters as Object[])[2].ToString();
            String offset = (parameters as Object[])[3].ToString();
            DataSet result = null;
            lock (masterlock)
            {
                result = DataBase.getSingleStockDates(DataBase.getInstance().DBConnection, symbol, dateLimit, exchange, offset);
            }
            return result;
        }

        public List<DataSet> multiStockDataExtract(Object parameters)
        {
            String exchange = (parameters as Object[])[0] as String;
            List<String> symbols = (parameters as Object[])[1] as List<String>;
            String dateLimit = (parameters as Object[])[2].ToString();
            String offset = (parameters as Object[])[3].ToString();            
            String dateStart = (parameters as Object[])[5].ToString();
            String dateEnd = (parameters as Object[])[6].ToString();
            


            List<DataSet> results = new List<DataSet>();


            bool weekly = Convert.ToBoolean((parameters as Object[])[4].ToString());
            lock (masterlock)
            {
                if(!weekly)
                    foreach (String symbol in symbols)
                    {
                        DataSet result = null;
                        result = DataBase.getSingleStockData(DataBase.getInstance().DBConnection, symbol, dateLimit, exchange, offset, dateStart, dateEnd);
                        if (result != null)
                            results.Add(result);
                    }
                else
                    foreach (String symbol in symbols)
                    {
                        DataSet result = null;
                        result = DataBase.getSingleWeeklyStockData(DataBase.getInstance().DBConnection, symbol, dateLimit, exchange, offset);
                        if (result != null)
                            results.Add(result);
                    }
            }
            return results;
        }

        
        public void saveFavoriteStocks(Object parameters)
        {
            String FavLabel = (parameters as Object[])[0] as String;
            String FavDescription = (parameters as Object[])[1] as String;
            List<String> FavSymbols = (parameters as Object[])[2] as List<String>;
            lock (masterlock)
            {
                DataBase.saveFavoriteStocks(DataBase.getInstance().DBConnection, FavSymbols, FavLabel, FavDescription);
            }
        }

        public DataSet loadFavoriteStockSets()
        {
            DataSet result = null;
            lock (masterlock)
            {
                result = DataBase.loadFavoriteStockSets(DataBase.getInstance().DBConnection);
            }
            return result;
        }

        //Delete all favorite stocks from db
        public void dropFavorites()
        {
            lock (masterlock)
            {
                DataBase.dropFavorites(DataBase.getInstance().DBConnection);
            }
        }

        public DataSet loadSelectedFavoriteStocks(Object parameters)
        {
            String name = (String)parameters;
            DataSet result = null;
            lock (masterlock)
            {
                result = DataBase.loadSelectedFavoriteStocks(DataBase.getInstance().DBConnection, name);
            }
            return result;
        }

        public void convertAllDailyToWeeklyForGivenExchange(object exchange)
        {
            lock (masterlock)
            {
                DataBase.ConvertToWeeklyData(DataBase.getInstance().DBConnection, exchange as String);
            }
        }

        public DataSet loadEvaluations()
        {
            DataSet result = null;
            lock (masterlock)
            {
                result = DataBase.loadSavedPortfolios(DataBase.getInstance().DBConnection);
            }
            return result;
        }

        public void setupEvaluations()
        {
            lock (masterlock)
            {
                DataBase.setupEvaluations(DataBase.getInstance().DBConnection);
            }
        }

        public void clearEvaluations()
        {
            lock (masterlock)
            {
                DataBase.clearEvaluations(DataBase.getInstance().DBConnection);
            }
        }

        public void insertEvaluationsData(Object parameters)
        {
            String NameOfPortfolio = (parameters as Object[])[0] as String;
            List<Double> Data = (parameters as Object[])[1] as List<Double>;

            lock (masterlock)
            {
                DataBase.InsertEvaluations(DataBase.getInstance().DBConnection, NameOfPortfolio, Data);
            }
        }

        public DataSet getEvaluationsData(Object parameters)
        {
            int id =  (Int32)((parameters as Object[])[0]);
            //string name = (parameters as Object[])[1] as String;
            DataSet result = null;

            lock (masterlock)
            {
                result = DataBase.GetEvaluations(DataBase.getInstance().DBConnection, id);
            }
            return result;
        }

        public void RemoveEvaluationsData(Object parameters)
        {
            int id = (Int32)((parameters as Object[])[0]);
            lock (masterlock)
            {
                DataBase.RemoveEvaluations(DataBase.getInstance().DBConnection, id);
            }

        }


        #region ErrorHandling
        public void dropSelectedDate(String Date, String Exchange)
        {
            lock (masterlock)
            {
                DataSet TargetTables = DataBase.getSpecificStockTables(DataBase.getInstance().DBConnection, Exchange);
                List<String> ListOfTables = Utilities.Convert_DataSet_to_StringList(TargetTables, 0);
                foreach (String Table in ListOfTables){
                    DataBase.deleteDateFromStockTables(DataBase.getInstance().DBConnection, Table, Date);
                }
            }
        }
        #endregion
    }
}
