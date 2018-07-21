using System;
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

        private void cmdUsers_Click(object sender, EventArgs e) {
            frmUsers frm = new frmUsers();
            frm.ShowDialog();
        }

        private void cmdTemplates_Click(object sender, EventArgs e) {
            frmTemplates frm = new frmTemplates();
            frm.ShowDialog();
        }

        private void cmdMeetings_Click(object sender, EventArgs e) {
            frmMeetings frm = new frmMeetings();
            frm.ShowDialog();
        }

        private void cmdTester_Click(object sender, EventArgs e) {
            frmJobConsiderationTester frm = new frmJobConsiderationTester();
            frm.ShowDialog();
        }

        private void cmdGenetic_Click(object sender, EventArgs e) {
            frmGenetic frm = new frmGenetic();
            frm.ShowDialog();
        }
    }
}
