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
            string bestSolution = IntArrayToString(results.BestSolutionSoFarSolution);
            if (!_isAnalyzing) {
                if (results.BestSolutionSoFarSolution != null) {
                    StringBuilder text = new StringBuilder();
                    text.AppendLine("Best Solution So Far: " + bestSolution);
                    text.AppendLine("Best Solution Score So Far: " + results.BestSolutionSoFarScore + "%");
                    text.AppendLine("Generation Count: " + results.GenerationCount);
                    text.AppendLine("Generations Per Second: " + results.GenerationsPerSecond);
                    text.AppendLine("Elapsed Time: " + elapsedTime);
                    text.AppendLine("Utilizing " + results.CPUCoreCount + " CPU Cores");
                    text.Append(BestScoresSoFarText(results.BestSolutionSoFarScore, elapsedTime, bestSolution));
                    txtResults.Text = text.ToString();
                }
            } else {
                StringBuilder text = new StringBuilder();
                if (_analysisBestScores.Count > 0) {
                    text.AppendLine(BestOfAnalysisScores());
                }
                text.AppendLine("Elapsed Time: " + elapsedTime);
                text.AppendLine("Best Solution So Far: " + IntArrayToString(results.BestSolutionSoFarSolution));
                text.AppendLine("Best Solution Score So Far: " + results.BestSolutionSoFarScore + "%");
                text.AppendLine(BestScoresSoFarText(results.BestSolutionSoFarScore, elapsedTime, bestSolution));
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
                text.AppendLine("Solution: " + _analysisBestScores[index].Solution);
                text.AppendLine("");
            }
            return text.ToString();
        }

        private string BestScoresSoFarText(float scoreToBeat, string secondsSinceStart, string bestSolution) {
            StringBuilder text = new StringBuilder();
            if (_bestScores.Count > 0) {
                if (scoreToBeat > _bestScores[0].Score) {
                    _bestScores.Insert(0, new BestScoreEntry(secondsSinceStart, scoreToBeat, bestSolution));
                }
                foreach (BestScoreEntry entry in _bestScores) {
                    text.AppendLine("");
                    text.AppendLine("Previous Best: " + entry.Score);
                    text.AppendLine("Best at: " + entry.TimeSinceStart);
                }
            } else {
                _bestScores.Insert(0, new BestScoreEntry(secondsSinceStart, scoreToBeat, bestSolution));
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
            
            SoundBuilderV3.ActionFillMeetingsAll action = new SoundBuilderV3.ActionFillMeetingsAll(
                users: _users,
                jobConsiderations: _jobConsiderations,
                meetings: _meetings,
                resultsFunc: Results,
                solutionAction: Finish,
                mutationRate: Convert.ToInt32(txtMutationRate.Text),
                chromosomeCount: Convert.ToInt32(txtPopulation.Text),
                threadCount: Convert.ToInt32(txtThreadCount.Text),
                startingSolution: txtStartingSolution.Text);
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

            private string _solution;
            public string Solution {
                get { return _solution; }
            }

            public BestScoreEntry(string timeSinceStart, float score, string solution) {
                _timeSinceStart = timeSinceStart;
                _score = score;
                _solution = solution;
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

            SoundBuilderV3.ActionFillMeetingsAll action = new SoundBuilderV3.ActionFillMeetingsAll(
                users: _users,
                jobConsiderations: _jobConsiderations,
                meetings: _meetings,
                resultsFunc: Results,
                solutionAction: Finish,
                mutationRate: Convert.ToInt32(txtMutationRate.Text),
                chromosomeCount: Convert.ToInt32(txtPopulation.Text),
                threadCount: Convert.ToInt32(txtThreadCount.Text),
                startingSolution: txtStartingSolution.Text);
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
            jobSound.UniqueId = 1;
            _jobs.Add(jobSound);

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            jobStage.CanBeSubstituded = true;
            jobStage.UniqueId = 2;
            _jobs.Add(jobStage);

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            jobMic1.CanBeSubstituded = true;
            jobMic1.UniqueId = 3;
            _jobs.Add(jobMic1);

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            jobMic2.CanBeSubstituded = true;
            jobMic2.UniqueId = 3;
            _jobs.Add(jobMic2);

            Job jobMic3 = new Job();
            jobMic3.Name = "Mic 3";
            jobMic3.CanBeSubstituded = true;
            jobMic3.UniqueId = 3;
            _jobs.Add(jobMic3);

            Job jobMic4 = new Job();
            jobMic4.Name = "Mic 4";
            jobMic4.CanBeSubstituded = true;
            jobMic4.UniqueId = 3;
            _jobs.Add(jobMic4);

            Job jobAuxillary = new Job();
            jobAuxillary.Name = "Auxillary";
            jobAuxillary.CanBeSubstituded = false;
            jobAuxillary.UniqueId = 4;
            _jobs.Add(jobAuxillary);

            Job jobAttendant1 = new Job();
            jobAttendant1.Name = "Attendant 1";
            jobAttendant1.CanBeSubstituded = false;
            jobAttendant1.UniqueId = 5;
            _jobs.Add(jobAttendant1);

            Job jobAttendant2 = new Job();
            jobAttendant2.Name = "Attendant 2";
            jobAttendant2.CanBeSubstituded = false;
            jobAttendant2.UniqueId = 5;
            _jobs.Add(jobAttendant2);

            jobMic1.AddSameJob(jobMic2);
            jobMic1.AddSameJob(jobMic3);
            jobMic1.AddSameJob(jobMic4);
            jobMic2.AddSameJob(jobMic3);
            jobMic2.AddSameJob(jobMic4);
             jobMic3.AddSameJob(jobMic4);
            jobAttendant1.AddSameJob(jobAttendant2);

            //User userCTangen = new User();
            //userCTangen.Name = "Cameron Tangen";
            //userCTangen.Jobs = new List<Job> {  };
            //_users.Add(userCTangen);

            //User userDCook = new User();
            //userDCook.Name = "Dennis Cook";
            //userDCook.Jobs = new List<Job> { jobSound, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            //_users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            _users.Add(userDLopez);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userRStubbs);

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobSound, jobMic1, jobMic2, jobMic3, jobMic4 };
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
            userDBecker.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userDBecker);

            User userVCook = new User();
            userVCook.Name = "Vincente Cook";
            userVCook.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            _users.Add(userVCook);

            User userBReynolds = new User();
            userBReynolds.Name = "Ben Reynolds";
            userBReynolds.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            _users.Add(userBReynolds);

            User userJRhoades = new User();
            userJRhoades.Name = "John Rhoades";
            userJRhoades.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobAuxillary, jobMic4 };
            _users.Add(userJRhoades);

            User userJHernandez = new User();
            userJHernandez.Name = "Josh Hernandez";
            userJHernandez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            _users.Add(userJHernandez);

            User userFCoffman = new User();
            userFCoffman.Name = "Frank Coffman";
            userFCoffman.Jobs = new List<Job>() { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            _users.Add(userFCoffman);

            User userDHughes = new User();
            userDHughes.Name = "Derrick Hughes";
            userDHughes.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4, jobAuxillary };
            _users.Add(userDHughes);

            User userGHernandez = new User();
            userGHernandez.Name = "Gus Hernandez";
            userGHernandez.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userGHernandez);

            User userVGarcia = new User();
            userVGarcia.Name = "Victor Garcia";
            userVGarcia.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4, jobSound };
            _users.Add(userVGarcia);

            User userSKing = new User();
            userSKing.Name = "Stevie King";
            userSKing.Jobs = new List<Job> { jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userSKing);

            User userSHacker = new User();
            userSHacker.Name = "Shawn Hacker";
            userSHacker.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            _users.Add(userSHacker);

            Template templateSunday = new Template(1);
            templateSunday.Name = "Sunday";
            templateSunday.UniqueId = 1;
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobAuxillary };
            templateSunday.AddJobToAllPartitions(jobSound);
            templateSunday.AddJobToAllPartitions(jobStage);
            templateSunday.AddJobToAllPartitions(jobMic1);
            templateSunday.AddJobToAllPartitions(jobMic2);
            templateSunday.AddJobToAllPartitions(jobAuxillary);
            //templateSunday.AddJobToAllPartitions(jobAttendant1);
            //templateSunday.AddJobToAllPartitions(jobAttendant2);

            Template templateTuesday = new Template(1);
            templateTuesday.Name = "Tuesday";
            templateTuesday.UniqueId = 2;
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobAuxillary };
            templateTuesday.AddJobToAllPartitions(jobSound);
            templateTuesday.AddJobToAllPartitions(jobStage);
            templateTuesday.AddJobToAllPartitions(jobMic1);
            templateTuesday.AddJobToAllPartitions(jobMic2);
            templateTuesday.AddJobToAllPartitions(jobAuxillary);
            //templateTuesday.AddJobToAllPartitions(jobAttendant1);
            //templateTuesday.AddJobToAllPartitions(jobAttendant2);

            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/03/2019")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("12/08/2019")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/10/2019")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("12/15/2019")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/17/2019")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("12/22/2019")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/24/2019")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("12/29/2019")));
            _meetings.Add(templateTuesday.ToMeeting(DateTime.Parse("12/31/2019")));

            foreach (Meeting meeting in _meetings) {
                _templates.Add(meeting.ToTemplate());
            }

            //_meetings[0].AddUserForJob(userSHacker, jobMic1);
            //_meetings[0].AddUserForJob(userDCook, jobStage);
            //_meetings[0].AddUserForJob(userRStubbs, jobMic1);
            //_meetings[0].AddUserForJob(userMPowers, jobMic2);
            //_meetings[0].AddUserForJob(userCTangen, jobAttendant1);
            //_meetings[0].AddUserForJob(userTSavelberg, jobAttendant2);

            //_meetings[1].AddUserForJob(userBReynolds, jobSound);
            //_meetings[1].AddUserForJob(userDLopez, jobStage);
            //_meetings[1].AddUserForJob(userJRhoades, jobMic1);
            //_meetings[1].AddUserForJob(userDBecker, jobMic2);
            //_meetings[1].AddUserForJob(userCTangen, jobSubstitute);
            //_meetings[1].AddUserForJob(userRStubbs, jobAttendant1);
            //_meetings[1].AddUserForJob(userKLogan, jobAttendant2);

            //_meetings[2].AddUserForJob(userCTangen, jobSound);
            //_meetings[2].AddUserForJob(userBReynolds, jobStage);
            //_meetings[2].AddUserForJob(userRStubbs, jobMic1);
            //_meetings[2].AddUserForJob(userBTyler, jobMic2);
            //_meetings[2].AddUserForJob(userDBecker, jobAttendant1);
            //_meetings[2].AddUserForJob(userMPowers, jobAttendant2);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();

            JobConsideration consideration = null;

            consideration = new JobConsiderationLimitsPerPeriod.Builder()
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            _jobConsiderations.Add(consideration);
            ((JobConsiderationLimitsPerPeriod)consideration).AddLimit(_users.IndexOf(userBBabbe), 1);
            ((JobConsiderationLimitsPerPeriod)consideration).AddLimit(_users.IndexOf(userDBecker), 1);
            ((JobConsiderationLimitsPerPeriod)consideration).AddIgnoreLimitCountOnJob(jobAttendant1);
            ((JobConsiderationLimitsPerPeriod)consideration).AddIgnoreLimitCountOnJob(jobAttendant2);

            //consideration = new JobConsiderationEvenUserDistributionPerJob.Builder()
            //    .SetJobs(_jobs)
            //    .SetTemplates(_templates)
            //    .SetUsers(_users)
            //    .SetUserExceptions(exceptions)
            //    .Build();
            //_jobConsiderations.Add(consideration);
            consideration = new JobConsiderationVariety.Builder()
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

            // School parts can't do attendant, sound, or stage
            UserExceptionType school = new UserExceptionType("school");
            school.AddJobExceptionValue(jobSound, 1);
            school.AddJobExceptionValue(jobStage, 1);
            school.AddJobExceptionValue(jobMic1, 0);
            school.AddJobExceptionValue(jobMic2, 0);
            school.AddJobExceptionValue(jobMic3, 0);
            school.AddJobExceptionValue(jobMic4, 0);
            school.AddJobExceptionValue(jobAuxillary, 1);
            school.AddJobExceptionValue(jobAttendant1, 1);
            school.AddJobExceptionValue(jobAttendant2, 1);

            // School parts don't require substitues (for now)
            school.AddSubRequiresAvailability(jobSound, false);
            school.AddSubRequiresAvailability(jobStage, false);
            school.AddSubRequiresAvailability(jobMic1, false);
            school.AddSubRequiresAvailability(jobMic2, false);
            school.AddSubRequiresAvailability(jobMic3, false);
            school.AddSubRequiresAvailability(jobMic4, false);
            school.AddSubRequiresAvailability(jobAuxillary, false);
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
            sundayChairman.AddJobExceptionValue(jobAuxillary, 1);
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
            tuesdayChairman.AddJobExceptionValue(jobAuxillary, 1);
            tuesdayChairman.AddJobExceptionValue(jobAttendant1, 1);
            tuesdayChairman.AddJobExceptionValue(jobAttendant2, 1);

            // Tuesday chairman requires substitute availability for all jobs
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
            discussion.AddJobExceptionValue(jobAuxillary, 1);
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
            talk.AddJobExceptionValue(jobAuxillary, 1);
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
            absent.AddJobExceptionValue(jobAuxillary, 1);
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
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobAuxillary, 1);
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
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userJHernandez), 0, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDKeil), 0, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userRStubbs), 2, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDLopez), 2, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(tuesdayChairman, _users.IndexOf(userDBecker), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userJHernandez), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userBTyler), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(tuesdayChairman, _users.IndexOf(userBBabbe), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userRStubbs), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userSKing), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userBTyler), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userJHernandez), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userDKeil), 8, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userFCoffman), 8, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDKeil), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDBecker), 0, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 0, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 0, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVGarcia), 0, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJRhoades), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userFCoffman), 2, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDHughes), 2, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDBecker), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userRStubbs), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSHacker), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSKing), 4, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userRStubbs), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDBecker), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userGHernandez), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 6, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userRStubbs), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 8, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 8, templateTuesday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVGarcia), 8, templateTuesday);

            consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobAuxillary)
                .SetJobs(_jobs)
                .SetTemplates(_templates)
                .SetUsers(_users)
                .SetUserExceptions(exceptions)
                .Build();
            //_jobConsiderations.Add(consideration);

            // Who needs to be substituted
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userTSavelberg), 1, talk);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userRStubbs), 1, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userDCook), 1, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userESavelberg), 2, talk);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userBBabbe), 2, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(3, _users.IndexOf(userDLopez), 2, talk);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userCTangen), 1, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userBTyler), 1, school);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userDKeil), 1, school);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userMBrock), 2, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userDBecker), 1, talk);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userDCook), 1, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userESavelberg), 2, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userMPowers), 2, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(7, _users.IndexOf(userCTangen), 2, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(5, _users.IndexOf(userTSavelberg), 1, absent);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userTSavelberg), 1, absent);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userBBabbe), 1, talk);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userRStubbs), 1, discussion);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDCook), 1, school);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDLopez), 1, school);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userDBecker), 1, school);
            //((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(9, _users.IndexOf(userBTyler), 2, discussion);

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
            //List<int> solutionAsList = new List<int> { 11, 9, 13, 4, 8, 4, 8, 0, 11, 9, 11, 9, 6, 3, 0, 0, 8, 4, 2, 9, 1, 9, 11, 12, 8, 11, 9, 13, 5, 0, 4, 8, 7, 11, 1, 8, 0, 4, 1, 9 };
            //int[] solutionAsArray = solutionAsList.ToArray();

            //foreach (JobConsideration jobConsideration in _jobConsiderations) {
            //    jobConsideration.Begin();
            //    float score = jobConsideration.IsValid(solutionAsArray);
            //}
        }

        private void cmdPrintSolution_Click(object sender, EventArgs e) {
            txtResults.Text = "";
            Build();
            int[] solution = Array.ConvertAll(txtPrintSolution.Text.Split(','), x => int.Parse(x));
            UpdateResultsFinished(solution);
        }
    }
}
