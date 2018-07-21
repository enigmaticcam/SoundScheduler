namespace SoundScheduler_Win {
    partial class frmMenu {
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
            this.cmdJobs = new System.Windows.Forms.Button();
            this.cmdUsers = new System.Windows.Forms.Button();
            this.cmdTemplates = new System.Windows.Forms.Button();
            this.cmdMeetings = new System.Windows.Forms.Button();
            this.cmdGenetic = new System.Windows.Forms.Button();
            this.cmdTester = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdJobs
            // 
            this.cmdJobs.Location = new System.Drawing.Point(13, 13);
            this.cmdJobs.Name = "cmdJobs";
            this.cmdJobs.Size = new System.Drawing.Size(123, 66);
            this.cmdJobs.TabIndex = 0;
            this.cmdJobs.Text = "Jobs";
            this.cmdJobs.UseVisualStyleBackColor = true;
            this.cmdJobs.Click += new System.EventHandler(this.cmdJobs_Click);
            // 
            // cmdUsers
            // 
            this.cmdUsers.Location = new System.Drawing.Point(142, 13);
            this.cmdUsers.Name = "cmdUsers";
            this.cmdUsers.Size = new System.Drawing.Size(123, 66);
            this.cmdUsers.TabIndex = 1;
            this.cmdUsers.Text = "Users";
            this.cmdUsers.UseVisualStyleBackColor = true;
            this.cmdUsers.Click += new System.EventHandler(this.cmdUsers_Click);
            // 
            // cmdTemplates
            // 
            this.cmdTemplates.Location = new System.Drawing.Point(13, 85);
            this.cmdTemplates.Name = "cmdTemplates";
            this.cmdTemplates.Size = new System.Drawing.Size(123, 66);
            this.cmdTemplates.TabIndex = 2;
            this.cmdTemplates.Text = "Templates";
            this.cmdTemplates.UseVisualStyleBackColor = true;
            this.cmdTemplates.Click += new System.EventHandler(this.cmdTemplates_Click);
            // 
            // cmdMeetings
            // 
            this.cmdMeetings.Location = new System.Drawing.Point(142, 85);
            this.cmdMeetings.Name = "cmdMeetings";
            this.cmdMeetings.Size = new System.Drawing.Size(123, 66);
            this.cmdMeetings.TabIndex = 4;
            this.cmdMeetings.Text = "Meetings";
            this.cmdMeetings.UseVisualStyleBackColor = true;
            this.cmdMeetings.Click += new System.EventHandler(this.cmdMeetings_Click);
            // 
            // cmdGenetic
            // 
            this.cmdGenetic.Location = new System.Drawing.Point(271, 13);
            this.cmdGenetic.Name = "cmdGenetic";
            this.cmdGenetic.Size = new System.Drawing.Size(123, 66);
            this.cmdGenetic.TabIndex = 5;
            this.cmdGenetic.Text = "Genetic";
            this.cmdGenetic.UseVisualStyleBackColor = true;
            this.cmdGenetic.Click += new System.EventHandler(this.cmdGenetic_Click);
            // 
            // cmdTester
            // 
            this.cmdTester.Location = new System.Drawing.Point(271, 85);
            this.cmdTester.Name = "cmdTester";
            this.cmdTester.Size = new System.Drawing.Size(123, 66);
            this.cmdTester.TabIndex = 6;
            this.cmdTester.Text = "Tester";
            this.cmdTester.UseVisualStyleBackColor = true;
            this.cmdTester.Click += new System.EventHandler(this.cmdTester_Click);
            // 
            // frmMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 166);
            this.Controls.Add(this.cmdTester);
            this.Controls.Add(this.cmdGenetic);
            this.Controls.Add(this.cmdMeetings);
            this.Controls.Add(this.cmdTemplates);
            this.Controls.Add(this.cmdUsers);
            this.Controls.Add(this.cmdJobs);
            this.Name = "frmMenu";
            this.Text = "frmMenu";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdJobs;
        private System.Windows.Forms.Button cmdUsers;
        private System.Windows.Forms.Button cmdTemplates;
        private System.Windows.Forms.Button cmdMeetings;
        private System.Windows.Forms.Button cmdGenetic;
        private System.Windows.Forms.Button cmdTester;
    }
}