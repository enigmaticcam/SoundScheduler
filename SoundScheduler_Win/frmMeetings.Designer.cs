namespace SoundScheduler_Win {
    partial class frmMeetings {
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
            this.treMeetings = new System.Windows.Forms.TreeView();
            this.cmdAddMeeting = new System.Windows.Forms.Button();
            this.calMeetingDate = new System.Windows.Forms.MonthCalendar();
            this.cboTemplates = new System.Windows.Forms.ComboBox();
            this.cmdAddSoftException = new System.Windows.Forms.Button();
            this.cmdAddHardException = new System.Windows.Forms.Button();
            this.cboUsers = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmdDeleteNode = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // treMeetings
            // 
            this.treMeetings.Location = new System.Drawing.Point(12, 12);
            this.treMeetings.Name = "treMeetings";
            this.treMeetings.Size = new System.Drawing.Size(264, 463);
            this.treMeetings.TabIndex = 0;
            this.treMeetings.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMeetings_AfterSelect);
            // 
            // cmdAddMeeting
            // 
            this.cmdAddMeeting.Location = new System.Drawing.Point(121, 19);
            this.cmdAddMeeting.Name = "cmdAddMeeting";
            this.cmdAddMeeting.Size = new System.Drawing.Size(100, 23);
            this.cmdAddMeeting.TabIndex = 1;
            this.cmdAddMeeting.Text = "Add Meeting";
            this.cmdAddMeeting.UseVisualStyleBackColor = true;
            this.cmdAddMeeting.Click += new System.EventHandler(this.cmdAddMeeting_Click);
            // 
            // calMeetingDate
            // 
            this.calMeetingDate.Location = new System.Drawing.Point(288, 12);
            this.calMeetingDate.MaxSelectionCount = 1;
            this.calMeetingDate.Name = "calMeetingDate";
            this.calMeetingDate.TabIndex = 2;
            this.calMeetingDate.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.calMeetingDate_DateChanged);
            // 
            // cboTemplates
            // 
            this.cboTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTemplates.FormattingEnabled = true;
            this.cboTemplates.Location = new System.Drawing.Point(6, 19);
            this.cboTemplates.Name = "cboTemplates";
            this.cboTemplates.Size = new System.Drawing.Size(109, 21);
            this.cboTemplates.TabIndex = 3;
            this.cboTemplates.SelectedIndexChanged += new System.EventHandler(this.cboTemplates_SelectedIndexChanged);
            // 
            // cmdAddSoftException
            // 
            this.cmdAddSoftException.Location = new System.Drawing.Point(121, 48);
            this.cmdAddSoftException.Name = "cmdAddSoftException";
            this.cmdAddSoftException.Size = new System.Drawing.Size(115, 23);
            this.cmdAddSoftException.TabIndex = 4;
            this.cmdAddSoftException.Text = "Add Soft Exception";
            this.cmdAddSoftException.UseVisualStyleBackColor = true;
            this.cmdAddSoftException.Click += new System.EventHandler(this.cmdAddSoftException_Click);
            // 
            // cmdAddHardException
            // 
            this.cmdAddHardException.Location = new System.Drawing.Point(121, 19);
            this.cmdAddHardException.Name = "cmdAddHardException";
            this.cmdAddHardException.Size = new System.Drawing.Size(115, 23);
            this.cmdAddHardException.TabIndex = 5;
            this.cmdAddHardException.Text = "Add Hard Exception";
            this.cmdAddHardException.UseVisualStyleBackColor = true;
            this.cmdAddHardException.Click += new System.EventHandler(this.cmdAddHardException_Click);
            // 
            // cboUsers
            // 
            this.cboUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsers.FormattingEnabled = true;
            this.cboUsers.Location = new System.Drawing.Point(6, 19);
            this.cboUsers.Name = "cboUsers";
            this.cboUsers.Size = new System.Drawing.Size(109, 21);
            this.cboUsers.TabIndex = 6;
            this.cboUsers.SelectedIndexChanged += new System.EventHandler(this.cboUsers_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdAddMeeting);
            this.groupBox1.Controls.Add(this.cboTemplates);
            this.groupBox1.Location = new System.Drawing.Point(288, 186);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(243, 54);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cboUsers);
            this.groupBox2.Controls.Add(this.cmdAddHardException);
            this.groupBox2.Controls.Add(this.cmdAddSoftException);
            this.groupBox2.Location = new System.Drawing.Point(288, 247);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(243, 79);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // cmdDeleteNode
            // 
            this.cmdDeleteNode.Enabled = false;
            this.cmdDeleteNode.Location = new System.Drawing.Point(12, 481);
            this.cmdDeleteNode.Name = "cmdDeleteNode";
            this.cmdDeleteNode.Size = new System.Drawing.Size(23, 23);
            this.cmdDeleteNode.TabIndex = 9;
            this.cmdDeleteNode.Text = "-";
            this.cmdDeleteNode.UseVisualStyleBackColor = true;
            // 
            // frmMeetings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 515);
            this.Controls.Add(this.cmdDeleteNode);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.calMeetingDate);
            this.Controls.Add(this.treMeetings);
            this.Name = "frmMeetings";
            this.Text = "frmMeetings";
            this.Load += new System.EventHandler(this.frmMeetings_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treMeetings;
        private System.Windows.Forms.Button cmdAddMeeting;
        private System.Windows.Forms.MonthCalendar calMeetingDate;
        private System.Windows.Forms.ComboBox cboTemplates;
        private System.Windows.Forms.Button cmdAddSoftException;
        private System.Windows.Forms.Button cmdAddHardException;
        private System.Windows.Forms.ComboBox cboUsers;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button cmdDeleteNode;
    }
}