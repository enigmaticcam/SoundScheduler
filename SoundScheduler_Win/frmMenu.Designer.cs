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
            this.cmdPreviousMeetings = new System.Windows.Forms.Button();
            this.cmdFutureExceptions = new System.Windows.Forms.Button();
            this.cmdAlgorithmComponents = new System.Windows.Forms.Button();
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
            // 
            // cmdTemplates
            // 
            this.cmdTemplates.Location = new System.Drawing.Point(13, 85);
            this.cmdTemplates.Name = "cmdTemplates";
            this.cmdTemplates.Size = new System.Drawing.Size(123, 66);
            this.cmdTemplates.TabIndex = 2;
            this.cmdTemplates.Text = "Templates";
            this.cmdTemplates.UseVisualStyleBackColor = true;
            // 
            // cmdPreviousMeetings
            // 
            this.cmdPreviousMeetings.Location = new System.Drawing.Point(142, 85);
            this.cmdPreviousMeetings.Name = "cmdPreviousMeetings";
            this.cmdPreviousMeetings.Size = new System.Drawing.Size(123, 66);
            this.cmdPreviousMeetings.TabIndex = 3;
            this.cmdPreviousMeetings.Text = "Previous Meetings";
            this.cmdPreviousMeetings.UseVisualStyleBackColor = true;
            // 
            // cmdFutureExceptions
            // 
            this.cmdFutureExceptions.Location = new System.Drawing.Point(271, 85);
            this.cmdFutureExceptions.Name = "cmdFutureExceptions";
            this.cmdFutureExceptions.Size = new System.Drawing.Size(123, 66);
            this.cmdFutureExceptions.TabIndex = 4;
            this.cmdFutureExceptions.Text = "Future Exceptions";
            this.cmdFutureExceptions.UseVisualStyleBackColor = true;
            // 
            // cmdAlgorithmComponents
            // 
            this.cmdAlgorithmComponents.Location = new System.Drawing.Point(13, 157);
            this.cmdAlgorithmComponents.Name = "cmdAlgorithmComponents";
            this.cmdAlgorithmComponents.Size = new System.Drawing.Size(123, 66);
            this.cmdAlgorithmComponents.TabIndex = 6;
            this.cmdAlgorithmComponents.Text = "Algorithm Components";
            this.cmdAlgorithmComponents.UseVisualStyleBackColor = true;
            // 
            // frmMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 269);
            this.Controls.Add(this.cmdAlgorithmComponents);
            this.Controls.Add(this.cmdFutureExceptions);
            this.Controls.Add(this.cmdPreviousMeetings);
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
        private System.Windows.Forms.Button cmdPreviousMeetings;
        private System.Windows.Forms.Button cmdFutureExceptions;
        private System.Windows.Forms.Button cmdAlgorithmComponents;
    }
}