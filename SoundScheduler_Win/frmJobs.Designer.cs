namespace SoundScheduler_Win {
    partial class frmJobs {
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
            this.lstJobs = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.chkVoidOnSoftException = new System.Windows.Forms.CheckBox();
            this.grpJobDetails = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdAddRelatedJob = new System.Windows.Forms.Button();
            this.cboRelatedJobsToAdd = new System.Windows.Forms.ComboBox();
            this.cmdDeleteRelatedJob = new System.Windows.Forms.Button();
            this.lstRelatedJobs = new System.Windows.Forms.ListBox();
            this.grpJobDetails.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstJobs
            // 
            this.lstJobs.FormattingEnabled = true;
            this.lstJobs.Location = new System.Drawing.Point(13, 13);
            this.lstJobs.Name = "lstJobs";
            this.lstJobs.Size = new System.Drawing.Size(204, 134);
            this.lstJobs.TabIndex = 0;
            this.lstJobs.SelectedIndexChanged += new System.EventHandler(this.lstJobs_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // txtJobName
            // 
            this.txtJobName.Location = new System.Drawing.Point(63, 19);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(161, 20);
            this.txtJobName.TabIndex = 2;
            this.txtJobName.TextChanged += new System.EventHandler(this.txtJobName_TextChanged);
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(12, 256);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(75, 23);
            this.cmdSave.TabIndex = 3;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.Location = new System.Drawing.Point(93, 256);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(75, 23);
            this.cmdClose.TabIndex = 4;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(13, 153);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(21, 23);
            this.cmdAdd.TabIndex = 5;
            this.cmdAdd.Text = "+";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(40, 153);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(22, 23);
            this.cmdDelete.TabIndex = 6;
            this.cmdDelete.Text = "-";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // chkVoidOnSoftException
            // 
            this.chkVoidOnSoftException.AutoSize = true;
            this.chkVoidOnSoftException.Location = new System.Drawing.Point(22, 45);
            this.chkVoidOnSoftException.Name = "chkVoidOnSoftException";
            this.chkVoidOnSoftException.Size = new System.Drawing.Size(134, 17);
            this.chkVoidOnSoftException.TabIndex = 7;
            this.chkVoidOnSoftException.Text = "Void on Soft Exception";
            this.chkVoidOnSoftException.UseVisualStyleBackColor = true;
            this.chkVoidOnSoftException.CheckedChanged += new System.EventHandler(this.chkVoidOnSoftException_CheckedChanged);
            // 
            // grpJobDetails
            // 
            this.grpJobDetails.Controls.Add(this.groupBox1);
            this.grpJobDetails.Controls.Add(this.txtJobName);
            this.grpJobDetails.Controls.Add(this.chkVoidOnSoftException);
            this.grpJobDetails.Controls.Add(this.label1);
            this.grpJobDetails.Location = new System.Drawing.Point(223, 12);
            this.grpJobDetails.Name = "grpJobDetails";
            this.grpJobDetails.Size = new System.Drawing.Size(256, 267);
            this.grpJobDetails.TabIndex = 8;
            this.grpJobDetails.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdAddRelatedJob);
            this.groupBox1.Controls.Add(this.cboRelatedJobsToAdd);
            this.groupBox1.Controls.Add(this.cmdDeleteRelatedJob);
            this.groupBox1.Controls.Add(this.lstRelatedJobs);
            this.groupBox1.Location = new System.Drawing.Point(6, 83);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(244, 178);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Related Jobs";
            // 
            // cmdAddRelatedJob
            // 
            this.cmdAddRelatedJob.Location = new System.Drawing.Point(134, 119);
            this.cmdAddRelatedJob.Name = "cmdAddRelatedJob";
            this.cmdAddRelatedJob.Size = new System.Drawing.Size(21, 23);
            this.cmdAddRelatedJob.TabIndex = 9;
            this.cmdAddRelatedJob.Text = "+";
            this.cmdAddRelatedJob.UseVisualStyleBackColor = true;
            this.cmdAddRelatedJob.Click += new System.EventHandler(this.cmdAddRelatedJob_Click);
            // 
            // cboRelatedJobsToAdd
            // 
            this.cboRelatedJobsToAdd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRelatedJobsToAdd.FormattingEnabled = true;
            this.cboRelatedJobsToAdd.Location = new System.Drawing.Point(7, 121);
            this.cboRelatedJobsToAdd.Name = "cboRelatedJobsToAdd";
            this.cboRelatedJobsToAdd.Size = new System.Drawing.Size(121, 21);
            this.cboRelatedJobsToAdd.TabIndex = 10;
            this.cboRelatedJobsToAdd.SelectedIndexChanged += new System.EventHandler(this.cboRelatedJobsToAdd_SelectedIndexChanged);
            // 
            // cmdDeleteRelatedJob
            // 
            this.cmdDeleteRelatedJob.Location = new System.Drawing.Point(128, 19);
            this.cmdDeleteRelatedJob.Name = "cmdDeleteRelatedJob";
            this.cmdDeleteRelatedJob.Size = new System.Drawing.Size(22, 23);
            this.cmdDeleteRelatedJob.TabIndex = 9;
            this.cmdDeleteRelatedJob.Text = "-";
            this.cmdDeleteRelatedJob.UseVisualStyleBackColor = true;
            this.cmdDeleteRelatedJob.Click += new System.EventHandler(this.cmdDeleteRelatedJob_Click);
            // 
            // lstRelatedJobs
            // 
            this.lstRelatedJobs.FormattingEnabled = true;
            this.lstRelatedJobs.Location = new System.Drawing.Point(6, 19);
            this.lstRelatedJobs.Name = "lstRelatedJobs";
            this.lstRelatedJobs.Size = new System.Drawing.Size(120, 95);
            this.lstRelatedJobs.TabIndex = 0;
            this.lstRelatedJobs.SelectedIndexChanged += new System.EventHandler(this.lstRelatedJobs_SelectedIndexChanged);
            // 
            // frmJobs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(491, 291);
            this.Controls.Add(this.grpJobDetails);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cmdAdd);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.lstJobs);
            this.Name = "frmJobs";
            this.Text = "frmJobs";
            this.Load += new System.EventHandler(this.frmJobs_Load);
            this.grpJobDetails.ResumeLayout(false);
            this.grpJobDetails.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstJobs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtJobName;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdAdd;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.CheckBox chkVoidOnSoftException;
        private System.Windows.Forms.GroupBox grpJobDetails;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cmdAddRelatedJob;
        private System.Windows.Forms.ComboBox cboRelatedJobsToAdd;
        private System.Windows.Forms.Button cmdDeleteRelatedJob;
        private System.Windows.Forms.ListBox lstRelatedJobs;
    }
}