﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundScheduler_Win {
    public partial class frmMenu : Form {
        public frmMenu() {
            InitializeComponent();
        }

        private void cmdJobs_Click(object sender, EventArgs e) {
            frmJobs frm = new frmJobs();
            frm.ShowDialog();
        }
    }
}