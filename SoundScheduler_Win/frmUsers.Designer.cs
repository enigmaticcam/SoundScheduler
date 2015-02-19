namespace SoundScheduler_Win {
    partial class frmUsers {
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
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.grpUserDetails = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdAddJob = new System.Windows.Forms.Button();
            this.cboJobsToAdd = new System.Windows.Forms.ComboBox();
            this.cmdDeleteJob = new System.Windows.Forms.Button();
            this.lstJobs = new System.Windows.Forms.ListBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpUserDetails.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdDelete
            // 
            this.cmdDelete.Location = new System.Drawing.Point(39, 152);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(22, 23);
            this.cmdDelete.TabIndex = 11;
            this.cmdDelete.Text = "-";
            this.cmdDelete.UseVisualStyleBackColor = true;
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(12, 152);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(21, 23);
            this.cmdAdd.TabIndex = 10;
            this.cmdAdd.Text = "+";
            this.cmdAdd.UseVisualStyleBackColor = true;
            // 
            // cmdClose
            // 
            this.cmdClose.Location = new System.Drawing.Point(90, 198);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(75, 23);
            this.cmdClose.TabIndex = 9;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(9, 198);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(75, 23);
            this.cmdSave.TabIndex = 8;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            // 
            // lstUsers
            // 
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.Location = new System.Drawing.Point(12, 12);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(204, 134);
            this.lstUsers.TabIndex = 7;
            // 
            // grpUserDetails
            // 
            this.grpUserDetails.Controls.Add(this.groupBox1);
            this.grpUserDetails.Controls.Add(this.txtUserName);
            this.grpUserDetails.Controls.Add(this.label1);
            this.grpUserDetails.Location = new System.Drawing.Point(222, 12);
            this.grpUserDetails.Name = "grpUserDetails";
            this.grpUserDetails.Size = new System.Drawing.Size(254, 209);
            this.grpUserDetails.TabIndex = 12;
            this.grpUserDetails.TabStop = false;
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
            // 
            // cboJobsToAdd
            // 
            this.cboJobsToAdd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboJobsToAdd.FormattingEnabled = true;
            this.cboJobsToAdd.Location = new System.Drawing.Point(7, 121);
            this.cboJobsToAdd.Name = "cboJobsToAdd";
            this.cboJobsToAdd.Size = new System.Drawing.Size(121, 21);
            this.cboJobsToAdd.TabIndex = 10;
            // 
            // cmdDeleteJob
            // 
            this.cmdDeleteJob.Location = new System.Drawing.Point(128, 19);
            this.cmdDeleteJob.Name = "cmdDeleteJob";
            this.cmdDeleteJob.Size = new System.Drawing.Size(22, 23);
            this.cmdDeleteJob.TabIndex = 9;
            this.cmdDeleteJob.Text = "-";
            this.cmdDeleteJob.UseVisualStyleBackColor = true;
            // 
            // lstJobs
            // 
            this.lstJobs.FormattingEnabled = true;
            this.lstJobs.Location = new System.Drawing.Point(6, 19);
            this.lstJobs.Name = "lstJobs";
            this.lstJobs.Size = new System.Drawing.Size(120, 95);
            this.lstJobs.TabIndex = 0;
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(53, 19);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(161, 20);
            this.txtUserName.TabIndex = 4;
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
            // frmUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 233);
            this.Controls.Add(this.grpUserDetails);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cmdAdd);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.lstUsers);
            this.Name = "frmUsers";
            this.Text = "frmUsers";
            this.grpUserDetails.ResumeLayout(false);
            this.grpUserDetails.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Button cmdAdd;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.GroupBox grpUserDetails;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cmdAddJob;
        private System.Windows.Forms.ComboBox cboJobsToAdd;
        private System.Windows.Forms.Button cmdDeleteJob;
        private System.Windows.Forms.ListBox lstJobs;

    }
}