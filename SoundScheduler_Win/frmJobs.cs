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
    public partial class frmJobs : Form {
        private IViewJobs _view;
        private IVirtualForm _form;
        private bool _isDirty;

        public frmJobs() {
            InitializeComponent();
        }

        private void BuildVirtualForm() {
            _form = new VirtualFormDecoratorDontCallDuringProcessing(new VirtualForm());

            IFormatLogic jobSelected = _form.AddFormatLogic(() => lstJobs.SelectedIndex > -1);
            jobSelected.ActionOnBoolean(new Action<bool>(x => {
                grpJobDetails.Enabled = x;
                cmdDelete.Enabled = x;
            }));
            jobSelected.ActionOnBoolean(new Action<bool>(x => {
                if (!x) {
                    txtJobName.Text = "";
                    chkVoidOnSoftException.Checked = false;
                    lstRelatedJobs.SelectedIndex = -1;
                    cboRelatedJobsToAdd.SelectedIndex = -1;
                }
            }));

            IFormatLogic isDirty = _form.AddFormatLogic(() => _isDirty);
            isDirty.ActionOnBoolean(new Action<bool>(x => cmdSave.Enabled = x));

            IFormatLogic relatedJobSelected = jobSelected.LogicOnTrue(() => lstRelatedJobs.SelectedIndex > -1);
            relatedJobSelected.ActionOnBoolean(new Action<bool>(x => cmdDeleteRelatedJob.Enabled = x));

            IFormatLogic relatedJobToAddSelected = jobSelected.LogicOnTrue(() => cboRelatedJobsToAdd.SelectedIndex > -1);
            relatedJobToAddSelected.ActionOnBoolean(new Action<bool>(x => cmdAddRelatedJob.Enabled = x));
        }

        private void RefreshScreen() {
            _form.PerformFormatLogic();
        }

        private void PopulateJobs() {
            lstJobs.Items.Clear();
            foreach (Job job in _view.Jobs) {
                lstJobs.Items.Add(job.Name);
            }
            RefreshScreen();
        }

        private void PopulateRelatedJobs() {
            lstRelatedJobs.Items.Clear();
            foreach (Job job in _view.RelatedJobs(_view.Jobs[lstJobs.SelectedIndex])) {
                lstRelatedJobs.Items.Add(job.Name);
            }
        }

        private void PopulateRelatedJobsToAdd() {
            cboRelatedJobsToAdd.Items.Clear();
            foreach (Job job in _view.ApplicableRelatedJobs(_view.Jobs[lstJobs.SelectedIndex])) {
                cboRelatedJobsToAdd.Items.Add(job.Name);
            }
        }

        private void frmJobs_Load(object sender, EventArgs e) {
            Factory factory = new Factory();
            _view = factory.CreateViewJobs();
            _view.LoadFromSource();
            BuildVirtualForm();
            PopulateJobs();
            RefreshScreen();
        }

        private void txtJobName_TextChanged() {
            _view.Jobs[lstJobs.SelectedIndex].Name = txtJobName.Text;
            _isDirty = true;
        }

        private void cmdSave_Click() {
            _view.SaveToSource();
            _isDirty = false;
            PopulateJobs();
            RefreshScreen();
        }

        private void cmdAdd_Click() {
            Job job = new Job();
            job.Name = "<New Job>";
            _view.Jobs.Add(job);
            _isDirty = true;
            PopulateJobs();
            RefreshScreen();
        }

        private void cmdDelete_Click() {
            _view.Jobs.RemoveAt(lstJobs.SelectedIndex);
            _isDirty = true;
            PopulateJobs();
            RefreshScreen();
        }

        private void lstJobs_SelectedIndexChanged() {
            Job job = _view.Jobs[lstJobs.SelectedIndex];
            txtJobName.Text = job.Name;
            chkVoidOnSoftException.Checked = job.IsVoidedOnSoftException;
            PopulateRelatedJobs();
            PopulateRelatedJobsToAdd();
            RefreshScreen();
        }

        private void chkVoidOnSoftException_CheckedChanged() {
            _view.Jobs[lstJobs.SelectedIndex].IsVoidedOnSoftException = chkVoidOnSoftException.Checked;
            _isDirty = true;
            RefreshScreen();
        }

        private void lstRelatedJobs_SelectedIndexChanged() {
            PopulateRelatedJobsToAdd();
            RefreshScreen();
        }

        private void cboRelatedJobsToAdd_SelectedIndexChanged() {
            RefreshScreen();
        }

        private void cmdAddRelatedJob_Click() {
            Job selectedJob = _view.Jobs[lstJobs.SelectedIndex];
            Job relatedJob = _view.ApplicableRelatedJobs(selectedJob).ElementAt(cboRelatedJobsToAdd.SelectedIndex);
            _view.AddRelatedJob(selectedJob, relatedJob);
            _isDirty = true;
            PopulateRelatedJobs();
            PopulateRelatedJobsToAdd();
            RefreshScreen();
        }

        private void cmdDeleteRelatedJob_Click() {
            Job selectedJob = _view.Jobs[lstJobs.SelectedIndex];
            Job relatedJob = _view.RelatedJobs(selectedJob).ElementAt(lstRelatedJobs.SelectedIndex);
            _view.RemoveRelatedJob(selectedJob, relatedJob);
            _isDirty = true;
            PopulateRelatedJobs();
            PopulateRelatedJobsToAdd();
            RefreshScreen();
        }

        private void txtJobName_TextChanged(object sender, EventArgs e) {
            _form.PerformAction(txtJobName_TextChanged, sender, e);
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

        private void lstJobs_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(lstJobs_SelectedIndexChanged, sender, e);
        }

        private void cmdClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void chkVoidOnSoftException_CheckedChanged(object sender, EventArgs e) {
            _form.PerformAction(chkVoidOnSoftException_CheckedChanged, sender, e);
        }

        private void lstRelatedJobs_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(lstRelatedJobs_SelectedIndexChanged, sender, e);
        }

        private void cboRelatedJobsToAdd_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(cboRelatedJobsToAdd_SelectedIndexChanged, sender, e);
        }

        private void cmdAddRelatedJob_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddRelatedJob_Click, sender, e);
        }

        private void cmdDeleteRelatedJob_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdDeleteRelatedJob_Click, sender, e);
        }
    }
}
