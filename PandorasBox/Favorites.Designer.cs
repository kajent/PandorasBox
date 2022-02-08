namespace PandorasBox
{
    partial class Favorites
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
            this.lBox_TargetStocks = new System.Windows.Forms.ListBox();
            this.txt_StockSetName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_StockSetDescription = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_SaveStocks = new System.Windows.Forms.Button();
            this.lBox_FavoriteStocks = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_LoadStocks = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_UseTargetStocks = new System.Windows.Forms.Button();
            this.btn_DropFavorites = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lBox_TargetStocks
            // 
            this.lBox_TargetStocks.FormattingEnabled = true;
            this.lBox_TargetStocks.Location = new System.Drawing.Point(526, 21);
            this.lBox_TargetStocks.Name = "lBox_TargetStocks";
            this.lBox_TargetStocks.Size = new System.Drawing.Size(160, 160);
            this.lBox_TargetStocks.TabIndex = 0;
            // 
            // txt_StockSetName
            // 
            this.txt_StockSetName.Location = new System.Drawing.Point(73, 21);
            this.txt_StockSetName.Name = "txt_StockSetName";
            this.txt_StockSetName.Size = new System.Drawing.Size(260, 20);
            this.txt_StockSetName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name";
            // 
            // txt_StockSetDescription
            // 
            this.txt_StockSetDescription.Location = new System.Drawing.Point(73, 47);
            this.txt_StockSetDescription.Name = "txt_StockSetDescription";
            this.txt_StockSetDescription.Size = new System.Drawing.Size(260, 20);
            this.txt_StockSetDescription.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Description";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(526, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Target Stocks";
            // 
            // btn_SaveStocks
            // 
            this.btn_SaveStocks.Location = new System.Drawing.Point(73, 74);
            this.btn_SaveStocks.Name = "btn_SaveStocks";
            this.btn_SaveStocks.Size = new System.Drawing.Size(119, 23);
            this.btn_SaveStocks.TabIndex = 6;
            this.btn_SaveStocks.Text = "Save";
            this.btn_SaveStocks.UseVisualStyleBackColor = true;
            this.btn_SaveStocks.Click += new System.EventHandler(this.btn_SaveStocks_Click);
            // 
            // lBox_FavoriteStocks
            // 
            this.lBox_FavoriteStocks.FormattingEnabled = true;
            this.lBox_FavoriteStocks.Location = new System.Drawing.Point(360, 21);
            this.lBox_FavoriteStocks.Name = "lBox_FavoriteStocks";
            this.lBox_FavoriteStocks.Size = new System.Drawing.Size(160, 160);
            this.lBox_FavoriteStocks.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(357, 1);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Saved Stocks";
            // 
            // btn_LoadStocks
            // 
            this.btn_LoadStocks.Location = new System.Drawing.Point(214, 74);
            this.btn_LoadStocks.Name = "btn_LoadStocks";
            this.btn_LoadStocks.Size = new System.Drawing.Size(119, 23);
            this.btn_LoadStocks.TabIndex = 9;
            this.btn_LoadStocks.Text = "Load";
            this.btn_LoadStocks.UseVisualStyleBackColor = true;
            this.btn_LoadStocks.Click += new System.EventHandler(this.btn_LoadStocks_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(214, 158);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(119, 23);
            this.btn_Cancel.TabIndex = 10;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_UseTargetStocks
            // 
            this.btn_UseTargetStocks.Location = new System.Drawing.Point(73, 157);
            this.btn_UseTargetStocks.Name = "btn_UseTargetStocks";
            this.btn_UseTargetStocks.Size = new System.Drawing.Size(119, 23);
            this.btn_UseTargetStocks.TabIndex = 11;
            this.btn_UseTargetStocks.Text = "Use Target Stocks";
            this.btn_UseTargetStocks.UseVisualStyleBackColor = true;
            this.btn_UseTargetStocks.Click += new System.EventHandler(this.btn_UseTargetStocks_Click);
            // 
            // btn_DropFavorites
            // 
            this.btn_DropFavorites.Location = new System.Drawing.Point(214, 129);
            this.btn_DropFavorites.Name = "btn_DropFavorites";
            this.btn_DropFavorites.Size = new System.Drawing.Size(119, 23);
            this.btn_DropFavorites.TabIndex = 12;
            this.btn_DropFavorites.Text = "Clear Favorites";
            this.btn_DropFavorites.UseVisualStyleBackColor = true;
            this.btn_DropFavorites.Click += new System.EventHandler(this.btn_DropFavorites_Click);
            // 
            // Favorites
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 192);
            this.Controls.Add(this.btn_DropFavorites);
            this.Controls.Add(this.btn_UseTargetStocks);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_LoadStocks);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lBox_FavoriteStocks);
            this.Controls.Add(this.btn_SaveStocks);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_StockSetDescription);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_StockSetName);
            this.Controls.Add(this.lBox_TargetStocks);
            this.Name = "Favorites";
            this.Text = "Favorites";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lBox_TargetStocks;
        private System.Windows.Forms.TextBox txt_StockSetName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_StockSetDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_SaveStocks;
        private System.Windows.Forms.ListBox lBox_FavoriteStocks;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_LoadStocks;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_UseTargetStocks;
        private System.Windows.Forms.Button btn_DropFavorites;
    }
}