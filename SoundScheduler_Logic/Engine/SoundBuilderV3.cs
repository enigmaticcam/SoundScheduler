using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Engine {
    public class SoundBuilderV3 {
        private List<Job> _jobs;
        private HashSet<User> _users;
        private MeetingsByDate _meetings;
        private Dictionary<DateTime, Template> _templates;
        private ExceptionsByDate _exceptions;

        public void AddTemplate(DateTime date, Template template) {
            _templates.Add(date, template);
        }

        public void AddMeeting(DateTime date, Meeting meeting) {
            _meetings.AddMeeting(meeting);
        }

        public void AddException(DateTime date, User user, bool isSoftException) {
            _exceptions.AddException(date, user, isSoftException);
        }

        public class ActionFillMeetingsAll {
            private List<Template> _templates = new List<Template>();
            private List<User> _users;
            private IEnumerable<JobConsideration> _jobConsiderations;
            private IEnumerable<Meeting> _meetings;
            private Genetic _genetic = new Genetic();
            private Func<Genetic.GeneticResults, bool> _resultsFunc;
            private Action<int[]> _solutionAction;
            private float _solve = 10000;
            private Dictionary<int, List<JobConsideration>> _jobConsiderationsInThreads = new Dictionary<int, List<JobConsideration>>();
            private int _chromosomeCount;
            private int _mutationRate;
            private int _threadCount;
            private string _startingSolution;
            static readonly object _object = new object();

            public void PerformAction() {
                RankJobConsiderations();
                CreateTemplatesFromMeetings();
                AddImmutableUsers();
                PerformAlgorithm();
            }

            private void RankJobConsiderations() {
                for (int i = 0; i < _jobConsiderations.Count(); i++) {
                    _jobConsiderations.ElementAt(i).JobRank = i + 1;
                }
            }

            private void CreateTemplatesFromMeetings() {
                foreach (Meeting meeting in _meetings) {
                    Template template = new Template();
                    template.UniqueId = meeting.UniqueId;
                    foreach (Job job in meeting.Jobs) {
                        template.Jobs.Add(job);
                    }
                    _templates.Add(template);
                }
            }

            private void AddImmutableUsers() {
                int index = 0;
                foreach (Meeting meeting in _meetings) {
                    foreach (Job job in meeting.Jobs) {
                        User user = meeting.UserForJob(job);
                        if (user != null) {
                            _genetic.AddImmutableBits(index, _users.IndexOf(user));
                        }
                        ++index;
                    }
                }
            }

            private void PerformAlgorithm() {
                _genetic.StartingSolution = _startingSolution;
                _genetic.ScoreSolveValue = _solve;
                _genetic.ChromosomeCount = _chromosomeCount;
                _genetic.MutationRate = _mutationRate;
                _genetic.ThreadCount = _threadCount;
                _genetic.BeginAsync(GetBitLength(), GetBitCount(), Fitness, _resultsFunc, _solutionAction);
            }

            private int GetBitLength() {
                int count = 0;
                foreach (Template template in _templates) {
                    count += template.Jobs.Count;
                }
                return count;
            }

            private int GetBitCount() {
                return _users.Count();
            }

            private float Fitness(int[] chromosome, Genetic genetic) {
                float score = _solve;
                CopyJobConsiderationsToThread();
                foreach (JobConsideration consideration in _jobConsiderationsInThreads[Thread.CurrentThread.ManagedThreadId]) {
                    float exceptionReduction = consideration.IsValid(chromosome);
                    if (!consideration.IsConsiderationSoft) {
                        exceptionReduction *= 100;
                    } else {
                        exceptionReduction *= consideration.JobRank;
                    }
                    score -= exceptionReduction;
                }
                if (score == _solve) {
                    return genetic.IsSolved;
                } else if (score == genetic.IsSolved) {
                    return score - 1;
                } else {
                    return score;
                }
            }

            private void CopyJobConsiderationsToThread() {
                if (!_jobConsiderationsInThreads.ContainsKey(Thread.CurrentThread.ManagedThreadId)) {
                    lock (_object) {
                        _jobConsiderationsInThreads.Add(Thread.CurrentThread.ManagedThreadId, new List<JobConsideration>());
                    }
                    foreach (JobConsideration consideration in _jobConsiderations) {
                        JobConsideration newConsideration = consideration.ToCopy();
                        newConsideration.Begin();
                        _jobConsiderationsInThreads[Thread.CurrentThread.ManagedThreadId].Add(newConsideration);
                    }
                }
            }

            public ActionFillMeetingsAll(IEnumerable<User> users, IEnumerable<JobConsideration> jobConsiderations, IEnumerable<Meeting> meetings, Func<Genetic.GeneticResults, bool> resultsFunc,
                Action<int[]> solutionAction, int mutationRate, int chromosomeCount, int threadCount, string startingSolution) {
                _users = users.ToList();
                _jobConsiderations = jobConsiderations;
                _meetings = meetings;
                _resultsFunc = resultsFunc;
                _solutionAction = solutionAction;
                _mutationRate = mutationRate;
                _chromosomeCount = chromosomeCount;
                _threadCount = threadCount;
                _startingSolution = startingSolution;
            }
        }

        public class ExistingMeeting {
            public int TemplateIndex { get; set; }
            public Meeting Meeting { get; set; }
        }

        public class MeetingsByDate {
            private Dictionary<DateTime, Meeting> _meetings;

            public IEnumerable<DateTime> Keys {
                get { return _meetings.Keys.OrderBy(k => k.Date); }
            }

            public IEnumerable<Meeting> Values {
                get { return _meetings.Values.OrderBy(m => m.Date); }
            }

            public void AddMeeting(Meeting meeting) {
                _meetings.Add(meeting.Date, meeting);
            }

            public Meeting GetMeeting(DateTime date) {
                if (_meetings.ContainsKey(date)) {
                    return _meetings[date];
                } else {
                    return null;
                }
            }

            public bool ContainsKey(DateTime date) {
                return _meetings.ContainsKey(date);
            }

            public MeetingsByDate() {
                _meetings = new Dictionary<DateTime, Meeting>();
            }
        }

        public class ExceptionsByDate {
            private Dictionary<string, Dictionary<User, MeetingException>> _exceptions;

            public void AddException(DateTime date, User user, bool isSoftException) {
                string key = DateToKey(date);
                if (!_exceptions.ContainsKey(key)) {
                    _exceptions.Add(key, new Dictionary<User, MeetingException>());
                }
                MeetingException exception = new MeetingException();
                exception.Date = date;
                exception.User = user;
                exception.IsSoftException = isSoftException;
                _exceptions[key].Add(user, exception);
            }

            public bool DoesUserHaveSoftException(DateTime date, User user) {
                string key = DateToKey(date);
                if (_exceptions.ContainsKey(key) && _exceptions[key].ContainsKey(user)) {
                    MeetingException exception = _exceptions[key][user];
                    return exception.IsSoftException;
                } else {
                    return false;
                }
            }

            public bool DoesUserHaveHardException(DateTime date, User user) {
                string key = DateToKey(date);
                if (_exceptions.ContainsKey(key) && _exceptions[key].ContainsKey(user)) {
                    MeetingException exception = _exceptions[key][user];
                    return !exception.IsSoftException;
                } else {
                    return false;
                }
            }

            private string DateToKey(DateTime date) {
                return date.Year.ToString() + date.Month.ToString() + date.Day.ToString();
            }

            public ExceptionsByDate() {
                _exceptions = new Dictionary<string, Dictionary<User, MeetingException>>();
            }
        }
    }
}
