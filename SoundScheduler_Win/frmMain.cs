﻿using System;
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
        private SoundBuilderV2.MeetingsByDate _meetings;

        public frmMain() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            // --------------------------------------------------------
            // JOBS
            // --------------------------------------------------------
            List<Job> jobs = new List<Job>();

            Job jobSound = new Job();
            jobSound.Name = "Sound Box";
            jobs.Add(jobSound);
            jobSound.IsVoidedOnSoftException = true;

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            jobs.Add(jobStage);
            jobStage.IsVoidedOnSoftException = false;

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            jobs.Add(jobMic1);
            jobMic1.IsVoidedOnSoftException = true;

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            jobs.Add(jobMic2);
            jobMic2.IsVoidedOnSoftException = true;

            Job jobMic3 = new Job();
            jobMic3.Name = "Mic 3";
            jobs.Add(jobMic3);
            jobMic3.IsVoidedOnSoftException = true;

            Job jobMic4 = new Job();
            jobMic4.Name = "Mic 4";
            jobs.Add(jobMic4);
            jobMic4.IsVoidedOnSoftException = true;

            jobMic1.AddSameJob(jobMic2);
            jobMic1.AddSameJob(jobMic3);
            jobMic1.AddSameJob(jobMic4);

            jobMic2.AddSameJob(jobMic3);
            jobMic2.AddSameJob(jobMic4);

            jobMic3.AddSameJob(jobMic4);


            // --------------------------------------------------------
            // USERS
            // --------------------------------------------------------

            HashSet<User> users = new HashSet<User>();

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userDLopez);

            User userEWilder = new User();
            userEWilder.Name = "Ethan Wilder";
            userEWilder.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userEWilder);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userRStubbs);

            // --------------------------------------------------------
            // TEMPLATES
            // --------------------------------------------------------

            Template templateSunday = new Template();
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };

            Template templateTuesday = new Template();
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2 };

            // --------------------------------------------------------
            // WORK
            // --------------------------------------------------------

            SoundBuilderV2 engine = new SoundBuilderV2.Builder()
                .SetJobs(jobs)
                .SetUser(users)
                .Build();

            // --------------------------------------------------------
            // WORK - Templates
            // --------------------------------------------------------

            DateTime nextTuesday = DateTime.Parse("12/30/2014");
            DateTime nextSunday = DateTime.Parse("1/4/2015");

            for (int i = 0; i < 9; i++) {
                engine.AddTemplate(nextTuesday.AddDays(7 * i), templateTuesday);
                engine.AddTemplate(nextSunday.AddDays(7 * i), templateSunday);
            }

            // --------------------------------------------------------
            // WORK - Previous Meetings
            // --------------------------------------------------------

            //Meeting meeting = templateTuesday.ToMeeting(DateTime.Parse("12/16/2014"));
            //meeting.AddUserForJob(userCTangen, jobSound);
            //meeting.AddUserForJob(userESavelberg, jobStage);
            //meeting.AddUserForJob(userDCook, jobMic1);
            //meeting.AddUserForJob(userEWilder, jobMic2);
            //engine.AddMeeting(meeting.Date, meeting);

            //meeting = templateSunday.ToMeeting(DateTime.Parse("12/21/2014"));
            //meeting.AddUserForJob(userESavelberg, jobSound);
            //meeting.AddUserForJob(userDLopez, jobStage);
            //meeting.AddUserForJob(userJHermosillo, jobMic1);
            //meeting.AddUserForJob(userCOldani, jobMic2);
            //meeting.AddUserForJob(userRStubbs, jobMic3);
            //meeting.AddUserForJob(userEWilder, jobMic4);
            //engine.AddMeeting(meeting.Date, meeting);

            //meeting = templateTuesday.ToMeeting(DateTime.Parse("12/23/2014"));
            //meeting.AddUserForJob(userDCook, jobSound);
            //meeting.AddUserForJob(userCTangen, jobStage);
            //meeting.AddUserForJob(userESavelberg, jobMic1);
            //meeting.AddUserForJob(userRStubbs, jobMic2);
            //engine.AddMeeting(meeting.Date, meeting);

            //meeting = templateSunday.ToMeeting(DateTime.Parse("12/28/2014"));
            //meeting.AddUserForJob(userJHermosillo, jobSound);
            //meeting.AddUserForJob(userEWilder, jobStage);
            //meeting.AddUserForJob(userDLopez, jobMic1);
            //meeting.AddUserForJob(userDCook, jobMic2);
            //meeting.AddUserForJob(userCOldani, jobMic3);
            //meeting.AddUserForJob(userKSugiyama, jobMic4);
            
            
            // --------------------------------------------------------
            // WORK - Exceptions
            // --------------------------------------------------------

            engine.AddException(DateTime.Parse("12/30/2014"), userDCook, true);
            engine.AddException(DateTime.Parse("12/30/2014"), userCTangen, true);
            engine.AddException(DateTime.Parse("1/25/2015"), userESavelberg, false);
            engine.AddException(DateTime.Parse("2/8/2015"), userESavelberg, false);
            engine.AddException(DateTime.Parse("1/13/2015"), userCTangen, true);
            engine.AddException(DateTime.Parse("1/20/2015"), userDCook, true);
            engine.AddException(DateTime.Parse("1/27/2015"), userCTangen, true);
            engine.AddException(DateTime.Parse("1/27/2015"), userESavelberg, true);
            engine.AddException(DateTime.Parse("2/15/2015"), userDLopez, true);
            engine.AddException(DateTime.Parse("3/1/2015"), userCTangen, false);
            engine.AddException(DateTime.Parse("3/3/2015"), userESavelberg, true);
            engine.AddException(DateTime.Parse("3/8/2015"), userDCook, false);
            engine.AddException(DateTime.Parse("3/8/2015"), userESavelberg, true);
            engine.AddException(DateTime.Parse("3/15/2015"), userRStubbs, true);
            engine.AddException(DateTime.Parse("3/23/2015"), userCTangen, false);
            engine.AddException(DateTime.Parse("4/12/2015"), userDCook, true);


            // --------------------------------------------------------
            // WORK - Preferences
            // --------------------------------------------------------

            engine.AddPreferenceNot(userRStubbs, templateTuesday);

            // --------------------------------------------------------
            // WORK - WORK
            // --------------------------------------------------------

            _meetings = engine.BuildSchedule();
            PopulateAllData();
        }

        private void PopulateAllData() {
            PopulateMeetings();
        }

        private void PopulateMeetings() {
            cboMeetings.Items.Clear();
            for (int i = 0; i < _meetings.Keys.Count(); i++) {
                Meeting meeting = _meetings.GetMeeting(_meetings.Keys.ElementAt(i));
                SelectItem item = new SelectItem();
                item.Text = meeting.Name;
                item.Value = i.ToString();
                cboMeetings.Items.Add(item);
            }
        }

        private void cboMeetings_SelectedIndexChanged(object sender, EventArgs e) {
            Meeting meeting = _meetings.GetMeeting(_meetings.Keys.ElementAt(cboMeetings.SelectedIndex));
            StringBuilder message = new StringBuilder();
            message.AppendLine("Meeting Name: " + meeting.Name);
            message.AppendLine("");
            for (int i = 0; i < meeting.Jobs.Count(); i++) {
                message.Append("Job: " + meeting.Jobs.ElementAt(i).Name);
                message.AppendLine(" (" + meeting.UserForJob(meeting.Jobs.ElementAt(i)).Name + ")");
            }
            txtResults.Text = message.ToString();
        }
    }

    public class SelectItem {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
