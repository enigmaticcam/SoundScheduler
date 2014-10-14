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
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_Win {
    public partial class frmMain : Form {
        private List<Meeting> _meetings;

        public frmMain() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            // --------------------------------------------------------
            // JOBS
            // --------------------------------------------------------
            Job jobSound = new Job();
            jobSound.Name = "Sound Box";

            Job jobStage = new Job();
            jobStage.Name = "Stage";

            Job jobMic = new Job();
            jobMic.Name = "Mic";

            // --------------------------------------------------------
            // USERS
            // --------------------------------------------------------

            List<User> users = new List<User>();

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userDLopez);

            User userJHermosillo = new User();
            userJHermosillo.Name = "Joseph Hermosillo";
            userJHermosillo.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userJHermosillo);

            User userEWilder = new User();
            userEWilder.Name = "Ethan Wilder";
            userEWilder.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userEWilder);

            User userCOldani = new User();
            userCOldani.Name = "Chris Oldani";
            userCOldani.Jobs = new List<Job> { jobMic };
            users.Add(userCOldani);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic };
            users.Add(userRStubbs);

            User userKSugiyama = new User();
            userKSugiyama.Name = "Keigi Sugiyama";
            userKSugiyama.Jobs = new List<Job> { jobMic };
            users.Add(userKSugiyama);

            // --------------------------------------------------------
            // TEMPLATES
            // --------------------------------------------------------

            Template templateSunday = new Template();
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic, jobMic, jobMic, jobMic };

            Template templateTuesday = new Template();
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic, jobMic };

            // --------------------------------------------------------
            // EXCEPTIONS
            // --------------------------------------------------------

            List<HashSet<User>> exceptions = new List<HashSet<User>>();
            for (int i = 1; i <= 20; i++) {
                exceptions.Add(new HashSet<User>());
            }
            exceptions[6].Add(userCTangen);
            exceptions[8].Add(userCTangen);
            exceptions[8].Add(userDCook);
            exceptions[10].Add(userDCook);
            exceptions[10].Add(userRStubbs);
            exceptions[10].Add(userKSugiyama);
            exceptions[12].Add(userKSugiyama);

            // --------------------------------------------------------
            // WORK
            // --------------------------------------------------------

            SoundBuilder engine = new SoundBuilder.Builder()
               .SetJobs(new List<Job> { jobSound, jobStage, jobMic })
               .SetUser(new List<User>(users))
               .SetTemplates(new List<Template> { 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday,
                    templateTuesday, templateSunday
                })
                .SetExistingMeetings(new List<SoundBuilder.ExistingMeeting>())
                .SetExceptions(exceptions)
               .Build();

            _meetings = engine.BuildSchedule();
            PopulateAllData();
        }

        private void PopulateAllData() {
            PopulateMeetings();
        }

        private void PopulateMeetings() {
            cboMeetings.Items.Clear();
            for (int i = 0; i < _meetings.Count; i++) {
                Meeting meeting = _meetings[i];
                SelectItem item = new SelectItem();
                item.Text = meeting.Name;
                item.Value = i.ToString();
                cboMeetings.Items.Add(item);
            }
        }

        private void cboMeetings_SelectedIndexChanged(object sender, EventArgs e) {
            Meeting meeting = _meetings[cboMeetings.SelectedIndex];
            StringBuilder message = new StringBuilder();
            message.AppendLine("Meeting Name: " + meeting.Name);
            message.AppendLine("");
            for (int i = 0; i < meeting.Jobs.Count; i++) {
                message.Append("Job: " + meeting.Jobs[i].Name);
                message.AppendLine(" (" + meeting.JobUserSlots.UserForJob(i).Name + ")");
            }
            txtResults.Text = message.ToString();
        }
    }

    public class SelectItem {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
