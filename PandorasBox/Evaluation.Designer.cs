namespace PandorasBox
{
    partial class Evaluation
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
            this.label4 = new System.Windows.Forms.Label();
            this.lBox_Portfolios = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_ClearPortfolio = new System.Windows.Forms.Button();
            this.btn_RecordPortfolio = new System.Windows.Forms.Button();
            this.txt_PortfolioName = new System.Windows.Forms.TextBox();
            this.btn_EvaluateSelectedStocks = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(357, -11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Saved Stocks";
            // 
            // lBox_Portfolios
            // 
            this.lBox_Portfolios.FormattingEnabled = true;
            this.lBox_Portfolios.Location = new System.Drawing.Point(200, 9);
            this.lBox_Portfolios.Name = "lBox_Portfolios";
            this.lBox_Portfolios.Size = new System.Drawing.Size(160, 160);
            this.lBox_Portfolios.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(526, -11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Target Stocks";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Name";
            // 
            // btn_ClearPortfolio
            // 
            this.btn_ClearPortfolio.Location = new System.Drawing.Point(63, 93);
            this.btn_ClearPortfolio.Name = "btn_ClearPortfolio";
            this.btn_ClearPortfolio.Size = new System.Drawing.Size(120, 23);
            this.btn_ClearPortfolio.TabIndex = 33;
            this.btn_ClearPortfolio.Text = "Clear Portfolio";
            this.btn_ClearPortfolio.UseVisualStyleBackColor = true;
            this.btn_ClearPortfolio.Click += new System.EventHandler(this.btn_ClearPortfolio_Click);
            // 
            // btn_RecordPortfolio
            // 
            this.btn_RecordPortfolio.Location = new System.Drawing.Point(63, 64);
            this.btn_RecordPortfolio.Name = "btn_RecordPortfolio";
            this.btn_RecordPortfolio.Size = new System.Drawing.Size(120, 23);
            this.btn_RecordPortfolio.TabIndex = 32;
            this.btn_RecordPortfolio.Text = "Record Portfolio";
            this.btn_RecordPortfolio.UseVisualStyleBackColor = true;
            this.btn_RecordPortfolio.Click += new System.EventHandler(this.btn_RecordPortfolio_Click);
            // 
            // txt_PortfolioName
            // 
            this.txt_PortfolioName.Location = new System.Drawing.Point(63, 9);
            this.txt_PortfolioName.Name = "txt_PortfolioName";
            this.txt_PortfolioName.Size = new System.Drawing.Size(119, 20);
            this.txt_PortfolioName.TabIndex = 35;
            // 
            // btn_EvaluateSelectedStocks
            // 
            this.btn_EvaluateSelectedStocks.Location = new System.Drawing.Point(63, 35);
            this.btn_EvaluateSelectedStocks.Name = "btn_EvaluateSelectedStocks";
            this.btn_EvaluateSelectedStocks.Size = new System.Drawing.Size(120, 23);
            this.btn_EvaluateSelectedStocks.TabIndex = 34;
            this.btn_EvaluateSelectedStocks.Text = "Evaluate Selection";
            this.btn_EvaluateSelectedStocks.UseVisualStyleBackColor = true;
            this.btn_EvaluateSelectedStocks.Click += new System.EventHandler(this.btn_EvaluateSelectedStocks_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(63, 122);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(119, 23);
            this.btn_Cancel.TabIndex = 36;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // Evaluation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 177);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.txt_PortfolioName);
            this.Controls.Add(this.btn_EvaluateSelectedStocks);
            this.Controls.Add(this.btn_ClearPortfolio);
            this.Controls.Add(this.btn_RecordPortfolio);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lBox_Portfolios);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "Evaluation";
            this.Text = "Evaluation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lBox_Portfolios;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_ClearPortfolio;
        private System.Windows.Forms.Button btn_RecordPortfolio;
        private System.Windows.Forms.TextBox txt_PortfolioName;
        private System.Windows.Forms.Button btn_EvaluateSelectedStocks;
        private System.Windows.Forms.Button btn_Cancel;
    }
}