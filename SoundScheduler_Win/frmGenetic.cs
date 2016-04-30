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
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobVideo);

            Job jobSound = new Job();
            jobSound.Name = "Sound Box";
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobSound);

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobStage);

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobMic1);

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobMic2);

            Job jobMic3 = new Job();
            jobMic3.Name = "Mic 3";
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobMic3);

            Job jobMic4 = new Job();
            jobMic4.Name = "Mic 4";
            jobVideo.CanBeSubstituded = true;
            _jobs.Add(jobMic4);

            Job jobSubstitute = new Job();
            jobSubstitute.Name = "Substitute";
            jobVideo.CanBeSubstituded = false;
            _jobs.Add(jobSubstitute);

            Job jobAttendant1 = new Job();
            jobAttendant1.Name = "Attendant 1";
            jobVideo.CanBeSubstituded = false;
            _jobs.Add(jobAttendant1);

            Job jobAttendant2 = new Job();
            jobAttendant2.Name = "Attendant 2";
            jobVideo.CanBeSubstituded = false;
            _jobs.Add(jobAttendant2);

            jobMic1.AddSameJob(jobMic2);
            jobMic1.AddSameJob(jobMic3);
            jobMic1.AddSameJob(jobMic4);
            jobMic2.AddSameJob(jobMic3);
            jobMic2.AddSameJob(jobMic4);
            jobMic3.AddSameJob(jobMic4);
            jobAttendant1.AddSameJob(jobAttendant2);

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobVideo, jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute, jobAttendant1, jobAttendant2 };
            _users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute, jobAttendant1, jobAttendant2 };
            _users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute, jobAttendant1, jobAttendant2 };
            _users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute };
            _users.Add(userDLopez);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobAttendant1, jobAttendant2 };
            _users.Add(userRStubbs);

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userDKeil);

            User userBTyler = new User();
            userBTyler.Name = "Bobby Tyler";
            userBTyler.Jobs = new List<Job> { jobSound, jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userBTyler);

            User userBBabbe = new User();
            userBBabbe.Name = "Bob Babbe";
            userBBabbe.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userBBabbe);

            User userDBecker = new User();
            userDBecker.Name = "Dave Becker";
            userDBecker.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobAttendant1, jobAttendant2 };
            _users.Add(userDBecker);

            User userMPowers = new User();
            userMPowers.Name = "Mike Powers";
            userMPowers.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobAttendant1, jobAttendant2 };
            _users.Add(userMPowers);

            User userGSimmons = new User();
            userGSimmons.Name = "George Simmons";
            userGSimmons.Jobs = new List<Job> { jobAttendant1, jobAttendant2 };
            _users.Add(userGSimmons);

            User userKLogan = new User();
            userKLogan.Name = "Ken Logan";
            userKLogan.Jobs = new List<Job> { jobAttendant1, jobAttendant2 };
            _users.Add(userKLogan);

            User userMBrock = new User();
            userMBrock.Name = "Mike Brock";
            userMBrock.Jobs = new List<Job> { jobAttendant1, jobAttendant2 };
            _users.Add(userMBrock);

            User userTSavelberg = new User();
            userTSavelberg.Name = "Tom Savelberg";
            userTSavelberg.Jobs = new List<Job> { jobAttendant1, jobAttendant2 };
            _users.Add(userTSavelberg);

            Template templateSunday = new Template(2);
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2 };
            templateSunday.AddJobToAllPartitions(jobSound);
            templateSunday.AddJobToAllPartitions(jobStage);
            templateSunday.AddJobToPartition(jobMic1, 2);
            templateSunday.AddJobToPartition(jobMic2, 2);

            Template templateTuesday = new Template(2);
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobSubstitute, jobAttendant1, jobAttendant2 };
            templateTuesday.AddJobToAllPartitions(jobVideo);
            templateTuesday.AddJobToAllPartitions(jobSound);
            templateTuesday.AddJobToAllPartitions(jobStage);
            templateTuesday.AddJobToAllPartitions(jobMic1);
            templateTuesday.AddJobToAllPartitions(jobMic2);
            templateTuesday.AddJobToAllPartitions(jobSubstitute);
            templateTuesday.AddJobToPartition(jobAttendant1, 1);
            templateTuesday.AddJobToPartition(jobAttendant2, 2);

            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("05/03/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/08/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("05/10/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/15/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("05/17/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/22/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("05/24/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/29/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("05/31/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/05/2016")));


            foreach (Meeting meeting in _meetings) {
                _templates.Add(meeting.ToTemplate());
            }

            //_meetings[0].AddUserForJob(userESavelberg, jobSound);
            //_meetings[0].AddUserForJob(userDLopez, jobStage);
            //_meetings[0].AddUserForJob(userEWilder, jobMic1);
            //_meetings[0].AddUserForJob(userDBecker, jobMic2);
            //_meetings[0].AddUserForJob(userCTangen, jobMic3);
            //_meetings[0].AddUserForJob(userBBabbe, jobMic4);

            //_meetings[1].AddUserForJob(userDLopez, jobSound);
            //_meetings[1].AddUserForJob(userESavelberg, jobStage);
            //_meetings[1].AddUserForJob(userBTyler, jobMic1);
            //_meetings[1].AddUserForJob(userBBabbe, jobMic2);
            //_meetings[1].AddUserForJob(userDCook, jobSubstitute);

            //_meetings[2].AddUserForJob(userEWilder, jobSound);
            //_meetings[2].AddUserForJob(userDCook, jobStage);
            //_meetings[2].AddUserForJob(userBBabbe, jobMic1);
            //_meetings[2].AddUserForJob(userDBecker, jobMic2);
            //_meetings[2].AddUserForJob(userBTyler, jobMic3);
            //_meetings[2].AddUserForJob(userRStubbs, jobMic4);

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

            // Chairman can do still do video, stage, mics, substitute. Can't do attendant or sound.
            UserExceptionType chairman = new UserExceptionType();
            chairman.AddJobExceptionValue(jobVideo, (float)0.5);
            chairman.AddJobExceptionValue(jobSound, 1);
            chairman.AddJobExceptionValue(jobStage, (float)0.5);
            chairman.AddJobExceptionValue(jobMic1, (float)0.5);
            chairman.AddJobExceptionValue(jobMic2, (float)0.5);
            chairman.AddJobExceptionValue(jobMic3, (float)0.5);
            chairman.AddJobExceptionValue(jobMic4, (float)0.5);
            chairman.AddJobExceptionValue(jobSubstitute, (float)0.5);
            chairman.AddJobExceptionValue(jobAttendant1, 1);
            chairman.AddJobExceptionValue(jobAttendant2, 1);

            // Chairman will require substitute availability if they do sound or attendant
            chairman.AddSubRequiresAvailability(jobVideo, false);
            chairman.AddSubRequiresAvailability(jobSound, true);
            chairman.AddSubRequiresAvailability(jobStage, false);
            chairman.AddSubRequiresAvailability(jobMic1, false);
            chairman.AddSubRequiresAvailability(jobMic2, false);
            chairman.AddSubRequiresAvailability(jobMic3, false);
            chairman.AddSubRequiresAvailability(jobMic4, false);
            chairman.AddSubRequiresAvailability(jobAttendant1, true);
            chairman.AddSubRequiresAvailability(jobAttendant2, true);

            // Discussion can still do video, stage, and substitute. Not attendant
            UserExceptionType discussionWithoutVideo = new UserExceptionType();
            discussionWithoutVideo.AddJobExceptionValue(jobVideo, (float)0.5);
            discussionWithoutVideo.AddJobExceptionValue(jobSound, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobStage, (float)0.5);
            discussionWithoutVideo.AddJobExceptionValue(jobMic1, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobMic2, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobMic3, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobMic4, 1);
            discussionWithoutVideo.AddJobExceptionValue(jobSubstitute, (float)0.5);
            discussionWithoutVideo.AddJobExceptionValue(jobAttendant1, 1);

            // Discussion will require substitute availability if they do sound, mics, or attendant
            discussionWithoutVideo.AddSubRequiresAvailability(jobVideo, false);
            discussionWithoutVideo.AddSubRequiresAvailability(jobSound, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobStage, false);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic1, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic2, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic3, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobMic4, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobAttendant1, true);
            discussionWithoutVideo.AddSubRequiresAvailability(jobAttendant2, true);

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
            discussionWithVideo.AddJobExceptionValue(jobAttendant1, 1);
            discussionWithVideo.AddJobExceptionValue(jobAttendant2, 1);

            // Discussion with video will require substitute availability for all jobs
            discussionWithVideo.AddSubRequiresAvailability(jobVideo, true);
            discussionWithVideo.AddSubRequiresAvailability(jobSound, true);
            discussionWithVideo.AddSubRequiresAvailability(jobStage, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic1, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic2, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic3, true);
            discussionWithVideo.AddSubRequiresAvailability(jobMic4, true);
            discussionWithVideo.AddSubRequiresAvailability(jobAttendant1, true);
            discussionWithVideo.AddSubRequiresAvailability(jobAttendant2, true);

            // Talk can do all but sound and attendant
            UserExceptionType talkWithoutVideo = new UserExceptionType();
            talkWithoutVideo.AddJobExceptionValue(jobVideo, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobSound, 1);
            talkWithoutVideo.AddJobExceptionValue(jobStage, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic1, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic2, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic3, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobMic4, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobSubstitute, (float)0.5);
            talkWithoutVideo.AddJobExceptionValue(jobAttendant1, 1);
            talkWithoutVideo.AddJobExceptionValue(jobAttendant2, 1);

            // Talk will require substitute availability only for sound and attendant
            talkWithoutVideo.AddSubRequiresAvailability(jobVideo, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobSound, true);
            talkWithoutVideo.AddSubRequiresAvailability(jobStage, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic1, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic2, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic3, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobMic4, false);
            talkWithoutVideo.AddSubRequiresAvailability(jobAttendant1, true);
            talkWithoutVideo.AddSubRequiresAvailability(jobAttendant2, true);

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
            talkWithVideo.AddJobExceptionValue(jobAttendant1, 1);
            talkWithVideo.AddJobExceptionValue(jobAttendant2, 1);

            // Talk with video requires substitute availability for all but mics
            talkWithVideo.AddSubRequiresAvailability(jobVideo, true);
            talkWithVideo.AddSubRequiresAvailability(jobSound, true);
            talkWithVideo.AddSubRequiresAvailability(jobStage, true);
            talkWithVideo.AddSubRequiresAvailability(jobMic1, false);
            talkWithVideo.AddSubRequiresAvailability(jobMic2, false);
            talkWithVideo.AddSubRequiresAvailability(jobMic3, false);
            talkWithVideo.AddSubRequiresAvailability(jobMic4, false);
            talkWithVideo.AddSubRequiresAvailability(jobAttendant1, true);
            talkWithVideo.AddSubRequiresAvailability(jobAttendant2, true);

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
            absent.AddJobExceptionValue(jobAttendant1, 1);
            absent.AddJobExceptionValue(jobAttendant2, 1);

            // Absentee requires substitute availability for all jobs. They ain't there!
            absent.AddSubRequiresAvailability(jobVideo, true);
            absent.AddSubRequiresAvailability(jobSound, true);
            absent.AddSubRequiresAvailability(jobStage, true);
            absent.AddSubRequiresAvailability(jobMic1, true);
            absent.AddSubRequiresAvailability(jobMic2, true);
            absent.AddSubRequiresAvailability(jobMic3, true);
            absent.AddSubRequiresAvailability(jobMic4, true);
            absent.AddSubRequiresAvailability(jobAttendant1, true);
            absent.AddSubRequiresAvailability(jobAttendant2, true);

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

            // No job can be done at the same time as substitute, attendant, or video
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobSubstitute, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobAttendant1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobAttendant2, 1);
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
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 1, templateSunday);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userDBecker), 1, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userCTangen), 1, 2);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userBBabbe), 3, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userESavelberg), 3, 1);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 3, templateSunday);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userESavelberg), 5, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userBBabbe), 5, 1);
            exceptions.AddUserException(attendant, _users.IndexOf(userDCook), 5, 2);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 5, templateSunday);
            exceptions.AddUserException(chairman, _users.IndexOf(userMPowers), 7, 1);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userDLopez), 7, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userDBecker), 7, 2);
            exceptions.AddUserException(chairman, _users.IndexOf(userCTangen), 9, 1);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userDBecker), 9, 2);
            exceptions.AddUserException(attendant, _users.IndexOf(userRStubbs), 9, 1);
            exceptions.AddUserException(attendant, _users.IndexOf(userMPowers), 9, 2);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDBecker), 0, templateTuesday);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userTSavelberg), 0, 1);
            exceptions.AddUserException(absent, _users.IndexOf(userTSavelberg), 2, 1);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userGSimmons), 2, templateTuesday);
            exceptions.AddUserException(discussionWithVideo, _users.IndexOf(userKLogan), 2, 1);
            exceptions.AddUserException(discussionWithVideo, _users.IndexOf(userMBrock), 2, 2);
            exceptions.AddUserException(discussionWithoutVideo, _users.IndexOf(userGSimmons), 4, 1);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMBrock), 6, templateTuesday);
            exceptions.AddUserException(absent, _users.IndexOf(userTSavelberg), 6, 1);
            exceptions.AddUserException(talkWithoutVideo, _users.IndexOf(userTSavelberg), 8, 1);
            exceptions.AddUserException(talkWithoutVideo, _users.IndexOf(userMBrock), 8, 2);

            consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            // Who needs to be substituted
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userDBecker), 1, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userCTangen), 1, discussionWithVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userDCook), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userMPowers), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userRStubbs), 1, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userESavelberg), 2, discussionWithVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userBBabbe), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userDKeil), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userESavelberg), 1, talkWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userDCook), 2, discussionWithVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userMPowers), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userCTangen), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDBecker), 2, discussionWithoutVideo);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDLopez), 2, discussionWithoutVideo);



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
