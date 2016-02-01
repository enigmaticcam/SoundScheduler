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
        private List<BestScoreEntry> _analysisBestScores;
        private bool _isAnalyzing;
        private int _analsysisTimeInSeconds = 180;
        private int _analsysisTriesBeforeFinish = 10;

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
            string elapsedTime = _timer.Elapsed.Hours.ToString().PadLeft(2, '0') + ":" + _timer.Elapsed.Minutes.ToString().PadLeft(2, '0') + ":" + _timer.Elapsed.Seconds.ToString().PadLeft(2, '0');
            if (!_isAnalyzing) {
                if (results.BestSolutionSoFarSolution != null) {
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
            } else {
                StringBuilder text = new StringBuilder();
                if (_analysisBestScores.Count > 0) {
                    text.AppendLine(BestOfAnalysisScores());
                }
                text.AppendLine("Elapsed Time: " + elapsedTime);
                text.AppendLine("Best Solution Score So Far: " + results.BestSolutionSoFarScore + "%");
                text.AppendLine(BestScoresSoFarText(results.BestSolutionSoFarScore, elapsedTime));
                txtResults.Text = text.ToString();
                if (_timer.Elapsed.TotalSeconds >= _analsysisTimeInSeconds && !_stop) {
                    _stop = true;
                    _analysisBestScores.Add(_bestScores[0]);
                }
            }
        }

        private string BestOfAnalysisScores() {
            StringBuilder text = new StringBuilder();
            for (int index = 0; index < _analysisBestScores.Count; index++) {
                text.AppendLine("Round " + (index + 1).ToString() + ": " + _analysisBestScores[index].Score + "% at " + _analysisBestScores[index].TimeSinceStart);
            }
            return text.ToString();
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
            if (!_isAnalyzing) {
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
            } else {
                if (_analysisBestScores.Count < _analsysisTriesBeforeFinish) {
                    PerformAnalysis(true);
                }
            }
        }

        private void cmdGo_Click(object sender, EventArgs e) {
            _isAnalyzing = false;
            _stop = false;
            _bestScores = new List<BestScoreEntry>();

            Build();
            
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

        private void cmdAnalyze_Click(object sender, EventArgs e) {
            PerformAnalysis(false);
        }

        private void PerformAnalysis(bool continued) {
            _isAnalyzing = true;
            _stop = false;
            _bestScores = new List<BestScoreEntry>();
            if (!continued) {
                _analysisBestScores = new List<BestScoreEntry>();
            }
            Build();

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

        private void Build() {
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

            Job jobSubstitute = new Job();
            jobSubstitute.Name = "Substitute";
            _jobs.Add(jobSubstitute);
            jobSubstitute.IsVoidedOnSoftException = true;

            jobMic1.AddSameJob(jobMic2);
            jobMic1.AddSameJob(jobMic3);
            jobMic1.AddSameJob(jobMic4);
            jobMic2.AddSameJob(jobMic3);
            jobMic2.AddSameJob(jobMic4);
            jobMic3.AddSameJob(jobMic4);

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobVideo, jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute };
            _users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute };
            _users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute };
            _users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute };
            _users.Add(userDLopez);

            User userEWilder = new User();
            userEWilder.Name = "Ethan Wilder";
            userEWilder.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute };
            _users.Add(userEWilder);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userRStubbs);

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userDKeil);

            User userBDeaver = new User();
            userBDeaver.Name = "Beau Deaver";
            userBDeaver.Jobs = new List<Job>();
            _users.Add(userBDeaver);

            User userBTyler = new User();
            userBTyler.Name = "Bobby Tyler";
            userBTyler.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userBTyler);

            User userBBabbe = new User();
            userBBabbe.Name = "Bob Babbe";
            userBBabbe.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userBBabbe);

            User userDBecker = new User();
            userDBecker.Name = "Dave Becker";
            userDBecker.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userDBecker);

            User userMPowers = new User();
            userMPowers.Name = "Mike Powers";
            userMPowers.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
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

            Template templateTuesdayWithVideo = new Template(2);
            templateTuesdayWithVideo.Name = "Tuesday";
            templateTuesdayWithVideo.Jobs = new List<Job> { jobVideo, jobSound, jobStage, jobMic1, jobMic2, jobSubstitute };
            templateTuesdayWithVideo.AddJobToAllPartitions(jobVideo);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobSound);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobStage);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobMic1);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobMic2);
            templateTuesdayWithVideo.AddJobToAllPartitions(jobSubstitute);

            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("12/27/2015")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/29/2015")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("01/03/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("01/05/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("01/10/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("01/12/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("01/17/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("01/19/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("01/24/2016")));
            _meetings.Add(templateTuesdayWithVideo.ToMeeting(DateTime.Parse("01/26/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("01/31/2016")));


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

            UserExceptionDictionary exceptions = new UserExceptionDictionary();

            JobConsideration consideration = null;

            consideration = new JobConsiderationEvenUserDistributionPerJob.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoDidSameJobLastMeeting.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationGiveUsersABreak.Builder()
                .SetGiveBreakOnDay(4)
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            // Chairman can do still do video, stage, mics, and substitute
            UserExceptionType chairman = new UserExceptionType();
            chairman.AddJobExceptionValue(jobVideo, (float)0.5);
            chairman.AddJobExceptionValue(jobSound, 1);
            chairman.AddJobExceptionValue(jobStage, (float)0.5);
            chairman.AddJobExceptionValue(jobMic1, (float)0.5);
            chairman.AddJobExceptionValue(jobMic2, (float)0.5);
            chairman.AddJobExceptionValue(jobMic3, (float)0.5);
            chairman.AddJobExceptionValue(jobMic4, (float)0.5);
            chairman.AddJobExceptionValue(jobSubstitute, (float)0.5);

            // Chairman will require substitute availability if they do sound
            chairman.AddSubRequiresAvailability(jobVideo, false);
            chairman.AddSubRequiresAvailability(jobSound, true);
            chairman.AddSubRequiresAvailability(jobStage, false);
            chairman.AddSubRequiresAvailability(jobMic1, false);
            chairman.AddSubRequiresAvailability(jobMic2, false);
            chairman.AddSubRequiresAvailability(jobMic3, false);
            chairman.AddSubRequiresAvailability(jobMic4, false);

            // Discussion can still do video, stage, and substitute
            UserExceptionType discussionWithoutVideo = new UserExceptionType();
            discussionWithoutVideo.AddJobExceptionValue(jobVideo, (float)0.5);
            discussionWithoutVideo.AddJobExceptionValue(jobSound, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobStage, (float)0.5);
            discussionWithoutVideo.AddJobExceptionValue(jobMic1, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobMic2, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobMic3, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobMic4, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobSubstitute, (float)0.5);

            // Discussion will require substitute availability if they do sound or mics
            discussionWithoutVideo.AddSubRequiresAvailability(jobVideo, false);
            discussionWithoutVideo.AddSubRequiresAvailability(jobSound, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobStage, false);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic1, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic2, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic3, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic4, true);

            // Discussion with video can't do anything but substitute
            UserExceptionType discussionWithVideo = new UserExceptionType();
            discussionWithVideo.AddJobExceptionValue(jobVideo, 1);
            discussionWithVideo.AddJobExceptionValue(jobSound, 1);
            discussionWithVideo.AddJobExceptionValue(jobStage, 1);
            discussionWithVideo.AddJobExceptionValue(jobMic1, 1);
            discussionWithVideo.AddJobExceptionValue(jobMic2, 1);
            discussionWithVideo.AddJobExceptionValue(jobMic3, 1);
            discussionWithVideo.AddJobExceptionValue(jobMic4, 1);
            discussionWithVideo.AddJobExceptionValue(jobSubstitute, (float)0.5);

            // Discussion with video will require substitute availability for all jobs
            discussionWithVideo.AddSubRequiresAvailability(jobVideo, true);
            discussionWithVideo.AddSubRequiresAvailability(jobSound, true);
            discussionWithVideo.AddSubRequiresAvailability(jobStage, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic1, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic2, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic3, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic4, true);

            // Talk can do all by sound
            UserExceptionType talkWithoutVideo = new UserExceptionType();
            talkWithoutVideo.AddJobExceptionValue(jobVideo, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobSound, 1);
            talkWithoutVideo.AddJobExceptionValue(jobStage, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic1, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic2, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic3, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic4, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobSubstitute, (float)0.5);

            // Talk will require substitute availability only for sound
            talkWithoutVideo.AddSubRequiresAvailability(jobVideo, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobSound, true);
            talkWithoutVideo.AddSubRequiresAvailability(jobStage, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic1, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic2, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic3, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic4, false);

            // Talk with video can only do mics and substitute
            UserExceptionType talkWithVideo = new UserExceptionType();
            talkWithVideo.AddJobExceptionValue(jobVideo, 1);
            talkWithVideo.AddJobExceptionValue(jobSound, 1);
            talkWithVideo.AddJobExceptionValue(jobStage, 1);
            talkWithVideo.AddJobExceptionValue(jobMic1, (float)0.5);
            talkWithVideo.AddJobExceptionValue(jobMic2, (float)0.5);
            talkWithVideo.AddJobExceptionValue(jobMic3, (float)0.5);
            talkWithVideo.AddJobExceptionValue(jobMic4, (float)0.5);
            talkWithVideo.AddJobExceptionValue(jobSubstitute, (float)0.5);

            // Talk with video requires substitute availability for all but mics
            talkWithVideo.AddSubRequiresAvailability(jobVideo, true);
            talkWithVideo.AddSubRequiresAvailability(jobSound, true);
            talkWithVideo.AddSubRequiresAvailability(jobStage, true);
            talkWithVideo.AddSubRequiresAvailability(jobMic1, false);
            talkWithVideo.AddSubRequiresAvailability(jobMic2, false);
            talkWithVideo.AddSubRequiresAvailability(jobMic3, false);
            talkWithVideo.AddSubRequiresAvailability(jobMic4, false);

            // Absentee can't do any job. They ain't there!
            UserExceptionType absent = new UserExceptionType();
            absent.AddJobExceptionValue(jobVideo, 1);
            absent.AddJobExceptionValue(jobSound, 1);
            absent.AddJobExceptionValue(jobStage, 1);
            absent.AddJobExceptionValue(jobMic1, 1);
            absent.AddJobExceptionValue(jobMic2, 1);
            absent.AddJobExceptionValue(jobMic3, 1);
            absent.AddJobExceptionValue(jobMic4, 1);
            absent.AddJobExceptionValue(jobSubstitute, 1);

            // Absentee requires substitute availability for all jobs. They ain't there!
            absent.AddSubRequiresAvailability(jobVideo, true);
            absent.AddSubRequiresAvailability(jobSound, true);
            absent.AddSubRequiresAvailability(jobStage, true);
            absent.AddSubRequiresAvailability(jobMic1, true);
            absent.AddSubRequiresAvailability(jobMic2, true);
            absent.AddSubRequiresAvailability(jobMic3, true);
            absent.AddSubRequiresAvailability(jobMic4, true);

            // Attendants can't do any other job
            UserExceptionType attendant = new UserExceptionType();
            attendant.AddJobExceptionValue(jobVideo, 1);
            attendant.AddJobExceptionValue(jobSound, 1);
            attendant.AddJobExceptionValue(jobStage, 1);
            attendant.AddJobExceptionValue(jobMic1, 1);
            attendant.AddJobExceptionValue(jobMic2, 1);
            attendant.AddJobExceptionValue(jobMic3, 1);
            attendant.AddJobExceptionValue(jobMic4, 1);
            attendant.AddJobExceptionValue(jobSubstitute, 1);

            // Attendants require substitute availability for all jobs
            attendant.AddSubRequiresAvailability(jobVideo, true);
            attendant.AddSubRequiresAvailability(jobSound, true);
            attendant.AddSubRequiresAvailability(jobStage, true);
            attendant.AddSubRequiresAvailability(jobMic1, true);
            attendant.AddSubRequiresAvailability(jobMic2, true);
            attendant.AddSubRequiresAvailability(jobMic3, true);
            attendant.AddSubRequiresAvailability(jobMic4, true);

            // No job can be done at the same time as substitute
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobSubstitute, 1);

            // No job can be done at the same time as video
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobVideo, 1);

            // Sound cannot be done the same time as stage and mic. Stage can be done the same time as mic. Mic cannot be done the same time as other mic.
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobSound, jobStage, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobSound, jobMic1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobSound, jobMic2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobSound, jobMic3, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobSound, jobMic4, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobStage, jobMic1, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobStage, jobMic2, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobStage, jobMic3, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobStage, jobMic4, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic1, jobMic2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic1, jobMic3, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic1, jobMic4, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic2, jobMic3, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic2, jobMic4, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic3, jobMic4, 1);

            // Who's partially or completely unavailable
            exceptions.AddUserException(attendant, _users.IndexOf(userRStubbs), 3, 1);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 3, templateTuesdayWithVideo);
            exceptions.AddUserException(attendant, _users.IndexOf(userCTangen), 5, 1);
            exceptions.AddUserException(attendant, _users.IndexOf(userMPowers), 5, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userRStubbs), 7, 1);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 9, templateTuesdayWithVideo);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userCTangen), 2, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userBBabbe), 2, 1);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userDBecker), 4, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userDCook), 4, 2);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userBBabbe), 6, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userRStubbs), 6, 1);
            exceptions.AddUserException(attendant, _users.IndexOf(userMPowers), 6, 2);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userMPowers), 8, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userDBecker), 8, 1);
            exceptions.AddUserException(attendant, _users.IndexOf(userESavelberg), 8, 2);
            exceptions.AddUserException(chairman, _users.IndexOf(userMPowers), 10, 1);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userDLopez), 10, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userCTangen), 10, 2);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 4, _templates[4]);

            consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            // Who needs to be substituted
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userBBabbe), 1, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userESavelberg), 1, discussionWithVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userDBecker), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userCTangen), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userDCook), 2, discussionWithVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userDLopez), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userESavelberg), 1, talkWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userDBecker), 2, talkWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userCTangen), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userMPowers), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userDKeil), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDCook), 1, talkWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userMPowers), 1, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userRStubbs), 2, discussionWithVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDBecker), 2, discussionWithoutVideo);

            consideration = new JobConsiderationUsersWhoHaveExceptions.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoCantDoJob.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .Build();
            _jobConsiderations.Add(consideration);

            // Debug
            //List<int> solutionAsList = new List<int> { 4, 3, 9, 11, 1, 2, 2, 4, 5, 9, 8, 6, 1, 2, 5, 6, 4, 11, 0, 3, 1, 11, 5, 2, 0, 4, 8, 10, 11, 9, 0, 2, 3, 6, 10, 1, 4, 2, 10, 9, 3, 5, 0, 1, 4, 9, 6, 2, 4, 0, 8, 11, 6, 10, 0, 2, 1, 11, 8, 3, 1, 3, 6, 5, 10, 8 };
            //int[] solutionAsArray = solutionAsList.ToArray();

            //foreach (JobConsideration jobConsideration in _jobConsiderations) {
            //    float score = jobConsideration.IsValid(solutionAsArray);
            //}
        }
    }
}
