using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Views.Abstract;
using SharedSolutions_Logic.UI.WindowsForms;

namespace SoundScheduler_Win {
    public partial class frmMeetings : Form {
        private IViewMeetings _view;
        private IVirtualForm _form;
        private bool _isDirty;

        public frmMeetings() {
            InitializeComponent();
        }

        private void BuildVirtualForm() {
            _form = new VirtualFormDecoratorDontCallDuringProcessing(new VirtualForm());
        }

        private void PopulateMeetings() {
            treMeetings.Nodes.Clear();
            foreach (Meeting meeting in _view.Meetings) {
                TreeNode[] childNodes = new TreeNode[meeting.Jobs.Count];
                for (int i = 0; i < meeting.Jobs.Count; i++) {
                    TreeNode child = new TreeNode(_view.NodeDescriptionJob(meeting, meeting.Jobs[i]));
                    childNodes[i] = child;
                }
                TreeNode parentNode = new TreeNode(_view.NodeDescriptionMeeting(meeting), childNodes);
                treMeetings.Nodes.Add(parentNode);
            }
        }

        private void PopulateTemplates() {
            cboTemplates.Items.Clear();
            foreach (Template template in _view.Templates) {
                cboTemplates.Items.Add(template.Name);
            }
        }

        private void RefreshScreen() {
            _form.PerformFormatLogic();
        }

        private void cmdAddMeeting_Click() {
            Template template = _view.Templates.ElementAt(cboTemplates.SelectedIndex);
            _view.AddMeeting(template, calMeetingDate.SelectionRange.Start);
            PopulateMeetings();
        }

        private void frmMeetings_Load(object sender, EventArgs e) {
            Factory factory = new Factory();
            _view = factory.CreateViewMeetings();
            _view.LoadFromSource();
            BuildVirtualForm();
            PopulateMeetings();
            PopulateTemplates();
            RefreshScreen();
        }

        private void cmdAddMeeting_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddMeeting_Click, sender, e);
        }


    }
}
