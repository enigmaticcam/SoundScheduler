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

            Job jobSound = new Job();
            jobSound.Name = "Sound Box";
            jobSound.CanBeSubstituded = true;
            _jobs.Add(jobSound);

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            jobStage.CanBeSubstituded = true;
            _jobs.Add(jobStage);

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            jobMic1.CanBeSubstituded = true;
            _jobs.Add(jobMic1);

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            jobMic2.CanBeSubstituded = true;
            _jobs.Add(jobMic2);

            Job jobMic3 = new Job();
            jobMic3.Name = "Mic 3";
            jobMic3.CanBeSubstituded = true;
            _jobs.Add(jobMic3);

            Job jobMic4 = new Job();
            jobMic4.Name = "Mic 4";
            jobMic4.CanBeSubstituded = true;
            _jobs.Add(jobMic4);

            Job jobSubstitute = new Job();
            jobSubstitute.Name = "Substitute";
            jobSubstitute.CanBeSubstituded = false;
            _jobs.Add(jobSubstitute);

            Job jobAttendant1 = new Job();
            jobAttendant1.Name = "Attendant 1";
            jobAttendant1.CanBeSubstituded = false;
            _jobs.Add(jobAttendant1);

            Job jobAttendant2 = new Job();
            jobAttendant2.Name = "Attendant 2";
            jobAttendant2.CanBeSubstituded = false;
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
            userCTangen.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobSubstitute, jobAttendant1, jobAttendant2 };
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
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobAttendant1, jobAttendant2 };
            templateSunday.AddJobToAllPartitions(jobSound);
            templateSunday.AddJobToAllPartitions(jobStage);
            templateSunday.AddJobToPartition(jobMic1, 2);
            templateSunday.AddJobToPartition(jobMic2, 2);
            templateSunday.AddJobToPartition(jobAttendant1, 1);
            templateSunday.AddJobToPartition(jobAttendant2, 2);

            Template templateTuesday = new Template(2);
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobSubstitute, jobAttendant1, jobAttendant2 };
            templateTuesday.AddJobToAllPartitions(jobSound);
            templateTuesday.AddJobToAllPartitions(jobStage);
            templateTuesday.AddJobToAllPartitions(jobMic1);
            templateTuesday.AddJobToAllPartitions(jobMic2);
            templateTuesday.AddJobToAllPartitions(jobSubstitute);
            templateTuesday.AddJobToPartition(jobAttendant1, 1);
            templateTuesday.AddJobToPartition(jobAttendant2, 2);

            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("09/20/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("09/25/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("09/27/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("10/02/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("10/04/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("10/09/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("10/18/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("10/23/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("10/25/2016")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("10/30/2016")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("11/01/2016")));

            foreach (Meeting meeting in _meetings) {
                _templates.Add(meeting.ToTemplate());
            }

            _meetings[0].AddUserForJob(userDLopez, jobSound);
            _meetings[0].AddUserForJob(userESavelberg, jobStage);
            _meetings[0].AddUserForJob(userRStubbs, jobMic1);
            _meetings[0].AddUserForJob(userCTangen, jobMic2);
            _meetings[0].AddUserForJob(userDCook, jobSubstitute);
            _meetings[0].AddUserForJob(userMPowers, jobAttendant1);
            _meetings[0].AddUserForJob(userTSavelberg, jobAttendant2);

            _meetings[1].AddUserForJob(userBTyler, jobSound);
            _meetings[1].AddUserForJob(userDLopez, jobStage);
            _meetings[1].AddUserForJob(userDBecker, jobMic1);
            _meetings[1].AddUserForJob(userMPowers, jobMic2);
            _meetings[1].AddUserForJob(userRStubbs, jobAttendant1);
            _meetings[1].AddUserForJob(userCTangen, jobAttendant2);

            _meetings[2].AddUserForJob(userDLopez, jobSound);
            _meetings[2].AddUserForJob(userDCook, jobStage);
            _meetings[2].AddUserForJob(userBTyler, jobMic1);
            _meetings[2].AddUserForJob(userRStubbs, jobMic2);
            _meetings[2].AddUserForJob(userCTangen, jobSubstitute);
            _meetings[2].AddUserForJob(userDBecker, jobAttendant1);
            _meetings[2].AddUserForJob(userESavelberg, jobAttendant2);

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

            // School parts can't do attendant
            UserExceptionType school = new UserExceptionType("school");
            school.AddJobExceptionValue(jobSound, 0);
            school.AddJobExceptionValue(jobStage, 0);
            school.AddJobExceptionValue(jobMic1, 0);
            school.AddJobExceptionValue(jobMic2, 0);
            school.AddJobExceptionValue(jobMic3, 0);
            school.AddJobExceptionValue(jobMic4, 0);
            school.AddJobExceptionValue(jobSubstitute, 0);
            school.AddJobExceptionValue(jobAttendant1, 1);
            school.AddJobExceptionValue(jobAttendant2, 1);

            // School parts don't require substitues (for now)
            school.AddSubRequiresAvailability(jobSound, false);
            school.AddSubRequiresAvailability(jobStage, false);
            school.AddSubRequiresAvailability(jobMic1, false);
            school.AddSubRequiresAvailability(jobMic2, false);
            school.AddSubRequiresAvailability(jobMic3, false);
            school.AddSubRequiresAvailability(jobMic4, false);
            school.AddSubRequiresAvailability(jobSubstitute, false);
            school.AddSubRequiresAvailability(jobAttendant1, false);
            school.AddSubRequiresAvailability(jobAttendant2, false);

            // Chairman on Sunday can do still do stage, mics, substitute. Can't do attendant or sound.
            UserExceptionType sundayChairman = new UserExceptionType("sundayChairman");
            sundayChairman.AddJobExceptionValue(jobSound, 1);
            sundayChairman.AddJobExceptionValue(jobStage, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobMic1, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobMic2, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobMic3, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobMic4, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobSubstitute, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobAttendant1, 1);
            sundayChairman.AddJobExceptionValue(jobAttendant2, 1);

            // Chairman on Sunday will require substitute availability if they do sound or attendant
            sundayChairman.AddSubRequiresAvailability(jobSound, true);
            sundayChairman.AddSubRequiresAvailability(jobStage, false);
            sundayChairman.AddSubRequiresAvailability(jobMic1, false);
            sundayChairman.AddSubRequiresAvailability(jobMic2, false);
            sundayChairman.AddSubRequiresAvailability(jobMic3, false);
            sundayChairman.AddSubRequiresAvailability(jobMic4, false);
            sundayChairman.AddSubRequiresAvailability(jobAttendant1, true);
            sundayChairman.AddSubRequiresAvailability(jobAttendant2, true);

            // Tuesday Chairman can't do anything
            UserExceptionType tuesdayChairman = new UserExceptionType("tuesdayChairman");
            tuesdayChairman.AddJobExceptionValue(jobSound, 1);
            tuesdayChairman.AddJobExceptionValue(jobStage, 1);
            tuesdayChairman.AddJobExceptionValue(jobMic1, 1);
            tuesdayChairman.AddJobExceptionValue(jobMic2, 1);
            tuesdayChairman.AddJobExceptionValue(jobMic3, 1);
            tuesdayChairman.AddJobExceptionValue(jobMic4, 1);
            tuesdayChairman.AddJobExceptionValue(jobSubstitute, 1);
            tuesdayChairman.AddJobExceptionValue(jobAttendant1, 1);
            tuesdayChairman.AddJobExceptionValue(jobAttendant2, 1);

            // Tuesday chairm requires substitute availability for all jobs
            tuesdayChairman.AddSubRequiresAvailability(jobSound, true);
            tuesdayChairman.AddSubRequiresAvailability(jobStage, true);
            tuesdayChairman.AddSubRequiresAvailability(jobMic1, true);
            tuesdayChairman.AddSubRequiresAvailability(jobMic2, true);
            tuesdayChairman.AddSubRequiresAvailability(jobMic3, true);
            tuesdayChairman.AddSubRequiresAvailability(jobMic4, true);
            tuesdayChairman.AddSubRequiresAvailability(jobAttendant1, true);
            tuesdayChairman.AddSubRequiresAvailability(jobAttendant2, true);

            // Discussion can still do stage and substitute. Not attendant
            UserExceptionType discussion = new UserExceptionType("discussion");
            discussion.AddJobExceptionValue(jobSound, 1);
            discussion.AddJobExceptionValue(jobStage, (float)0.5);
            discussion.AddJobExceptionValue(jobMic1, 1);
            discussion.AddJobExceptionValue(jobMic2, 1);
            discussion.AddJobExceptionValue(jobMic3, 1);
            discussion.AddJobExceptionValue(jobMic4, 1);
            discussion.AddJobExceptionValue(jobSubstitute, (float)0.5);
            discussion.AddJobExceptionValue(jobAttendant1, 1);
            discussion.AddJobExceptionValue(jobAttendant2, 1);

            // Discussion will require substitute availability if they do sound, mics, or attendant
            discussion.AddSubRequiresAvailability(jobSound, true);
            discussion.AddSubRequiresAvailability(jobStage, false);
            discussion.AddSubRequiresAvailability(jobMic1, true);
            discussion.AddSubRequiresAvailability(jobMic2, true);
            discussion.AddSubRequiresAvailability(jobMic3, true);
            discussion.AddSubRequiresAvailability(jobMic4, true);
            discussion.AddSubRequiresAvailability(jobAttendant1, true);
            discussion.AddSubRequiresAvailability(jobAttendant2, true);

            // Talk can do all but sound and attendant
            UserExceptionType talk = new UserExceptionType("talk");
            talk.AddJobExceptionValue(jobSound, 1);
            talk.AddJobExceptionValue(jobStage, (float)0.5);
            talk.AddJobExceptionValue(jobMic1, (float)0.5);
            talk.AddJobExceptionValue(jobMic2, (float)0.5);
            talk.AddJobExceptionValue(jobMic3, (float)0.5);
            talk.AddJobExceptionValue(jobMic4, (float)0.5);
            talk.AddJobExceptionValue(jobSubstitute, (float)0.5);
            talk.AddJobExceptionValue(jobAttendant1, 1);
            talk.AddJobExceptionValue(jobAttendant2, 1);

            // Talk will require substitute availability only for sound and attendant
            talk.AddSubRequiresAvailability(jobSound, true);
            talk.AddSubRequiresAvailability(jobStage, false);
            talk.AddSubRequiresAvailability(jobMic1, false);
            talk.AddSubRequiresAvailability(jobMic2, false);
            talk.AddSubRequiresAvailability(jobMic3, false);
            talk.AddSubRequiresAvailability(jobMic4, false);
            talk.AddSubRequiresAvailability(jobAttendant1, true);
            talk.AddSubRequiresAvailability(jobAttendant2, true);

            // Absentee can't do any job. They ain't there!
            UserExceptionType absent = new UserExceptionType("absent");
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
            absent.AddSubRequiresAvailability(jobSound, true);
            absent.AddSubRequiresAvailability(jobStage, true);
            absent.AddSubRequiresAvailability(jobMic1, true);
            absent.AddSubRequiresAvailability(jobMic2, true);
            absent.AddSubRequiresAvailability(jobMic3, true);
            absent.AddSubRequiresAvailability(jobMic4, true);
            absent.AddSubRequiresAvailability(jobAttendant1, true);
            absent.AddSubRequiresAvailability(jobAttendant2, true);

            // No job can be done at the same time as substitutev or attendant
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobSubstitute, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobAttendant1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobAttendant2, 1);

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
            exceptions.AddUserExceptionToAllPartitions(tuesdayChairman, _users.IndexOf(userGSimmons), 4, templateTuesday);
            exceptions.AddUserException(absent, _users.IndexOf(userTSavelberg), 4, 1);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(tuesdayChairman, _users.IndexOf(userMBrock), 6, templateTuesday);
            exceptions.AddUserException(discussion, _users.IndexOf(userGSimmons), 6, 2);
            exceptions.AddUserException(absent, _users.IndexOf(userTSavelberg), 8, 1);
            exceptions.AddUserException(discussion, _users.IndexOf(userMBrock), 8, 2);
            exceptions.AddUserException(discussion, _users.IndexOf(userGSimmons), 8, 2);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 8, templateTuesday);
            exceptions.AddUserException(sundayChairman, _users.IndexOf(userESavelberg), 3, 1);
            exceptions.AddUserException(discussion, _users.IndexOf(userMPowers), 3, 2);
            exceptions.AddUserException(sundayChairman, _users.IndexOf(userTSavelberg), 5, 1);
            exceptions.AddUserException(discussion, _users.IndexOf(userMBrock), 5, 2);
            exceptions.AddUserException(sundayChairman, _users.IndexOf(userGSimmons), 7, 1);
            exceptions.AddUserException(discussion, _users.IndexOf(userESavelberg), 7, 2);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 7, templateSunday);
            exceptions.AddUserException(sundayChairman, _users.IndexOf(userDBecker), 9, 1);
            exceptions.AddUserException(discussion, _users.IndexOf(userCTangen), 9, 2);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userMPowers), 10, templateTuesday);

            consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobSubstitute)
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);

            // Who needs to be substituted
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(4, _users.IndexOf(userCTangen), 1, talk);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(4, _users.IndexOf(userMPowers), 2, talk);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(4, _users.IndexOf(userDLopez), 2, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(6, _users.IndexOf(userDBecker), 1, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(6, _users.IndexOf(userESavelberg), 2, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(6, _users.IndexOf(userMPowers), 2, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(8, _users.IndexOf(userRStubbs), 1, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(8, _users.IndexOf(userDLopez), 1, school);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(8, _users.IndexOf(userDBecker), 2, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(10, _users.IndexOf(userESavelberg), 1, talk);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(10, _users.IndexOf(userCTangen), 1, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(10, _users.IndexOf(userBBabbe), 2, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(10, _users.IndexOf(userDBecker), 2, discussion);
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(10, _users.IndexOf(userDKeil), 2, discussion);

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
            //List<int> solutionAsList = new List<int> { 2, 3, 6, 8, 4, 9, 6, 1, 4, 7, 3, 0, 8, 3, 0, 9, 1, 2, 4, 1, 2, 6, 4, 0, 9, 12, 6, 3, 5, 7, 11, 4, 6, 2, 0, 8, 3, 1, 9, 1, 0, 2, 4, 13, 8, 6, 3, 7, 8, 2, 0, 12, 3, 1, 6, 9, 13, 4, 1, 2, 7, 0, 3, 8, 10, 6, 2, 8, 4, 9, 0, 3, 0, 4, 6, 2, 8, 1 };
            //int[] solutionAsArray = solutionAsList.ToArray();

            //foreach (JobConsideration jobConsideration in _jobConsiderations) {
            //    float score = jobConsideration.IsValid(solutionAsArray);
            //}
        }
    }
}
