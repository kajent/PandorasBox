using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.MySqlClient;

namespace PandorasBox
{
    class DataBase
    {
        private static DataBase instance = null;
        public MySqlConnection DBConnection = null;
        static String connTargetDB;
        static String targetDB;
        static String connLocalServer;
        static String connUserVariables;

        static readonly object masterlock = new object();

        //no instantiation allowed
        private DataBase() 
        {
            //String RemoteServer = "Server=duffbeer.doesntexist.com;";
            //String RemotePort = "PORT=3306;";
            connLocalServer = "Server=localhost;";
            connUserVariables = "Allow User Variables=True;";
            connTargetDB = "DATABASE=Pandora;";
            targetDB = "Pandora";
            String MyConnectString = connLocalServer + connUserVariables + connTargetDB +
                "UID=root;" + "PASSWORD=abc123;";
            DBConnection = new MySqlConnection(MyConnectString);
            try
            {
                DBConnection.Open();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static DataBase getInstance()
        {
            lock (masterlock)
            {
                if (instance == null)
                {
                    instance = new DataBase();
                    return instance;
                }
                else
                {
                    return instance;
                }
            }
        }

        public static DataSet crunchQuery(MySqlConnection Conn, String SQLQuery)
        {
            if (Conn.State.ToString() == "Open")
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery,Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        #region Importer

        public static void importCSV(MySqlConnection Conn, String filePath, String exchange)
        {
            if (Conn.State.ToString() == "Open")
            {
                int lastSlash = filePath.LastIndexOf('\\');
                int period = filePath.LastIndexOf('.');
                String tableName = "`STOCK_" + exchange + "_" + filePath.Substring(lastSlash + 1, period - lastSlash - 1) + "`";
                String SQLQuery = "CREATE TABLE IF NOT EXISTS " + targetDB + "." + tableName + "(" +
                    "symbol  varchar(10)," +
                    "date    int unique," +
                    "open    decimal(8,4)," +
                    "high    decimal(8,4)," +
                    "low     decimal(8,4)," +
                    "close   decimal(8,4)," +
                    "volume  bigint," +
                    "index (date)"+
                    ");";

                MySqlCommand command = Conn.CreateCommand();
                command.CommandText = SQLQuery;
                MySqlDataReader reader = command.ExecuteReader();
                reader.Close();

                for (int i = 0; i < filePath.Length; i++)
                {
                    if (filePath[i] == '\\')
                    {
                        filePath = filePath.Substring(0, i) + "\\\\" + filePath.Substring(i + 1, filePath.Length - 1 -i);
                        i++;
                    }
                }
                SQLQuery = "LOAD DATA LOCAL INFILE '" + filePath + "' INTO TABLE " + targetDB + "." + tableName +
                    " FIELDS TERMINATED BY ','" + "LINES TERMINATED BY '\n'" +
                    " (symbol, date, open, high,low, close, volume); ";

                command.CommandText = SQLQuery;
                reader = command.ExecuteReader();
                reader.Close();
            }
        }

        public static void emptyAllTables(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                DataSet tableList = getAllStockTables(Conn);

                Console.Write("Clearing all stock tables ...");
                Console.Out.Flush();
                
                MySqlCommand command = Conn.CreateCommand();
                for (int i = 0; i < tableList.Tables[0].Rows.Count; i++)
                {
                    String SQLQuery = "TRUNCATE `" + targetDB + "`.`" + tableList.Tables[0].Rows[i][0] + "`;";
                    command.CommandText = SQLQuery;
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                }
                Console.WriteLine("Done!");
            }
        }

        public static void emptyAllWeeklyTables(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                DataSet tableList = getAllWeeklyStockTables(Conn);

                Console.Write("Clearing all Weekly stock tables ...");
                Console.Out.Flush();

                MySqlCommand command = Conn.CreateCommand();
                for (int i = 0; i < tableList.Tables[0].Rows.Count; i++)
                {
                    String SQLQuery = "TRUNCATE `" + targetDB + "`.`" + tableList.Tables[0].Rows[i][0] + "`;";
                    command.CommandText = SQLQuery;
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                }
                Console.WriteLine("Done!");
            }
        }

        public static void dropAllTables(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                DataSet tableList = getAllStockTables(Conn);

                Console.Write("Deleting all stock tables ...");
                Console.Out.Flush();

                MySqlCommand command = Conn.CreateCommand();
                for (int i = 0; i < tableList.Tables[0].Rows.Count; i++)
                {
                    String SQLQuery = "DROP TABLE IF EXISTS " + targetDB + ".`" + tableList.Tables[0].Rows[i][0] + "` ;";
                    command.CommandText = SQLQuery;
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                }
                Console.WriteLine("Done!");
            }
        }

        public static void importSymbols(MySqlConnection Conn, String filePath)
        {
            if (Conn.State.ToString() == "Open")
            {
                Console.Write("Loading Symbols ...");
                Console.Out.Flush();
                //File Name syntax is important here
                int lastDash = filePath.LastIndexOf('_');
                int lastSlash = filePath.LastIndexOf('\\');
                String tableName = "`SYMBOLS_" + filePath.Substring(lastSlash + 1, lastDash - lastSlash - 1) + "`";
                String SQLQuery = "CREATE TABLE IF NOT EXISTS " + targetDB + "." + tableName + "(" +
                    "description  varchar(100)," +
                    "symbol    varchar(10)," +
                    "date    int," +
                    "open    decimal(8,4)," +
                    "high    decimal(8,4)," +
                    "low     decimal(8,4)," +
                    "close   decimal(8,4)," +
                    "volume  bigint," +
                    "index  (symbol)," +
                    "index (close),"+
                    "index (volume)"+
                    ");" +
                    "TRUNCATE " + targetDB + "." + tableName + ";";

                MySqlCommand command = Conn.CreateCommand();
                command.CommandText = SQLQuery;
                MySqlDataReader reader = command.ExecuteReader();
                reader.Close();

                for (int i = 0; i < filePath.Length; i++)
                {
                    if (filePath[i] == '\\')
                    {
                        filePath = filePath.Substring(0, i) + "\\\\" + filePath.Substring(i + 1, filePath.Length - 1 - i);
                        i++;
                    }
                }
                SQLQuery = "LOAD DATA LOCAL INFILE '" + filePath + "' INTO TABLE " + targetDB + "." + tableName +
                    " FIELDS TERMINATED BY ','" + "LINES TERMINATED BY '\n'" +
                    " (description, symbol, date, open, high,low, close, volume);";

                command.CommandText = SQLQuery;
                reader = command.ExecuteReader();
                reader.Close();
                Console.WriteLine("Done!");
            }
        }

        public static void importSingleDaysData(MySqlConnection Conn, String symbol, String date,
            String open, String high, String low, String close, String volume, String exchange)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    Console.Write("Updating " + symbol + "...");
                    Console.Out.Flush();
                    String tableName = "`STOCK_" + exchange + "_" + symbol + "`" ;
                                        
                    String SQLQuery = "CREATE TABLE IF NOT EXISTS " + targetDB + "." + tableName + "(" +
                        "symbol  varchar(10)," +
                        "date    int unique," +
                        "open    decimal(8,4)," +
                        "high    decimal(8,4)," +
                        "low     decimal(8,4)," +
                        "close   decimal(8,4)," +
                        "volume  bigint," +
                        "index (date)" +
                        ");";


                    SQLQuery += "INSERT INTO " + targetDB + "." + tableName + " VALUES(" +
                        "'" + symbol + "'," + date + "," + open + "," + high + "," + low + "," + close + "," + volume + ")"
                        + "ON DUPLICATE KEY UPDATE date=" + date + ";";

                    MySqlCommand command = Conn.CreateCommand();
                    command.CommandText = SQLQuery;
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Close();

                    Console.WriteLine("Updated " + symbol);
                    Console.Out.Flush();
                }
                catch (MySqlException error)
                {
                    Console.WriteLine("MySQL Error" + error.Message);
                }
            }
        }

        #endregion

        #region weekly converion
        public static void ConvertToWeeklyData(MySqlConnection Conn, String exchange)
        {
            //Get daily from current exchange and convert it to weekly data
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES " +
                    "WHERE table_schema = '" + targetDB + "' AND table_name LIKE 'STOCK_" + exchange + "%' AND table_name NOT LIKE '%WEEKLY%'";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);

                List<String> stockSymbols = Utilities.Convert_DataSet_to_StringList(output, 0);
               
                foreach (string stockTable in stockSymbols)
                {
                    String tableName = stockTable + "_weekly";
                    SQLQuery = "CREATE TABLE IF NOT EXISTS `" + targetDB + "`.`" + tableName + "`(" +
                    "symbol  varchar(10)," +
                    "date    int unique," +
                    "open    decimal(8,4)," +
                    "high    decimal(8,4)," +
                    "low     decimal(8,4)," +
                    "close   decimal(8,4)," +
                    "volume  bigint," +
                    "index (date)" +
                    "); TRUNCATE `" + targetDB + "`.`" + tableName + "`;";

                    MySqlCommand command = Conn.CreateCommand();
                    command.CommandText = SQLQuery;
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Close();

                    SQLQuery = "(SELECT symbol, date, open, high, low, close, volume FROM " +
                    targetDB + ".`" + stockTable + "` ORDER BY date DESC) ORDER BY date;";

                    DataSet stockData = new DataSet();  
                    adapter = new MySqlDataAdapter(SQLQuery, Conn);                                      
                    adapter.Fill(stockData);

                    SQLQuery = "SELECT symbol, date, open, high, low, close, volume FROM " +
                    "`" + targetDB + "`.`" + tableName + "`;";

                    DataSet stockDataWeekly = new DataSet();
                    adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    adapter.Fill(stockDataWeekly);
                    
                    stockDataWeekly.Tables[0].Rows.Clear();
                    
                    int distFromEndOfWeek = 10;
                    
                    String symbol = stockData.Tables[0].Rows[0][0].ToString();
                    DateTime currentDateTime = new DateTime();
                    String dateOfWeek = "";
                    uint localVolume = 0;
                    double localOpen = 0;
                    double localHigh = 0;
                    double localHighMax = 0;
                    double localLow = 0;
                    double localLowMin = 0;
                    double localClose = 0;

                    Console.Write("Updating weekly data for " + symbol + "...");
                    
                    StringBuilder mySQLStringBuilder = new StringBuilder();
                    mySQLStringBuilder.Append("INSERT INTO `"+targetDB + "`.`"+ tableName+"`(symbol, date, open, high, low, close, volume)" + "VALUES ");
 
                    for (int i = 0; i < stockData.Tables[0].Rows.Count; i++)
                    {
                        String currentDate = stockData.Tables[0].Rows[i][1].ToString();
                        currentDateTime = DateTime.Parse(currentDate.Substring(0, 4) + "-" + currentDate.Substring(4, 2) + "-" + currentDate.Substring(6, 2));

                        if (i == 0)
                        {
                            dateOfWeek = currentDate;                            
                            localOpen = Convert.ToDouble(stockData.Tables[0].Rows[i][2].ToString());
                            localHigh = Convert.ToDouble(stockData.Tables[0].Rows[i][3].ToString());
                            localHighMax = localHigh;
                            localLow = Convert.ToDouble(stockData.Tables[0].Rows[i][4].ToString());
                            localLowMin = localLow;
                            localVolume = Convert.ToUInt32(stockData.Tables[0].Rows[i][6].ToString());
                            distFromEndOfWeek = Utilities.getDayOfWeekDifference(currentDateTime.DayOfWeek, DayOfWeek.Saturday);
                        }                        

                        //We've entered a new week!
                        if(distFromEndOfWeek <=Utilities.getDayOfWeekDifference(currentDateTime.DayOfWeek, DayOfWeek.Saturday) && i > 0 )
                        {                            
                            String date = dateOfWeek.ToString();
                            String open = localOpen.ToString();
                            String high = localHighMax.ToString();
                            String low = localLowMin.ToString();
                            String close = localClose.ToString();
                            String volume = localVolume.ToString();

                            mySQLStringBuilder.AppendFormat("('{0}',{1},{2},{3},{4},{5},{6}),",symbol, date, open, high, low, close, volume);
                            //stockDataWeekly.Tables[0].LoadDataRow(new object[] {symbol, date, open, high, low, close, volume}, true);

                            dateOfWeek = currentDate;
                            localOpen = Convert.ToDouble(stockData.Tables[0].Rows[i][2].ToString());                            
                            distFromEndOfWeek = Utilities.getDayOfWeekDifference(currentDateTime.DayOfWeek, DayOfWeek.Saturday);
                            localVolume = 0;
                            localHighMax = double.MinValue;
                            localLowMin = double.MaxValue;
                        }
                        //We're at the end of existing data! wrap up the week
                        else if (i == stockData.Tables[0].Rows.Count - 1)
                        {
                            localHigh = Convert.ToDouble(stockData.Tables[0].Rows[i][3].ToString());
                            localLow = Convert.ToDouble(stockData.Tables[0].Rows[i][4].ToString());
                            localVolume += Convert.ToUInt32(stockData.Tables[0].Rows[i][6].ToString());
                            localHighMax = Math.Max(localHigh, localHighMax);
                            localLowMin = Math.Min(localLow, localLowMin);
                            localClose = Convert.ToDouble(stockData.Tables[0].Rows[i][5].ToString());

                            String date = dateOfWeek.ToString();
                            String open = localOpen.ToString();
                            String high = localHighMax.ToString();
                            String low = localLowMin.ToString();
                            String close = localClose.ToString();
                            String volume = localVolume.ToString();

                            mySQLStringBuilder.AppendFormat("('{0}',{1},{2},{3},{4},{5},{6}),", symbol, date, open, high, low, close, volume);
                            //stockDataWeekly.Tables[0].LoadDataRow(new object[] {symbol, date, open, high, low, close, volume}, true);
                        }

                        localHigh = Convert.ToDouble(stockData.Tables[0].Rows[i][3].ToString());
                        localLow = Convert.ToDouble(stockData.Tables[0].Rows[i][4].ToString());                        
                        localVolume += Convert.ToUInt32(stockData.Tables[0].Rows[i][6].ToString());
                        localHighMax = Math.Max(localHigh,localHighMax);
                        localLowMin = Math.Min(localLow,localLowMin);
                        localClose = Convert.ToDouble(stockData.Tables[0].Rows[i][5].ToString());
                    }

                    mySQLStringBuilder.Remove(mySQLStringBuilder.Length - 1, 1);
                    mySQLStringBuilder.Append(";");

                    command = Conn.CreateCommand();
                    command.CommandText = mySQLStringBuilder.ToString();;
                    reader = command.ExecuteReader();
                    reader.Close();
                    Console.WriteLine("Done!");                    
                }

                
                /*
                for (int i = 0; i < filePath.Length; i++)
                {
                    if (filePath[i] == '\\')
                    {
                        filePath = filePath.Substring(0, i) + "\\\\" + filePath.Substring(i + 1, filePath.Length - 1 - i);
                        i++;
                    }
                }
                SQLQuery = "LOAD DATA LOCAL INFILE '" + filePath + "' INTO TABLE " + targetDB + "." + tableName +
                    " FIELDS TERMINATED BY ','" + "LINES TERMINATED BY '\n'" +
                    " (symbol, date, open, high,low, close, volume); ";

                command.CommandText = SQLQuery;
                reader = command.ExecuteReader();
                reader.Close();
                 **/
            }
        }
        #endregion

        #region Analyzer
        public static DataSet getAllStockTables(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES " +
                    "WHERE table_schema = '"+ targetDB +"' AND table_name LIKE 'STOCK%'";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        public static DataSet getAllWeeklyStockTables(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES " +
                    "WHERE table_schema = '" + targetDB + "' AND table_name LIKE 'STOCK%' AND table_name LIKE '%WEEKLY%'";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        #region Errorhandling
        public static DataSet getSpecificStockTables(MySqlConnection Conn, String targetExchange)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES " +
                    "WHERE table_schema = '" + targetDB + "' AND table_name LIKE 'STOCK_"+ targetExchange +"%'";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        public static void deleteDateFromStockTables(MySqlConnection Conn, String targetTable, String targetDate)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "DELETE FROM `" + targetDB + "`.`"+targetTable+"` WHERE date = " + targetDate+";";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
            }
        }

        public static DataSet getSingleStockDates(MySqlConnection Conn, String symbol, String dateLimit, String exchange, String offset)
        {
            if (Conn.State.ToString() == "Open")
            {
                Console.Write("Extracting " + symbol + "...");
                String SQLQuery = "SELECT date FROM (SELECT symbol, date, open, high, low, close, volume FROM " +
                    targetDB + "." + "`STOCK_" + "NASDAQ" + "_" + symbol + "` ORDER BY date DESC LIMIT " + dateLimit +
                    ") AS `temp` ORDER BY date LIMIT " + offset + ";";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                Console.WriteLine("Done!");
                return output;
            }
            return null;
        }
        #endregion

        public static DataSet getExchangeData(MySqlConnection Conn, String exchange, List<String> filters)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT symbol, close, volume, description from " + targetDB + "." 
                    + "`symbols_" + exchange + "`";
                if (filters.Count > 0)
                {
                    String refinedSearch = " WHERE ";
                    foreach (String clause in filters)
                    {
                        refinedSearch = refinedSearch + clause + " AND ";
                    }
                    refinedSearch = refinedSearch.Remove(refinedSearch.Length - 4);
                    SQLQuery += refinedSearch;
                }
                SQLQuery += ";";

                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        public static DataSet getSingleStockData(MySqlConnection Conn, String symbol, String dateLimit, String exchange, String offset, String startDate, String endDate)
        {
            if (Conn.State.ToString() == "Open")
            {
                Console.Write("Extracting " + symbol + "...");
                String SQLQuery = "SELECT * FROM (SELECT symbol, date, open, high, low, close, volume FROM " +
                    targetDB + "." + "`STOCK_" + exchange + "_" + symbol +
                    "` WHERE date > " + startDate + " AND " + " date < " + endDate +                     
                    " ORDER BY date DESC LIMIT " + dateLimit +
                    ") AS `temp` ORDER BY date LIMIT "+ offset +";";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                try
                {
                    adapter.Fill(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR, Stock did not exist");
                    output = null;
                }
                Console.WriteLine("Done!");
                return output;
            }
            return null;
        }

        public static DataSet getSingleWeeklyStockData(MySqlConnection Conn, String symbol, String dateLimit, String exchange, String offset)
        {
            if (Conn.State.ToString() == "Open")
            {
                Console.Write("Extracting " + symbol + "...");
                String SQLQuery = "SELECT * FROM (SELECT symbol, date, open, high, low, close, volume FROM " +
                    targetDB + "." + "`STOCK_" + exchange + "_" + symbol + "_WEEKLY` ORDER BY date DESC LIMIT " + dateLimit +
                    ") AS `temp` ORDER BY date LIMIT " + offset + ";";
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                Console.WriteLine("Done!");
                return output;
            }
            return null;
        }

        public static DataSet getStockSymbolsByName(MySqlConnection Conn, String exchange, List<String> symbols)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT symbol, close, volume, description from " + targetDB + "." 
                    + "`symbols_" + exchange + "` WHERE symbol IN (";
                for (int i = 0; i < symbols.Count -1; i++)
                {
                    SQLQuery += "'" + symbols[i] + "',";
                }
                
            
                SQLQuery += "'"+symbols[symbols.Count -1]+"');";

                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        //Save a favorite stock group, and add the stocks to favorite stock list. Return the group id
        public static DataSet saveFavoriteStocks(MySqlConnection Conn, List<String> symbols, String label, String description)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {         
                    setupFavorites(Conn);
                    Console.Write("Saving Favorites");
                    Console.Out.Flush();                    

                    String SQLQuery = "INSERT INTO " + targetDB + ".`FAVORITE_STOCKGROUPS` (name, description) " +
                        " VALUES(" + "'" + label + "','" + description + "');";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);
                    
                    Console.Write("Stock Saved! Loading new list of favorites!");
                    Console.Out.Flush();

                    SQLQuery = "SELECT group_id FROM " + targetDB + ".`FAVORITE_STOCKGROUPS`" + 
                        " ORDER BY group_id DESC LIMIT 1 ";

                    adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet gid = new DataSet();
                    adapter.Fill(gid);
                    int group_id = Convert.ToInt32(gid.Tables[0].Rows[0][0].ToString());

                    String Values =  "VALUES (" + group_id + ",'" + symbols[0] + "')";
                    for (int i = 1; i < symbols.Count; i++)
                    {
                        Values += ",(" + group_id + ",'" + symbols[i] + "')";
                    }
                    Values += ";";

                    SQLQuery = "INSERT INTO " + targetDB + ".`FAVORITE_STOCKS` " + Values;

                    adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    output = new DataSet();
                    adapter.Fill(output);
                    return output;
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            
            return null;
        }

        //Load favorite stock groups
        public static DataSet loadFavoriteStockSets(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    setupFavorites(Conn);
                    Console.Write("Loading Favorites");
                    Console.Out.Flush();
                    String tableName = "`FAVORITE_STOCKGROUPS`";

                    String SQLQuery = "SELECT * FROM " + targetDB + "." + tableName + ";";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    return output;
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            return null;
        }

        //Delete all favorite tables from favorites
        public static DataSet dropFavorites(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    String SQLQuery = "DROP TABLE `" + targetDB + "`.`FAVORITE_STOCKGROUPS`; " +
                        "DROP TABLE `" + targetDB + "`.`FAVORITE_STOCKS`;";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    return output;
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            return null;
        }

        //Load selected favorite stock into target stock window of favorites 
        public static DataSet loadSelectedFavoriteStocks(MySqlConnection Conn, String Name)
        {
            if (Conn.State.ToString() == "Open")
            {
                String SQLQuery = "SELECT group_id FROM " + targetDB + ".`FAVORITE_STOCKGROUPS`"+
                    " WHERE name = '" + Name + "';";

                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet gid = new DataSet();
                adapter.Fill(gid);

                int group_id = Convert.ToInt32(gid.Tables[0].Rows[0][0].ToString());

                SQLQuery = "SELECT * FROM " + targetDB + ".`FAVORITE_STOCKS`" +
                    " WHERE group_id = " + group_id + ";";

                adapter = new MySqlDataAdapter(SQLQuery, Conn);
                DataSet output = new DataSet();
                adapter.Fill(output);
                return output;
            }
            return null;
        }

        //Setup favorite stock groups, and favorite stocks tables
        public static void setupFavorites(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {                    
                    String SQLQuery = 
                        "CREATE TABLE IF NOT EXISTS " + targetDB + ".`FAVORITE_STOCKGROUPS`(" +
                        "group_id   int null auto_increment, " +
                        "name    varchar(255) UNIQUE, " +
                        "description    varchar(255), " +
                        "PRIMARY KEY (group_id)" +
                        "); ";
                    SQLQuery +=
                        "CREATE TABLE IF NOT EXISTS " + targetDB + ".`FAVORITE_STOCKS`(" +
                        "group_id   int null auto_increment, " +
                        "stock    varchar(255), " +
                        "FOREIGN KEY (group_id) REFERENCES `FAVORITE_STOCKGROUPS`(group_id) " +
                        "ON DELETE CASCADE"+
                        "); ";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);   

                    Console.WriteLine("Favorites table ready");
                    Console.Out.Flush();
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        //Create Evaluations Tables
        public static void setupEvaluations(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    String SQLQuery =
                      "CREATE TABLE IF NOT EXISTS " + targetDB + ".`EVALUATIONS` (" +
                      "id int AUTO_INCREMENT," +
                      "name varchar(255)," +
                      "PRIMARY KEY (id)" +
                      ");";

                    SQLQuery +=
                        "CREATE TABLE if not EXISTS " + targetDB + ".`EVALUATIONS_DATA`(" +
                        "id int," +
                        "x int," +
                        "y decimal(8,4)," +
                        "FOREIGN KEY (id) REFERENCES evaluations(id)" +
                        ");";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    Console.WriteLine("Evaluations table ready");
                    Console.Out.Flush();
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        //Clear evaluations
        public static void clearEvaluations(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    String SQLQuery = "TRUNCATE TABLE  " + targetDB + ".`EVALUATIONS_DATA`; " ;
                    SQLQuery += "TRUNCATE TABLE  " + targetDB + ".`EVALUATIONS`;";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    Console.WriteLine("Evaluations table cleared");
                    Console.Out.Flush();
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        //Insert Record into Evaluations
        public static void InsertEvaluations(MySqlConnection Conn, String Name, List<Double> Data)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    String SQLQuery = "INSERT INTO " + targetDB + ".`EVALUATIONS` (id, name) " +
                        " VALUES( NULL" + ",'" + Name + "'); SELECT LAST_INSERT_ID();";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    int id = (int)adapter.SelectCommand.LastInsertedId;
                    
                    SQLQuery = "INSERT INTO " + targetDB + ".`EVALUATIONS_DATA` (id, x, y) " +
                       " VALUES";

                    if (Data.Count > 0)
                        SQLQuery += "( " + id.ToString() + ", 0 ," + Data[0].ToString() + ")";

                    for (int i = 1; i < Data.Count; i++)
                    {
                        SQLQuery += ", ( " + id.ToString() + ", " + i.ToString() + "," + Data[i].ToString() + ")";
                    }

                    adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    output = new DataSet();
                    adapter.Fill(output);
                       

                    Console.WriteLine("Evaluations table cleared");
                    Console.Out.Flush();
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        //Get Evaluations
        public static DataSet GetEvaluations(MySqlConnection Conn, int id)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    String SQLQuery = "SELECT * FROM " + targetDB + ".`evaluations_data` WHERE id = "+ id +" ;";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    return output;
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            return null;
        }

        //Remove Evaluations
        public static DataSet RemoveEvaluations(MySqlConnection Conn, int id)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    String SQLQuery = "DELETE FROM " + targetDB + ".`evaluations` WHERE id = " + id + " ; ";
                    SQLQuery += "DELETE FROM " + targetDB + ".`evaluations_data` WHERE id = " + id + " ;";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    return output;
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            return null;
        }

        //Load Evaluations
        public static DataSet loadSavedPortfolios(MySqlConnection Conn)
        {
            if (Conn.State.ToString() == "Open")
            {
                try
                {
                    setupFavorites(Conn);
                    Console.Write("Loading Saved Portfolios");
                    Console.Out.Flush();
                    String tableName = "`EVALUATIONS`";

                    String SQLQuery = "SELECT * FROM " + targetDB + "." + tableName + ";";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, Conn);
                    DataSet output = new DataSet();
                    adapter.Fill(output);

                    return output;
                }
                catch (MySqlException error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            return null;
        }


        #endregion
    }
}
