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
            foreach (NodeBase nodeBase in _view.Nodes) {
                treMeetings.Nodes.Add(PopulateNode(nodeBase));
            }
        }

        private TreeNode PopulateNode(NodeBase node) {
            TreeNode returnNode = null;
            if (node.Children.Count() > 0) {
                TreeNode[] childNodes = new TreeNode[node.Children.Count()];
                for (int i = 0; i < node.Children.Count(); i++) {
                    childNodes[i] = PopulateNode(node.Children.ElementAt(i));
                }
                returnNode = new TreeNode(node.Name, childNodes);
            } else {
                returnNode = new TreeNode(node.Name);
            }
            return returnNode;
        }

        private void PopulateTemplates() {
            cboTemplates.Items.Clear();
            foreach (Template template in _view.Templates) {
                cboTemplates.Items.Add(template.Name);
            }
        }

        private void PopulateUsers() {
            cboUsers.Items.Clear();
            foreach (User user in _view.Users) {
                cboUsers.Items.Add(user.Name);
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

        private void cmdAddHardException_Click() {
            User user = _view.Users.ElementAt(cboUsers.SelectedIndex);
            _view.AddMeetingException(calMeetingDate.SelectionRange.Start, user, true);
            PopulateMeetings();
        }

        private void cmdAddSoftException_Click() {
            User user = _view.Users.ElementAt(cboUsers.SelectedIndex);
            _view.AddMeetingException(calMeetingDate.SelectionRange.Start, user, false);
            PopulateMeetings();
        }

        private void frmMeetings_Load(object sender, EventArgs e) {
            Factory factory = new Factory();
            _view = factory.CreateViewMeetings();
            _view.LoadFromSource();
            BuildVirtualForm();
            PopulateMeetings();
            PopulateTemplates();
            PopulateUsers();
            RefreshScreen();
        }

        private void cmdAddMeeting_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddMeeting_Click, sender, e);
        }

        private void cmdAddHardException_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddHardException_Click, sender, e);
        }

        private void cmdAddSoftException_Click(object sender, EventArgs e) {
            _form.PerformAction(cmdAddSoftException_Click, sender, e);
        }
    }
}
