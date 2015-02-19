using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedSolutions_Logic.UI.WindowsForms;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Views.Abstract;

namespace SoundScheduler_Win {
    public partial class frmUsers : Form {
        private IViewUsers _view;
        private IVirtualForm _form;
        private bool _isDirty;

        public frmUsers() {
            InitializeComponent();
        }

        private void BuildVirtualForm() {
            _form = new VirtualFormDecoratorDontCallDuringProcessing(new VirtualForm());

            IFormatLogic userSelected = _form.AddFormatLogic(() => lstUsers.SelectedIndex > -1);
            userSelected.ActionOnBoolean(new Action<bool>(x => {
                grpUserDetails.Enabled = x;
                cmdDelete.Enabled = x;
            }));
            userSelected.ActionOnBoolean(new Action<bool>(x => {
                if (!x) {
                    txtUserName.Text = "";
                    lstJobs.SelectedIndex = -1;
                    cboJobsToAdd.SelectedIndex = -1;
                }
            }));

            IFormatLogic isDirty = _form.AddFormatLogic(() => _isDirty);
            isDirty.ActionOnBoolean(new Action<bool>(x => cmdSave.Enabled = x));

            IFormatLogic jobSelected = userSelected.LogicOnTrue(() => lstJobs.SelectedIndex > -1);
            jobSelected.ActionOnBoolean(new Action<bool>(x => cmdDeleteJob.Enabled = x));

            IFormatLogic jobToAddSelected = jobSelected.LogicOnTrue(() => cboJobsToAdd.SelectedIndex > -1);
            jobToAddSelected.ActionOnBoolean(new Action<bool>(x => cmdAddJob.Enabled = x));
        }

        private void RefreshScreen() {
            _form.PerformFormatLogic();
        }

        private void PopulateUsers() {
            lstUsers.Items.Clear();
            foreach (User user in _view.Users) {
                lstUsers.Items.Add(user.Name);
            }
            RefreshScreen();
        }

        private void PopulateJobs() {
            lstJobs.Items.Clear();
            foreach (Job job in _view.Jobs(_view.Users.ElementAt(lstUsers.SelectedIndex))) {
                lstJobs.Items.Add(job.Name);
            }
        }

        private void frmUsers_Load(object sender, EventArgs e) {
            Factory factory = new Factory();
            _view = factory.CreateViewUsers();
            _view.LoadFromSource();
            BuildVirtualForm();
            PopulateUsers();
            RefreshScreen();
        }

        private void txtUserName_TextChanged() {
            _view.Users[lstUsers.SelectedIndex].Name = txtUserName.Text;
            _isDirty = true;
        }

        private void cmdSave_Click() {
            _view.SaveToSource();
            _isDirty = false;
            PopulateUsers();
            RefreshScreen();
        }

        private void cmdAdd_Click() {
            User user = new User();
            user.Name = "<New User>";
            _view.Users.Add(user);
            _isDirty = true;
            PopulateUsers();
            RefreshScreen();
        }

        private void cmdDelete_Click() {
            _view.Users.RemoveAt(lstUsers.SelectedIndex);
            _isDirty = true;
            PopulateUsers();
            RefreshScreen();
        }

        private void lstUsers_SelectedIndexChanged() {
            User user = _view.Users[lstUsers.SelectedIndex];
            txtUserName.Text = user.Name;
            PopulateJobs();
            RefreshScreen();
        }

        private void cboJobsToAdd_SelectedIndexChanged() {
            RefreshScreen();
        }

        private void cmdAddJob_Click() {
            User user = _view.Users[lstUsers.SelectedIndex];
            Job job = _view.Jobs(user).ElementAt(lstJobs.SelectedIndex);
            _view.AddJob(user, job);
            PopulateJobs();
            RefreshScreen();
        }

        private void cmdDeleteJob_Click() {
            User user = _view.Users[lstUsers.SelectedIndex];
            Job job = _view.Jobs(user).ElementAt(lstJobs.SelectedIndex);
            _view.RemoveJob(user, job);
            PopulateJobs();
            RefreshScreen();
        }

        private void txtUserName_TextChanged(object sender, EventArgs e) {
            _form.PerformAction(txtUserName_TextChanged, sender, e);
        }

        private void cmdSave_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdSave_Click, sender, e);
        }

        private void cmdAdd_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAdd_Click, sender, e);
        }

        private void cmdDelete_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdDelete_Click, sender, e);
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(lstUsers_SelectedIndexChanged, sender, e);
        }

        private void cboJobsToAdd_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(cboJobsToAdd_SelectedIndexChanged, sender, e);
        }

        private void cmdAddJob_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddJob_Click, sender, e);
        }

        private void cmdDeleteJob_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdDeleteJob_Click, sender, e);
        }

        private void cmdClose_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
