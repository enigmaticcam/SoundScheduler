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
        private delegate void UpdateResultsFinishedDelegate(int[] solution);
        private System.Diagnostics.Stopwatch _timer;
        private bool _stop;
        private List<Job> _jobs;
        private List<User> _users;
        private List<Meeting> _meetings;
        private List<JobConsideration> _jobConsiderations;
        private List<Template> _templates;
        private List<BestScoreEntry> _bestScores;

        public frmGenetic() {
            InitializeComponent();
        }

        private bool Results(Genetic.GeneticResults results) {
            txtResults.BeginInvoke(new UpdateResultsDelegate(UpdateResults), new object[] { results });
            return !_stop;
        }

        private void Finish(int[] solution) {
            _timer.Stop();
            txtResults.BeginInvoke(new UpdateResultsFinishedDelegate(UpdateResultsFinished), new object[] { solution });
        }

        private void UpdateResults(Genetic.GeneticResults results) {
            if (results.BestSolutionSoFarSolution != null) {
                string elapsedTime = _timer.Elapsed.Hours.ToString().PadLeft(2, '0') + ":" + _timer.Elapsed.Minutes.ToString().PadLeft(2, '0') + ":" + _timer.Elapsed.Seconds.ToString().PadLeft(2, '0');
                StringBuilder text = new StringBuilder();
                text.AppendLine("Best Solution So Far: " + IntArrayToString(results.BestSolutionSoFarSolution));
                text.AppendLine("Best Solution Score So Far: " + results.BestSolutionSoFarScore + "%");
                text.AppendLine("Generation Count: " + results.GenerationCount);
                text.AppendLine("Generations Per Second: " + results.GenerationsPerSecond);
                text.AppendLine("Elapsed Time: " + elapsedTime);
                text.AppendLine("Utilizing " + results.CPUCoreCount + " CPU Cores");
                text.Append(BestScoresSoFarText(results.BestSolutionSoFarScore, elapsedTime));
                txtResults.Text = text.ToString();
            }
        }

        private string BestScoresSoFarText(float scoreToBeat, string secondsSinceStart) {
            StringBuilder text = new StringBuilder();
            if (_bestScores.Count > 0) {
                if (scoreToBeat > _bestScores[0].Score) {
                    _bestScores.Insert(0, new BestScoreEntry(secondsSinceStart, scoreToBeat));
                }
                foreach (BestScoreEntry entry in _bestScores) {
                    text.AppendLine("");
                    text.AppendLine("Previous Best: " + entry.Score);
                    text.AppendLine("Best at: " + entry.TimeSinceStart);
                }
            } else {
                _bestScores.Insert(0, new BestScoreEntry(secondsSinceStart, scoreToBeat));
            }
            return text.ToString();
        }

        private string IntArrayToString(int[] intArray) {
            StringBuilder text = new StringBuilder();
            bool started = false;
            for (int i = 0; i <= intArray.GetUpperBound(0); i++) {
                if (!started) {
                    started = true;
                } else {
                    text.Append(",");
                }
                text.Append(intArray[i]);
            }
            return text.ToString();
        }

        private void UpdateResultsFinished(int[] solution) {
            int counter = 0;
            int day = 1;
            StringBuilder text = new StringBuilder();
            foreach (JobConsideration consideration in _jobConsiderations) {
                float score = consideration.IsValid(solution);
                if (score != 0) {
                    text.AppendLine("");
                    text.AppendLine(consideration.JobName + ": -" + score + " points");
                }
            }
            foreach (Meeting meeting in _meetings) {
                text.AppendLine("");
                text.AppendLine("Day " + day);
                text.AppendLine("Date: " + meeting.Date.ToShortDateString());
                foreach (Job job in meeting.Jobs) {
                    text.AppendLine("Job: " + job.Name + " User: " + _users[solution[counter]].Name);
                    counter++;
                }
                day++;
            }
            txtResults.Text += text.ToString();
        }

        private void cmdGo_Click(object sender, EventArgs e) {
            _stop = false;
            _bestScores = new List<BestScoreEntry>();

            // Arrange
            _jobs = new List<Job>();
            _users = new List<User>();
            _meetings = new List<Meeting>();
            _jobConsiderations = new List<JobConsideration>();
            _templates = new List<Template>();

            Job jobVideo = new Job();
            jobVideo.Name = "Video";
            _jobs.Add(jobVideo);
            jobVideo.IsVoidedOnSoftException = true;

            Job jobSound = new Job();
            jobSound.Name = "Sound Box";
            _jobs.Add(jobSound);
            jobSound.IsVoidedOnSoftException = true;

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            _jobs.Add(jobStage);
            jobStage.IsVoidedOnSoftException = false;

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            _jobs.Add(jobMic1);
            jobMic1.IsVoidedOnSoftException = true;

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            _jobs.Add(jobMic2);
            jobMic2.IsVoidedOnSoftException = true;

            Job jobMic3 = new Job();
            jobMic3.Name = "Mic 3";
            _jobs.Add(jobMic3);
            jobMic3.IsVoidedOnSoftException = true;

            Job jobMic4 = new Job();
            jobMic4.Name = "Mic 4";
            _jobs.Add(jobMic4);
            jobMic4.IsVoidedOnSoftException = true;

            Job jobMicSub1 = new Job();
            jobMicSub1.Name = "Mic Sub 1";
            _jobs.Add(jobMicSub1);
            jobMicSub1.IsVoidedOnSoftException = true;

            Job jobMicSub2 = new Job();
            jobMicSub2.Name = "Mic Sub 2";
            _jobs.Add(jobMicSub2);
            jobMicSub2.IsVoidedOnSoftException = true;

            jobMic1.AddSameJob(jobMic2);
            jobMic1.AddSameJob(jobMic3);
            jobMic1.AddSameJob(jobMic4);
            jobMic2.AddSameJob(jobMic3);
            jobMic2.AddSameJob(jobMic4);
            jobMic3.AddSameJob(jobMic4);

            jobMicSub1.AddSameJob(jobMicSub2);

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobVideo, jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userDLopez);

            User userEWilder = new User();
            userEWilder.Name = "Ethan Wilder";
            userEWilder.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userEWilder);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userRStubbs);

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userDKeil);

            User userBDeaver = new User();
            userBDeaver.Name = "Beau Deaver";
            userBDeaver.Jobs = new List<Job>();
            _users.Add(userBDeaver);

            User userBTyler = new User();
            userBTyler.Name = "Bobby Tyler";
            userBTyler.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userBTyler);

            User userBBabbe = new User();
            userBBabbe.Name = "Bob Babbe";
            userBBabbe.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userBBabbe);

            User userDBecker = new User();
            userDBecker.Name = "Dave Becker";
            userDBecker.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userDBecker);

            User userMPowers = new User();
            userMPowers.Name = "Mike Powers";
            userMPowers.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobMicSub1, jobMicSub2 };
            _users.Add(userMPowers);

            Template templateSunday = new Template(2);
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            templateSunday.AddJobToAllPartitions(jobSound);
            templateSunday.AddJobToAllPartitions(jobStage);
            templateSunday.AddJobToPartition(jobMic1, 2);
            templateSunday.AddJobToPartition(jobMic2, 2);
            templateSunday.AddJobToPartition(jobMic3, 2);
            templateSunday.AddJobToPartition(jobMic4, 2);

            Template templateTuesday = new Template(2);
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            templateTuesday.AddJobToAllPartitions(jobSound);
            templateTuesday.AddJobToAllPartitions(jobStage);
            templateTuesday.AddJobToPartition(jobMic1, 1);
            templateTuesday.AddJobToPartition(jobMic2, 1);
            templateTuesday.AddJobToPartition(jobMic3, 2);
            templateTuesday.AddJobToPartition(jobMic4, 2);

            Template templateTuesdayWithVideo = new Template();
            templateTuesdayWithVideo.Name = "Tuesday";
            templateTuesdayWithVideo.Jobs = new List<Job> { jobVideo, jobSound, jobStage, jobMic1, jobMic2 };
            templateTuesdayWithVideo.AddJobToAllPartitions(jobVideo);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobSound);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobStage);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobMic1);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobMic2);
            
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("12/27/2015")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/29/2015")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("1/3/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("1/5/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("1/10/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("1/12/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("1/17/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("1/19/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("1/24/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("1/26/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("1/31/2016")));

            foreach (Meeting meeting in _meetings) {
                _templates.Add(meeting.ToTemplate());
            }

            _meetings[0].AddUserForJob(userEWilder, jobSound);
            _meetings[0].AddUserForJob(userDLopez, jobStage);
            _meetings[0].AddUserForJob(userBBabbe, jobMic1);
            _meetings[0].AddUserForJob(userMPowers, jobMic2);
            _meetings[0].AddUserForJob(userESavelberg, jobMic3);
            _meetings[0].AddUserForJob(userDCook, jobMic4);

            _meetings[1].AddUserForJob(userDCook, jobSound);
            _meetings[1].AddUserForJob(userEWilder, jobStage);
            _meetings[1].AddUserForJob(userRStubbs, jobMic1);
            _meetings[1].AddUserForJob(userBBabbe, jobMic2);
            _meetings[1].AddUserForJob(userBTyler, jobMic3);
            _meetings[1].AddUserForJob(userDKeil, jobMic4);

            _meetings[2].AddUserForJob(userESavelberg, jobSound);
            _meetings[2].AddUserForJob(userDCook, jobStage);
            _meetings[2].AddUserForJob(userRStubbs, jobMic1);
            _meetings[2].AddUserForJob(userDKeil, jobMic2);
            _meetings[2].AddUserForJob(userEWilder, jobMic3);
            _meetings[2].AddUserForJob(userMPowers, jobMic4);

            JobConsideration consideration = null;

            consideration = new JobConsiderationEvenUserDistributionPerJob.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoDidSameJobLastMeeting.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationGiveUsersABreak.Builder()
                .SetGiveBreakOnDay(4)
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoAlreadyHaveJobOld.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .Build();

            // Stage and video can still be run if you have a discussion part already
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddJobToException(jobStage, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddJobToException(jobVideo, (float)0.5);

            // Any job can be done at the same time as video
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddAllJobCombos(jobVideo, (float)0.5);

            // Sound cannot be done the same time as stage and mic. Stage can be done the same time as mic. Mic cannot be done the same time as other mic.
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobSound, jobStage, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobSound, jobMic1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobSound, jobMic2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobSound, jobMic3, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobSound, jobMic4, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobStage, jobMic1, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobStage, jobMic2, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobStage, jobMic3, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobStage, jobMic4, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobMic1, jobMic2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobMic1, jobMic3, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobMic1, jobMic4, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobMic2, jobMic3, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobMic2, jobMic4, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddSingleJobCombo(jobMic3, jobMic4, 1);

            // Who's partially or completely unavailable.
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(3, _users.IndexOf(userRStubbs), (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(3, _users.IndexOf(userMPowers), (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(5, _users.IndexOf(userCTangen), (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(5, _users.IndexOf(userMPowers), (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(7, _users.IndexOf(userRStubbs), (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(9, _users.IndexOf(userBBabbe), (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(4, _users.IndexOf(userDBecker), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(4, _users.IndexOf(userDCook), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(6, _users.IndexOf(userBBabbe), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(6, _users.IndexOf(userRStubbs), (float)0.5, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(6, _users.IndexOf(userMPowers), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(8, _users.IndexOf(userMPowers), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(8, _users.IndexOf(userDBecker), (float)0.5, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(8, _users.IndexOf(userESavelberg), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(10, _users.IndexOf(userDLopez), (float)0.5, 2);
            ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(10, _users.IndexOf(userCTangen), (float)0.5, 2);

            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoCantDoJob.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .Build();
            _jobConsiderations.Add(consideration);
            
            SoundBuilderV3.ActionFillMeetingsAll action = new SoundBuilderV3.ActionFillMeetingsAll.Builder()
                .SetJobConsiderations(_jobConsiderations)
                .SetMeetings(_meetings)
                .SetUsers(_users)
                .SetResultsFunc(Results)
                .SetSolutionAction(Finish)
                .SetChromosomeCount(Convert.ToInt32(txtPopulation.Text))
                .SetMutationRate(Convert.ToInt32(txtMutationRate.Text))
                .SetThreadCount(Convert.ToInt32(txtThreadCount.Text))
                .Build();
            action.PerformAction();

            _timer = new System.Diagnostics.Stopwatch();
            _timer.Start();
        }

        private void cmdStop_Click(object sender, EventArgs e) {
            _stop = true;
        }

        private class BestScoreEntry {
            private string _timeSinceStart;
            public string TimeSinceStart {
                get { return _timeSinceStart; }
            }

            private float _score;
            public float Score {
                get { return _score; }
            }

            public BestScoreEntry(string timeSinceStart, float score) {
                _timeSinceStart = timeSinceStart;
                _score = score;
            }
        }

        private void frmGenetic_Load(object sender, EventArgs e) {
            txtThreadCount.Text = Environment.ProcessorCount.ToString();
            lblProcessorCount.Text = Environment.ProcessorCount + " processors detected";
        }
    }
}
