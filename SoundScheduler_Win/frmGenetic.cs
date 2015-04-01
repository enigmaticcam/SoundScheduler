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
    public partial class frmGenetic : Form {
        private delegate void UpdateResultsDelegate(Genetic.GeneticResults results);

        public frmGenetic() {
            InitializeComponent();
        }

        private bool Results(Genetic.GeneticResults results) {
            txtResults.BeginInvoke(new UpdateResultsDelegate(UpdateResults), new object[] { results });
            return false;
        }

        private void Finish(int[] solution) {

        }

        private void UpdateResults(Genetic.GeneticResults results) {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Best Solution So Far: " + results.BestSolutionSoFarSolution);
            text.AppendLine("Best Solution Score So Far: " + results.BestSolutionSoFarScore);
            text.AppendLine("Generation Count: " + results.GenerationCount);
            text.AppendLine("Generations Per Second: " + results.GenerationsPerSecond);
            txtResults.Text = text.ToString();
        }

        private void cmdGo_Click(object sender, EventArgs e) {
            // Arrange
            List<Job> jobs = new List<Job>();
            List<User> users = new List<User>();
            List<Meeting> meetings = new List<Meeting>();
            List<JobConsideration> jobConsiderations = new List<JobConsideration>();
            List<Template> templates = new List<Template>();

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

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userDKeil);

            User userBDeaver = new User();
            userBDeaver.Name = "Beau Deaver";
            userBDeaver.Jobs = new List<Job> { jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userBDeaver);

            Meeting.Data templateSunday = new Meeting.Data();
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };

            Meeting.Data templateTuesday = new Meeting.Data();
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2 };

            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));
            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));
            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));
            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));

            foreach (Meeting meeting in meetings) {
                templates.Add(meeting.ToTemplate());
            }

            meetings[7].AddUserForJob(userCTangen, jobSound);

            JobConsideration consideration = new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoCantDoJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            consideration = new JobConsiderationEvenUserDistributionPerJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            // Act
            SoundBuilderV3.ActionFillMeetingsAll action = new SoundBuilderV3.ActionFillMeetingsAll.Builder()
                .SetJobConsiderations(jobConsiderations)
                .SetMeetings(meetings)
                .SetUsers(users)
                .SetResultsFunc(Results)
                .SetSolutionAction(Finish)
                .Build();
            action.PerformAction();
        }
    }
}
