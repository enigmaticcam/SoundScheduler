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
            this.SuspendLayout();
            // 
            // treMeetings
            // 
            this.treMeetings.Location = new System.Drawing.Point(12, 12);
            this.treMeetings.Name = "treMeetings";
            this.treMeetings.Size = new System.Drawing.Size(264, 248);
            this.treMeetings.TabIndex = 0;
            // 
            // cmdAddMeeting
            // 
            this.cmdAddMeeting.Location = new System.Drawing.Point(415, 185);
            this.cmdAddMeeting.Name = "cmdAddMeeting";
            this.cmdAddMeeting.Size = new System.Drawing.Size(75, 23);
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
            // 
            // cboTemplates
            // 
            this.cboTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTemplates.FormattingEnabled = true;
            this.cboTemplates.Location = new System.Drawing.Point(288, 187);
            this.cboTemplates.Name = "cboTemplates";
            this.cboTemplates.Size = new System.Drawing.Size(121, 21);
            this.cboTemplates.TabIndex = 3;
            // 
            // frmMeetings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 518);
            this.Controls.Add(this.cboTemplates);
            this.Controls.Add(this.calMeetingDate);
            this.Controls.Add(this.cmdAddMeeting);
            this.Controls.Add(this.treMeetings);
            this.Name = "frmMeetings";
            this.Text = "frmMeetings";
            this.Load += new System.EventHandler(this.frmMeetings_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treMeetings;
        private System.Windows.Forms.Button cmdAddMeeting;
        private System.Windows.Forms.MonthCalendar calMeetingDate;
        private System.Windows.Forms.ComboBox cboTemplates;
    }
}