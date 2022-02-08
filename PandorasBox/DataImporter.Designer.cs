namespace PandorasBox
{
    partial class DataImporter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_ArchiveFolderPath = new System.Windows.Forms.TextBox();
            this.btn_BrowseCSVArchive = new System.Windows.Forms.Button();
            this.btn_ImportFiles = new System.Windows.Forms.Button();
            this.btn_ClearStocks = new System.Windows.Forms.Button();
            this.btn_DropStocks = new System.Windows.Forms.Button();
            this.txt_SymbolsFilePath = new System.Windows.Forms.TextBox();
            this.btn_BrowseSymbols = new System.Windows.Forms.Button();
            this.btn_LoadSymbols = new System.Windows.Forms.Button();
            this.btn_SingleStockAnalyzer = new System.Windows.Forms.Button();
            this.btn_LoadSingleDay = new System.Windows.Forms.Button();
            this.btn_BrowseDailyData = new System.Windows.Forms.Button();
            this.txt_SingleDayDataFilePath = new System.Windows.Forms.TextBox();
            this.cmbBox_Exchange = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_CreateWeeklyData = new System.Windows.Forms.Button();
            this.btn_ClearWeeklyStocks = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_ArchiveFolderPath
            // 
            this.txt_ArchiveFolderPath.Enabled = false;
            this.txt_ArchiveFolderPath.Location = new System.Drawing.Point(135, 28);
            this.txt_ArchiveFolderPath.Name = "txt_ArchiveFolderPath";
            this.txt_ArchiveFolderPath.Size = new System.Drawing.Size(257, 20);
            this.txt_ArchiveFolderPath.TabIndex = 0;
            // 
            // btn_BrowseCSVArchive
            // 
            this.btn_BrowseCSVArchive.Location = new System.Drawing.Point(12, 28);
            this.btn_BrowseCSVArchive.Name = "btn_BrowseCSVArchive";
            this.btn_BrowseCSVArchive.Size = new System.Drawing.Size(117, 23);
            this.btn_BrowseCSVArchive.TabIndex = 1;
            this.btn_BrowseCSVArchive.Text = "Browse Archive";
            this.btn_BrowseCSVArchive.UseVisualStyleBackColor = true;
            this.btn_BrowseCSVArchive.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // btn_ImportFiles
            // 
            this.btn_ImportFiles.Location = new System.Drawing.Point(398, 28);
            this.btn_ImportFiles.Name = "btn_ImportFiles";
            this.btn_ImportFiles.Size = new System.Drawing.Size(117, 23);
            this.btn_ImportFiles.TabIndex = 2;
            this.btn_ImportFiles.Text = "Import CSV Files";
            this.btn_ImportFiles.UseVisualStyleBackColor = true;
            this.btn_ImportFiles.Click += new System.EventHandler(this.btn_ImportFiles_Click);
            // 
            // btn_ClearStocks
            // 
            this.btn_ClearStocks.Location = new System.Drawing.Point(12, 199);
            this.btn_ClearStocks.Name = "btn_ClearStocks";
            this.btn_ClearStocks.Size = new System.Drawing.Size(117, 23);
            this.btn_ClearStocks.TabIndex = 3;
            this.btn_ClearStocks.Text = "Clear All Stock Data";
            this.btn_ClearStocks.UseVisualStyleBackColor = true;
            this.btn_ClearStocks.Click += new System.EventHandler(this.btn_ClearStocks_Click);
            // 
            // btn_DropStocks
            // 
            this.btn_DropStocks.Location = new System.Drawing.Point(12, 229);
            this.btn_DropStocks.Name = "btn_DropStocks";
            this.btn_DropStocks.Size = new System.Drawing.Size(117, 23);
            this.btn_DropStocks.TabIndex = 4;
            this.btn_DropStocks.Text = "Drop All Stocks";
            this.btn_DropStocks.UseVisualStyleBackColor = true;
            this.btn_DropStocks.Click += new System.EventHandler(this.btn_DropStocks_Click);
            // 
            // txt_SymbolsFilePath
            // 
            this.txt_SymbolsFilePath.Enabled = false;
            this.txt_SymbolsFilePath.Location = new System.Drawing.Point(135, 57);
            this.txt_SymbolsFilePath.Name = "txt_SymbolsFilePath";
            this.txt_SymbolsFilePath.Size = new System.Drawing.Size(257, 20);
            this.txt_SymbolsFilePath.TabIndex = 5;
            // 
            // btn_BrowseSymbols
            // 
            this.btn_BrowseSymbols.Location = new System.Drawing.Point(12, 57);
            this.btn_BrowseSymbols.Name = "btn_BrowseSymbols";
            this.btn_BrowseSymbols.Size = new System.Drawing.Size(117, 23);
            this.btn_BrowseSymbols.TabIndex = 6;
            this.btn_BrowseSymbols.Text = "Browse Symbols";
            this.btn_BrowseSymbols.UseVisualStyleBackColor = true;
            this.btn_BrowseSymbols.Click += new System.EventHandler(this.btn_BrowseSymbols_Click);
            // 
            // btn_LoadSymbols
            // 
            this.btn_LoadSymbols.Location = new System.Drawing.Point(398, 57);
            this.btn_LoadSymbols.Name = "btn_LoadSymbols";
            this.btn_LoadSymbols.Size = new System.Drawing.Size(117, 23);
            this.btn_LoadSymbols.TabIndex = 7;
            this.btn_LoadSymbols.Text = "Load Symbol Info";
            this.btn_LoadSymbols.UseVisualStyleBackColor = true;
            this.btn_LoadSymbols.Click += new System.EventHandler(this.btn_LoadSymbols_Click);
            // 
            // btn_SingleStockAnalyzer
            // 
            this.btn_SingleStockAnalyzer.Location = new System.Drawing.Point(398, 229);
            this.btn_SingleStockAnalyzer.Name = "btn_SingleStockAnalyzer";
            this.btn_SingleStockAnalyzer.Size = new System.Drawing.Size(117, 23);
            this.btn_SingleStockAnalyzer.TabIndex = 8;
            this.btn_SingleStockAnalyzer.Text = "Stock Analyzer";
            this.btn_SingleStockAnalyzer.UseVisualStyleBackColor = true;
            this.btn_SingleStockAnalyzer.Click += new System.EventHandler(this.btn_SingleStockAnalyzer_Click);
            // 
            // btn_LoadSingleDay
            // 
            this.btn_LoadSingleDay.Location = new System.Drawing.Point(398, 86);
            this.btn_LoadSingleDay.Name = "btn_LoadSingleDay";
            this.btn_LoadSingleDay.Size = new System.Drawing.Size(117, 23);
            this.btn_LoadSingleDay.TabIndex = 11;
            this.btn_LoadSingleDay.Text = "Load Single Day";
            this.btn_LoadSingleDay.UseVisualStyleBackColor = true;
            this.btn_LoadSingleDay.Click += new System.EventHandler(this.btn_LoadSingleDay_Click);
            // 
            // btn_BrowseDailyData
            // 
            this.btn_BrowseDailyData.Location = new System.Drawing.Point(12, 86);
            this.btn_BrowseDailyData.Name = "btn_BrowseDailyData";
            this.btn_BrowseDailyData.Size = new System.Drawing.Size(117, 23);
            this.btn_BrowseDailyData.TabIndex = 10;
            this.btn_BrowseDailyData.Text = "Browse Daily Data";
            this.btn_BrowseDailyData.UseVisualStyleBackColor = true;
            this.btn_BrowseDailyData.Click += new System.EventHandler(this.btn_BrowseDailyData_Click);
            // 
            // txt_SingleDayDataFilePath
            // 
            this.txt_SingleDayDataFilePath.Enabled = false;
            this.txt_SingleDayDataFilePath.Location = new System.Drawing.Point(135, 86);
            this.txt_SingleDayDataFilePath.Name = "txt_SingleDayDataFilePath";
            this.txt_SingleDayDataFilePath.Size = new System.Drawing.Size(257, 20);
            this.txt_SingleDayDataFilePath.TabIndex = 9;
            // 
            // cmbBox_Exchange
            // 
            this.cmbBox_Exchange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBox_Exchange.FormattingEnabled = true;
            this.cmbBox_Exchange.Location = new System.Drawing.Point(271, 112);
            this.cmbBox_Exchange.Name = "cmbBox_Exchange";
            this.cmbBox_Exchange.Size = new System.Drawing.Size(121, 21);
            this.cmbBox_Exchange.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(135, 119);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Target Exchange";
            // 
            // btn_CreateWeeklyData
            // 
            this.btn_CreateWeeklyData.Location = new System.Drawing.Point(135, 199);
            this.btn_CreateWeeklyData.Name = "btn_CreateWeeklyData";
            this.btn_CreateWeeklyData.Size = new System.Drawing.Size(117, 23);
            this.btn_CreateWeeklyData.TabIndex = 14;
            this.btn_CreateWeeklyData.Text = "Create Weekly Data";
            this.btn_CreateWeeklyData.UseVisualStyleBackColor = true;
            this.btn_CreateWeeklyData.Click += new System.EventHandler(this.btn_CreateWeeklyData_Click);
            // 
            // btn_ClearWeeklyStocks
            // 
            this.btn_ClearWeeklyStocks.Location = new System.Drawing.Point(135, 229);
            this.btn_ClearWeeklyStocks.Name = "btn_ClearWeeklyStocks";
            this.btn_ClearWeeklyStocks.Size = new System.Drawing.Size(117, 23);
            this.btn_ClearWeeklyStocks.TabIndex = 15;
            this.btn_ClearWeeklyStocks.Text = "Clear Weekly Data";
            this.btn_ClearWeeklyStocks.UseVisualStyleBackColor = true;
            this.btn_ClearWeeklyStocks.Click += new System.EventHandler(this.btn_ClearWeeklyStocks_Click);
            // 
            // DataImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 264);
            this.Controls.Add(this.btn_ClearWeeklyStocks);
            this.Controls.Add(this.btn_CreateWeeklyData);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbBox_Exchange);
            this.Controls.Add(this.btn_LoadSingleDay);
            this.Controls.Add(this.btn_BrowseDailyData);
            this.Controls.Add(this.txt_SingleDayDataFilePath);
            this.Controls.Add(this.btn_SingleStockAnalyzer);
            this.Controls.Add(this.btn_LoadSymbols);
            this.Controls.Add(this.btn_BrowseSymbols);
            this.Controls.Add(this.txt_SymbolsFilePath);
            this.Controls.Add(this.btn_DropStocks);
            this.Controls.Add(this.btn_ClearStocks);
            this.Controls.Add(this.btn_ImportFiles);
            this.Controls.Add(this.btn_BrowseCSVArchive);
            this.Controls.Add(this.txt_ArchiveFolderPath);
            this.Name = "DataImporter";
            this.Text = "Data Importer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_ArchiveFolderPath;
        private System.Windows.Forms.Button btn_BrowseCSVArchive;
        private System.Windows.Forms.Button btn_ImportFiles;
        private System.Windows.Forms.Button btn_ClearStocks;
        private System.Windows.Forms.Button btn_DropStocks;
        private System.Windows.Forms.TextBox txt_SymbolsFilePath;
        private System.Windows.Forms.Button btn_BrowseSymbols;
        private System.Windows.Forms.Button btn_LoadSymbols;
        private System.Windows.Forms.Button btn_SingleStockAnalyzer;
        private System.Windows.Forms.Button btn_LoadSingleDay;
        private System.Windows.Forms.Button btn_BrowseDailyData;
        private System.Windows.Forms.TextBox txt_SingleDayDataFilePath;
        private System.Windows.Forms.ComboBox cmbBox_Exchange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_CreateWeeklyData;
        private System.Windows.Forms.Button btn_ClearWeeklyStocks;
    }
}

