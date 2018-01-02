namespace SoundScheduler_Win {
    partial class frmGenetic {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.cmdGo = new System.Windows.Forms.Button();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.cmdStop = new System.Windows.Forms.Button();
            this.txtPopulation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMutationRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtThreadCount = new System.Windows.Forms.TextBox();
            this.lblProcessorCount = new System.Windows.Forms.Label();
            this.cmdAnalyze = new System.Windows.Forms.Button();
            this.txtPrintSolution = new System.Windows.Forms.TextBox();
            this.cmdPrintSolution = new System.Windows.Forms.Button();
            this.txtStartingSolution = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdGo
            // 
            this.cmdGo.Location = new System.Drawing.Point(13, 146);
            this.cmdGo.Name = "cmdGo";
            this.cmdGo.Size = new System.Drawing.Size(75, 23);
            this.cmdGo.TabIndex = 0;
            this.cmdGo.Text = "Go";
            this.cmdGo.UseVisualStyleBackColor = true;
            this.cmdGo.Click += new System.EventHandler(this.cmdGo_Click);
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(13, 175);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResults.Size = new System.Drawing.Size(768, 386);
            this.txtResults.TabIndex = 1;
            // 
            // cmdStop
            // 
            this.cmdStop.Location = new System.Drawing.Point(94, 146);
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.Size = new System.Drawing.Size(75, 23);
            this.cmdStop.TabIndex = 2;
            this.cmdStop.Text = "Stop";
            this.cmdStop.UseVisualStyleBackColor = true;
            this.cmdStop.Click += new System.EventHandler(this.cmdStop_Click);
            // 
            // txtPopulation
            // 
            this.txtPopulation.Location = new System.Drawing.Point(131, 12);
            this.txtPopulation.Name = "txtPopulation";
            this.txtPopulation.Size = new System.Drawing.Size(100, 20);
            this.txtPopulation.TabIndex = 3;
            this.txtPopulation.Text = "250";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(68, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Population";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Mutation Rate (1 in ?)";
            // 
            // txtMutationRate
            // 
            this.txtMutationRate.Location = new System.Drawing.Point(131, 38);
            this.txtMutationRate.Name = "txtMutationRate";
            this.txtMutationRate.Size = new System.Drawing.Size(100, 20);
            this.txtMutationRate.TabIndex = 5;
            this.txtMutationRate.Text = "250";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(53, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Thread Count";
            // 
            // txtThreadCount
            // 
            this.txtThreadCount.Location = new System.Drawing.Point(131, 64);
            this.txtThreadCount.Name = "txtThreadCount";
            this.txtThreadCount.Size = new System.Drawing.Size(100, 20);
            this.txtThreadCount.TabIndex = 7;
            // 
            // lblProcessorCount
            // 
            this.lblProcessorCount.AutoSize = true;
            this.lblProcessorCount.Location = new System.Drawing.Point(238, 67);
            this.lblProcessorCount.Name = "lblProcessorCount";
            this.lblProcessorCount.Size = new System.Drawing.Size(98, 13);
            this.lblProcessorCount.TabIndex = 9;
            this.lblProcessorCount.Text = "(lblProcessorCount)";
            // 
            // cmdAnalyze
            // 
            this.cmdAnalyze.Location = new System.Drawing.Point(706, 146);
            this.cmdAnalyze.Name = "cmdAnalyze";
            this.cmdAnalyze.Size = new System.Drawing.Size(75, 23);
            this.cmdAnalyze.TabIndex = 10;
            this.cmdAnalyze.Text = "Analyze";
            this.cmdAnalyze.UseVisualStyleBackColor = true;
            this.cmdAnalyze.Click += new System.EventHandler(this.cmdAnalyze_Click);
            // 
            // txtPrintSolution
            // 
            this.txtPrintSolution.Location = new System.Drawing.Point(681, 12);
            this.txtPrintSolution.Name = "txtPrintSolution";
            this.txtPrintSolution.Size = new System.Drawing.Size(100, 20);
            this.txtPrintSolution.TabIndex = 11;
            // 
            // cmdPrintSolution
            // 
            this.cmdPrintSolution.Location = new System.Drawing.Point(681, 36);
            this.cmdPrintSolution.Name = "cmdPrintSolution";
            this.cmdPrintSolution.Size = new System.Drawing.Size(100, 23);
            this.cmdPrintSolution.TabIndex = 12;
            this.cmdPrintSolution.Text = "Print Solution";
            this.cmdPrintSolution.UseVisualStyleBackColor = true;
            this.cmdPrintSolution.Click += new System.EventHandler(this.cmdPrintSolution_Click);
            // 
            // txtStartingSolution
            // 
            this.txtStartingSolution.Location = new System.Drawing.Point(131, 90);
            this.txtStartingSolution.Name = "txtStartingSolution";
            this.txtStartingSolution.Size = new System.Drawing.Size(100, 20);
            this.txtStartingSolution.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Starting Solution";
            // 
            // frmGenetic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(793, 573);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtStartingSolution);
            this.Controls.Add(this.cmdPrintSolution);
            this.Controls.Add(this.txtPrintSolution);
            this.Controls.Add(this.cmdAnalyze);
            this.Controls.Add(this.lblProcessorCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtThreadCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMutationRate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPopulation);
            this.Controls.Add(this.cmdStop);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.cmdGo);
            this.Name = "frmGenetic";
            this.Text = "frmGenetic";
            this.Load += new System.EventHandler(this.frmGenetic_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdGo;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Button cmdStop;
        private System.Windows.Forms.TextBox txtPopulation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMutationRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtThreadCount;
        private System.Windows.Forms.Label lblProcessorCount;
        private System.Windows.Forms.Button cmdAnalyze;
        private System.Windows.Forms.TextBox txtPrintSolution;
        private System.Windows.Forms.Button cmdPrintSolution;
        private System.Windows.Forms.TextBox txtStartingSolution;
        private System.Windows.Forms.Label label4;
    }
}