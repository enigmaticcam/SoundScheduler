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
    }
}
