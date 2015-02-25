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
    public partial class frmTemplates : Form {
        private IViewTemplates _view;
        private IVirtualForm _form;
        private bool _isDirty;

        public frmTemplates() {
            InitializeComponent();
        }

        private void BuildVirtualForm() {
            _form = new VirtualFormDecoratorDontCallDuringProcessing(new VirtualForm());

            IFormatLogic templateSelected = _form.AddFormatLogic(() => lstTemplates.SelectedIndex > -1);
            templateSelected.ActionOnBoolean(new Action<bool>(x => {
                grpTemplateDetails.Enabled = x;
                cmdDelete.Enabled = x;
            }));
            templateSelected.ActionOnBoolean(new Action<bool>(x => {
                if (!x) {
                    txtTemplateName.Text = "";
                    lstJobs.SelectedIndex = -1;
                    cboJobsToAdd.SelectedIndex = -1;
                }
            }));

            IFormatLogic isDirty = _form.AddFormatLogic(() => _isDirty);
            isDirty.ActionOnBoolean(new Action<bool>(x => cmdSave.Enabled = x));

            IFormatLogic jobSelected = templateSelected.LogicOnTrue(() => lstJobs.SelectedIndex > -1);
            jobSelected.ActionOnBoolean(new Action<bool>(x => cmdDeleteJob.Enabled = x));

            IFormatLogic jobToAddSelected = templateSelected.LogicOnTrue(() => cboJobsToAdd.SelectedIndex > -1);
            jobToAddSelected.ActionOnBoolean(new Action<bool>(x => cmdAddJob.Enabled = x));
        }

        private void RefreshScreen() {
            _form.PerformFormatLogic();
        }

        private void PopulateTemplates() {
            lstTemplates.Items.Clear();
            foreach (Template template in _view.Templates) {
                lstTemplates.Items.Add(template.Name);
            }
        }

        private void PopulateJobs() {
            lstJobs.Items.Clear();
            foreach (Job job in _view.Jobs(_view.Templates[lstTemplates.SelectedIndex])) {
                lstJobs.Items.Add(job.Name);
            }
        }

        private void PopulateJobsToAdd() {
            cboJobsToAdd.Items.Clear();
            foreach (Job job in _view.ApplicableJobs(_view.Templates[lstTemplates.SelectedIndex])) {
                cboJobsToAdd.Items.Add(job.Name);
            }
        }

        private void cmdSave_Click() {
            _view.SaveToSource();
            _isDirty = false;
            PopulateTemplates();
            RefreshScreen();
        }

        private void lstTemplates_SelectedIndexChanged() {
            if (lstTemplates.SelectedIndex > -1) {
                Template template = _view.Templates[lstTemplates.SelectedIndex];
                txtTemplateName.Text = template.Name;
                PopulateJobs();
                PopulateJobsToAdd();
                RefreshScreen();
            }
        }

        private void cmdAdd_Click() {
            Template template = new Template();
            template.Name = "<New Template>";
            _view.Templates.Add(template);
            _isDirty = true;
            PopulateTemplates();
            RefreshScreen();
        }

        private void cmdDelete_Click() {
            _view.Templates.RemoveAt(lstTemplates.SelectedIndex);
            _isDirty = true;
            PopulateTemplates();
            PopulateJobs();
            PopulateJobsToAdd();
            RefreshScreen();
        }

        private void cmdDeleteJob_Click() {
            Template template = _view.Templates[lstTemplates.SelectedIndex];
            Job job = _view.Jobs(template).ElementAt(lstJobs.SelectedIndex);
            _view.RemoveJob(template, job);
            _isDirty = true;
            PopulateJobs();
            PopulateJobsToAdd();
            RefreshScreen();
        }

        private void cmdAddJob_Click() {
            Template template = _view.Templates[lstTemplates.SelectedIndex];
            Job job = _view.ApplicableJobs(template).ElementAt(cboJobsToAdd.SelectedIndex);
            _view.AddJob(template, job);
            _isDirty = true;
            PopulateJobs();
            PopulateJobsToAdd();
            RefreshScreen();
        }

        private void frmTemplates_Load(object sender, EventArgs e) {
            Factory factory = new Factory();
            _view = factory.CreateViewTemplates();
            _view.LoadFromSource();
            BuildVirtualForm();
            PopulateTemplates();
            RefreshScreen();
        }

        private void txtTemplateName_TextChanged() {
            _view.Templates[lstTemplates.SelectedIndex].Name = txtTemplateName.Text;
            _isDirty = true;
        }

        private void cboJobsToAdd_SelectedIndexChanged() {
            RefreshScreen();
        }

        private void cmdClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void lstJobs_SelectedIndexChanged(object sender, EventArgs e) {
            RefreshScreen();
        }

        private void cmdSave_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdSave_Click, sender, e);
        }

        private void lstTemplates_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(lstTemplates_SelectedIndexChanged, sender, e);
        }

        private void cmdAddJob_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddJob_Click, sender, e);
        }

        private void cmdDeleteJob_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdDeleteJob_Click, sender, e);
        }

        private void cmdAdd_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAdd_Click, sender, e);
        }

        private void cmdDelete_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdDelete_Click, sender, e);
        }

        private void txtTemplateName_TextChanged(object sender, EventArgs e) {
            _form.PerformAction(txtTemplateName_TextChanged, sender, e);
        }

        private void cboJobsToAdd_SelectedIndexChanged(object sender, EventArgs e) {
            _form.PerformAction(cboJobsToAdd_SelectedIndexChanged, sender, e);
        }
    }
}
