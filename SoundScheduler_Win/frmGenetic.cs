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
                    text.AppendLine("Date:\t" + meeting.Date.ToShortDateString());
                    foreach (Job job in meeting.Jobs) {
                        text.AppendLine(job.Name + ":\t" + _users[solution[counter]].Name);
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

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            jobStage.CanBeSubstituded = false;
            jobStage.UniqueId = 1;
            _jobs.Add(jobStage);

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            jobMic1.CanBeSubstituded = false;
            jobMic1.UniqueId = 2;
            _jobs.Add(jobMic1);

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            jobMic2.CanBeSubstituded = false;
            jobMic2.UniqueId = 2;
            _jobs.Add(jobMic2);

            Job jobOutsideAttendant1 = new Job();
            jobOutsideAttendant1.Name = "Outside Attendant 1";
            jobOutsideAttendant1.CanBeSubstituded = false;
            jobOutsideAttendant1.UniqueId = 3;
            _jobs.Add(jobOutsideAttendant1);

            Job jobOutsideAttendant2 = new Job();
            jobOutsideAttendant2.Name = "Outside Attendant 2";
            jobOutsideAttendant2.CanBeSubstituded = false;
            jobOutsideAttendant2.UniqueId = 3;
            _jobs.Add(jobOutsideAttendant2);

            Job jobAuditoriumAttendant1 = new Job();
            jobAuditoriumAttendant1.Name = "Entry Auditorium Attendant";
            jobAuditoriumAttendant1.CanBeSubstituded = false;
            jobAuditoriumAttendant1.UniqueId = 4;
            _jobs.Add(jobAuditoriumAttendant1);

            Job jobAuditoriumAttendant2 = new Job();
            jobAuditoriumAttendant2.Name = "Auditorium Attendant";
            jobAuditoriumAttendant2.CanBeSubstituded = false;
            jobAuditoriumAttendant2.UniqueId = 4;
            _jobs.Add(jobAuditoriumAttendant2);

            jobMic1.AddSameJob(jobMic2);
            jobAuditoriumAttendant1.AddSameJob(jobAuditoriumAttendant2);
            jobOutsideAttendant1.AddSameJob(jobOutsideAttendant2);

            User userARhoades = new User();
            userARhoades.Name = "Aidan Rhoades";
            userARhoades.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobMic1, jobMic2 };
            _users.Add(userARhoades);

            User userBReynolds = new User();
            userBReynolds.Name = "Ben Reynolds";
            userBReynolds.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userBReynolds);

            User userBBabbe = new User();
            userBBabbe.Name = "Bob Babbe";
            userBBabbe.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2 };
            _users.Add(userBBabbe);

            User userDBecker = new User();
            userDBecker.Name = "Dave Becker";
            userDBecker.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2 };
            _users.Add(userDBecker);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2 };
            _users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobMic1, jobMic2, jobOutsideAttendant1, jobAuditoriumAttendant2 };
            _users.Add(userDLopez);

            User userDHughes = new User();
            userDHughes.Name = "Derrick Hughes";
            userDHughes.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userDHughes);

            User userFCoffman = new User();
            userFCoffman.Name = "Frank Coffman";
            userFCoffman.Jobs = new List<Job>() { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2 };
            _users.Add(userFCoffman);

            User userGSimmons = new User();
            userGSimmons.Name = "George Simmons";
            userGSimmons.Jobs = new List<Job>() { jobAuditoriumAttendant1, jobAuditoriumAttendant2 };
            _users.Add(userGSimmons);

            User userJRhoades = new User();
            userJRhoades.Name = "John Rhoades";
            userJRhoades.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userJRhoades);

            User userJHernandez = new User();
            userJHernandez.Name = "Josh Hernandez";
            userJHernandez.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userJHernandez);

            User userPDunsmuir = new User();
            userPDunsmuir.Name = "Peter Dunsmuir";
            userPDunsmuir.Jobs = new List<Job> { jobMic1, jobMic2 };
            _users.Add(userPDunsmuir);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2 };
            _users.Add(userRStubbs);

            User userSHacker = new User();
            userSHacker.Name = "Shawn Hacker";
            userSHacker.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userSHacker);

            User userSKing = new User();
            userSKing.Name = "Stevie King";
            userSKing.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userSKing);

            User userSTaylor = new User();
            userSTaylor.Name = "Steve Taylor";
            userSTaylor.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userSTaylor);

            User userVCook = new User();
            userVCook.Name = "Vincente Cook";
            userVCook.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2, jobStage };
            _users.Add(userVCook);

            Template templateSunday = new Template(1);
            templateSunday.Name = "Sunday";
            templateSunday.UniqueId = 1;
            templateSunday.Jobs = new List<Job> { jobOutsideAttendant1, jobOutsideAttendant2, jobAuditoriumAttendant1, jobAuditoriumAttendant2, jobMic1, jobMic2, jobStage };
            templateSunday.AddJobToAllPartitions(jobOutsideAttendant1);
            templateSunday.AddJobToAllPartitions(jobOutsideAttendant2);
            templateSunday.AddJobToAllPartitions(jobAuditoriumAttendant1);
            templateSunday.AddJobToAllPartitions(jobAuditoriumAttendant2);
            templateSunday.AddJobToAllPartitions(jobMic1);
            templateSunday.AddJobToAllPartitions(jobMic2);
            templateSunday.AddJobToAllPartitions(jobStage);

            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/01/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/03/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/08/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/10/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/15/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/17/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/22/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/24/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/29/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("05/31/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/05/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/07/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/12/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/14/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/19/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/21/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/26/2022")));
            _meetings.Add(templateSunday.ToMeeting(DateTime.Parse("06/28/2022")));

            foreach (Meeting meeting in _meetings) {
                _templates.Add(meeting.ToTemplate());
            }

            UserExceptionDictionary exceptions = new UserExceptionDictionary();

            JobConsideration consideration = null;

            //consideration = new JobConsiderationLimitsPerPeriod.Builder()
            //    .SetJobs(_jobs)
            //    .SetTemplates(_templates)
            //    .SetUsers(_users)
            //    .SetUserExceptions(exceptions)
            //    .Build();
            //_jobConsiderations.Add(consideration);
            //((JobConsiderationLimitsPerPeriod)consideration).AddLimit(_users.IndexOf(userBBabbe), 1);
            //((JobConsiderationLimitsPerPeriod)consideration).AddLimit(_users.IndexOf(userDBecker), 1);
            //((JobConsiderationLimitsPerPeriod)consideration).AddIgnoreLimitCountOnJob(jobAttendant1);
            //((JobConsiderationLimitsPerPeriod)consideration).AddIgnoreLimitCountOnJob(jobAttendant2);

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

            // School parts can't do attendant or stage
            UserExceptionType school = new UserExceptionType("school");
            school.AddJobExceptionValue(jobAuditoriumAttendant1, 1);
            school.AddJobExceptionValue(jobAuditoriumAttendant2, 1);
            school.AddJobExceptionValue(jobOutsideAttendant1, 1);
            school.AddJobExceptionValue(jobOutsideAttendant2, 1);
            school.AddJobExceptionValue(jobStage, 1);
            school.AddJobExceptionValue(jobMic1, (float)0.5);
            school.AddJobExceptionValue(jobMic2, (float)0.5);

            // Chairman on Sunday can do still do stage, mics. Can't do attendant
            UserExceptionType sundayChairman = new UserExceptionType("sundayChairman");
            sundayChairman.AddJobExceptionValue(jobAuditoriumAttendant1, 1);
            sundayChairman.AddJobExceptionValue(jobAuditoriumAttendant2, 1);
            sundayChairman.AddJobExceptionValue(jobOutsideAttendant1, 1);
            sundayChairman.AddJobExceptionValue(jobOutsideAttendant2, 1);
            sundayChairman.AddJobExceptionValue(jobStage, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobMic1, (float)0.5);
            sundayChairman.AddJobExceptionValue(jobMic2, (float)0.5);

            // Discussion can still do stage. Not attendant
            UserExceptionType discussion = new UserExceptionType("discussion");
            discussion.AddJobExceptionValue(jobAuditoriumAttendant1, 1);
            discussion.AddJobExceptionValue(jobAuditoriumAttendant2, 1);
            discussion.AddJobExceptionValue(jobOutsideAttendant1, (float)0.5);
            discussion.AddJobExceptionValue(jobOutsideAttendant2, (float)0.5);
            discussion.AddJobExceptionValue(jobStage, (float)0.5);
            discussion.AddJobExceptionValue(jobMic1, 1);
            discussion.AddJobExceptionValue(jobMic2, 1);

            // Talk can do all but sound and attendant
            UserExceptionType talk = new UserExceptionType("talk");
            talk.AddJobExceptionValue(jobAuditoriumAttendant1, 1);
            talk.AddJobExceptionValue(jobAuditoriumAttendant2, 1);
            talk.AddJobExceptionValue(jobOutsideAttendant1, (float)0.5);
            talk.AddJobExceptionValue(jobOutsideAttendant2, (float)0.5);
            talk.AddJobExceptionValue(jobStage, (float)0.5);
            talk.AddJobExceptionValue(jobMic1, (float)0.5);
            talk.AddJobExceptionValue(jobMic2, (float)0.5);

            // Absentee can't do any job. They ain't there!
            UserExceptionType absent = new UserExceptionType("absent");
            absent.AddJobExceptionValue(jobAuditoriumAttendant1, 1);
            absent.AddJobExceptionValue(jobAuditoriumAttendant2, 1);
            absent.AddJobExceptionValue(jobOutsideAttendant1, 1);
            absent.AddJobExceptionValue(jobOutsideAttendant2, 1);
            absent.AddJobExceptionValue(jobStage, 1);
            absent.AddJobExceptionValue(jobMic1, 1);
            absent.AddJobExceptionValue(jobMic2, 1);

            // Auditorium Attendants and Stage can't overlap with anything. Outside Attendant can overlap with mics.
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant1, jobAuditoriumAttendant2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant1, jobOutsideAttendant1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant1, jobOutsideAttendant2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant1, jobStage, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant1, jobMic1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant1, jobMic2, 1);

            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant2, jobOutsideAttendant1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant2, jobOutsideAttendant2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant2, jobStage, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant2, jobMic1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobAuditoriumAttendant2, jobMic2, 1);

            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant1, jobOutsideAttendant2, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant1, jobStage, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant1, jobMic1, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant1, jobMic2, (float)0.5);

            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant2, jobStage, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant2, jobMic1, (float)0.5);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobOutsideAttendant2, jobMic2, (float)0.5);

            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobStage, jobMic1, 1);
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobStage, jobMic2, 1);

            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddSingleJobCombo(jobMic1, jobMic2, 1);

            // Who's partially or completely unavailable
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userPDunsmuir), 0, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 0, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSTaylor), 0, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userFCoffman), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJRhoades), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 2, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userFCoffman), 2, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSTaylor), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDHughes), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 4, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userFCoffman), 4, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSHacker), 4, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJRhoades), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDHughes), 6, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 6, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 6, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSKing), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDHughes), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSHacker), 8, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userFCoffman), 8, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userPDunsmuir), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJRhoades), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 10, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 10, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSHacker), 10, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSTaylor), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDHughes), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 12, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userFCoffman), 12, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSTaylor), 12, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJRhoades), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userVCook), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSKing), 14, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 14, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 14, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userPDunsmuir), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDHughes), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJHernandez), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSHacker), 16, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDLopez), 16, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userJRhoades), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBReynolds), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userRStubbs), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userPDunsmuir), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userGSimmons), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userJHernandez), 1, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userGSimmons), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userSHacker), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userJRhoades), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userDCook), 3, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userFCoffman), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userVCook), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userARhoades), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userRStubbs), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userPDunsmuir), 5, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userPDunsmuir), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userBReynolds), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userDLopez), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userBBabbe), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userJRhoades), 7, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDBecker), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDLopez), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userBReynolds), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userRStubbs), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userBBabbe), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDCook), 9, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userRStubbs), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDCook), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userPDunsmuir), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userGSimmons), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userVCook), 11, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userGSimmons), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userJHernandez), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userFCoffman), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userRStubbs), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userBBabbe), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDHughes), 13, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDCook), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(talk, _users.IndexOf(userJRhoades), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userBBabbe), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDBecker), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userFCoffman), 15, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userFCoffman), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(school, _users.IndexOf(userSHacker), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userPDunsmuir), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userVCook), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDLopez), 17, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userDBecker), 0, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(sundayChairman, _users.IndexOf(userGSimmons), 2, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDLopez), 2, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 4, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(sundayChairman, _users.IndexOf(userDBecker), 6, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(sundayChairman, _users.IndexOf(userDCook), 8, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userPDunsmuir), 8, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(sundayChairman, _users.IndexOf(userRStubbs), 10, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userJHernandez), 10, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(sundayChairman, _users.IndexOf(userBBabbe), 12, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userBBabbe), 14, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(sundayChairman, _users.IndexOf(userJHernandez), 14, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userDBecker), 14, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(discussion, _users.IndexOf(userJRhoades), 16, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSTaylor), 2, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userPDunsmuir), 6, templateSunday);
            exceptions.AddUserExceptionToAllPartitions(absent, _users.IndexOf(userSTaylor), 16, templateSunday);


            //consideration = new JobConsiderationSubstituteJobAvailability.Builder()
            //    .SetSubstituteJob(jobAuxillary)
            //    .SetJobs(_jobs)
            //    .SetTemplates(_templates)
            //    .SetUsers(_users)
            //    .SetUserExceptions(exceptions)
            //    .Build();
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
