namespace SoundScheduler_Win {
    partial class frmTemplates {
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
            this.grpTemplateDetails = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdAddJob = new System.Windows.Forms.Button();
            this.cboJobsToAdd = new System.Windows.Forms.ComboBox();
            this.cmdDeleteJob = new System.Windows.Forms.Button();
            this.lstJobs = new System.Windows.Forms.ListBox();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.grpTemplateDetails.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTemplateDetails
            // 
            this.grpTemplateDetails.Controls.Add(this.groupBox1);
            this.grpTemplateDetails.Controls.Add(this.txtTemplateName);
            this.grpTemplateDetails.Controls.Add(this.label1);
            this.grpTemplateDetails.Location = new System.Drawing.Point(222, 12);
            this.grpTemplateDetails.Name = "grpTemplateDetails";
            this.grpTemplateDetails.Size = new System.Drawing.Size(254, 209);
            this.grpTemplateDetails.TabIndex = 18;
            this.grpTemplateDetails.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdAddJob);
            this.groupBox1.Controls.Add(this.cboJobsToAdd);
            this.groupBox1.Controls.Add(this.cmdDeleteJob);
            this.groupBox1.Controls.Add(this.lstJobs);
            this.groupBox1.Location = new System.Drawing.Point(6, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(244, 157);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Jobs";
            // 
            // cmdAddJob
            // 
            this.cmdAddJob.Location = new System.Drawing.Point(134, 119);
            this.cmdAddJob.Name = "cmdAddJob";
            this.cmdAddJob.Size = new System.Drawing.Size(21, 23);
            this.cmdAddJob.TabIndex = 9;
            this.cmdAddJob.Text = "+";
            this.cmdAddJob.UseVisualStyleBackColor = true;
            this.cmdAddJob.Click += new System.EventHandler(this.cmdAddJob_Click);
            // 
            // cboJobsToAdd
            // 
            this.cboJobsToAdd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboJobsToAdd.FormattingEnabled = true;
            this.cboJobsToAdd.Location = new System.Drawing.Point(7, 121);
            this.cboJobsToAdd.Name = "cboJobsToAdd";
            this.cboJobsToAdd.Size = new System.Drawing.Size(121, 21);
            this.cboJobsToAdd.TabIndex = 10;
            this.cboJobsToAdd.SelectedIndexChanged += new System.EventHandler(this.cboJobsToAdd_SelectedIndexChanged);
            // 
            // cmdDeleteJob
            // 
            this.cmdDeleteJob.Location = new System.Drawing.Point(128, 19);
            this.cmdDeleteJob.Name = "cmdDeleteJob";
            this.cmdDeleteJob.Size = new System.Drawing.Size(22, 23);
            this.cmdDeleteJob.TabIndex = 9;
            this.cmdDeleteJob.Text = "-";
            this.cmdDeleteJob.UseVisualStyleBackColor = true;
            this.cmdDeleteJob.Click += new System.EventHandler(this.cmdDeleteJob_Click);
            // 
            // lstJobs
            // 
            this.lstJobs.FormattingEnabled = true;
            this.lstJobs.Location = new System.Drawing.Point(6, 19);
            this.lstJobs.Name = "lstJobs";
            this.lstJobs.Size = new System.Drawing.Size(120, 95);
            this.lstJobs.TabIndex = 0;
            this.lstJobs.SelectedIndexChanged += new System.EventHandler(this.lstJobs_SelectedIndexChanged);
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Location = new System.Drawing.Point(53, 19);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(161, 20);
            this.txtTemplateName.TabIndex = 4;
            this.txtTemplateName.TextChanged += new System.EventHandler(this.txtTemplateName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Name:";
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(39, 152);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(22, 23);
            this.cmdDelete.TabIndex = 17;
            this.cmdDelete.Text = "-";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(12, 152);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(21, 23);
            this.cmdAdd.TabIndex = 16;
            this.cmdAdd.Text = "+";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.Location = new System.Drawing.Point(90, 198);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(75, 23);
            this.cmdClose.TabIndex = 15;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(9, 198);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(75, 23);
            this.cmdSave.TabIndex = 14;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // lstTemplates
            // 
            this.lstTemplates.FormattingEnabled = true;
            this.lstTemplates.Location = new System.Drawing.Point(12, 12);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.Size = new System.Drawing.Size(204, 134);
            this.lstTemplates.TabIndex = 13;
            this.lstTemplates.SelectedIndexChanged += new System.EventHandler(this.lstTemplates_SelectedIndexChanged);
            // 
            // frmTemplates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 237);
            this.Controls.Add(this.grpTemplateDetails);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cmdAdd);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.lstTemplates);
            this.Name = "frmTemplates";
            this.Text = "frmTemplates";
            this.Load += new System.EventHandler(this.frmTemplates_Load);
            this.grpTemplateDetails.ResumeLayout(false);
            this.grpTemplateDetails.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTemplateDetails;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cmdAddJob;
        private System.Windows.Forms.ComboBox cboJobsToAdd;
        private System.Windows.Forms.Button cmdDeleteJob;
        private System.Windows.Forms.ListBox lstJobs;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Button cmdAdd;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.ListBox lstTemplates;
    }
}