﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Engine {
    public class SoundBuilderV3 {
        private List<Job> _jobs;
        private HashSet<User> _users;
        private MeetingsByDate _meetings;
        private Dictionary<DateTime, Template> _templates;
        private ExceptionsByDate _exceptions;
        private JobCancelUsersWhoPreferNotCertainMeetings _preferences;

        public void AddTemplate(DateTime date, Template template) {
            _templates.Add(date, template);
        }

        public void AddMeeting(DateTime date, Meeting meeting) {
            _meetings.AddMeeting(meeting);
        }

        public void AddException(DateTime date, User user, bool isSoftException) {
            _exceptions.AddException(date, user, isSoftException);
        }

        public void AddPreferenceNot(User user, Template template) {
            _preferences.AddPrefernceNot(user, template);
        }

        public MeetingsByDate BuildSchedule() {
            //CreateMeetingsFromTemplates();
            FillMeetingsAll();
            return _meetings;
        }

        private void CreateMeetingsFromTemplates() {
            foreach (DateTime date in _templates.Keys) {
                if (!_meetings.ContainsKey(date)) {
                    Meeting meeting = _templates[date].ToMeeting(date);
                    _meetings.AddMeeting(meeting);
                }
            }
        }

        private void FillMeetingsAll() {
            
        }

        public class ActionFillMeetingsAll {
            private List<Template> _templates = new List<Template>();
            private List<User> _users;
            private IEnumerable<JobConsideration> _jobConsiderations;
            private IEnumerable<Meeting> _meetings;
            private Genetic _genetic = new Genetic();
            private Func<Genetic.GeneticResults, bool> _resultsFunc;
            private Action<int[]> _solutionAction;

            public void PerformAction() {
                CreateTemplatesFromMeetings();
                AddImmutableUsers();
                PerformAlgorithm();
            }

            private void CreateTemplatesFromMeetings() {
                foreach (Meeting meeting in _meetings) {
                    Template template = new Template();
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
                //int[] solution = _genetic.Begin(GetBitLength(), GetBitCount(), Fitness);
                //bool stopHere = true;
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
                int score = 100000;
                foreach (JobConsideration consideration in _jobConsiderations) {
                    int exceptionReduction = consideration.IsValid(chromosome);
                    if (!consideration.IsConsiderationSoft) {
                        exceptionReduction *= 100;
                    }
                    score -= exceptionReduction;
                }
                if (score == 100000) {
                    return genetic.IsSolved;
                } else {
                    return score;
                }
            }

            public ActionFillMeetingsAll(Builder builder) {
                _users = builder.Users.ToList();
                _jobConsiderations = builder.JobConsiderations;
                _meetings = builder.Meetings;
                _resultsFunc = builder.ResultsFunc;
                _solutionAction = builder.SolutionAction;
            }

            public class Builder {
                public IEnumerable<User> Users;
                public IEnumerable<JobConsideration> JobConsiderations;
                public IEnumerable<Meeting> Meetings;
                public Func<Genetic.GeneticResults, bool> ResultsFunc;
                public Action<int[]> SolutionAction;

                public Builder SetUsers(IEnumerable<User> users) {
                    this.Users = users;
                    return this;
                }

                public Builder SetJobConsiderations(IEnumerable<JobConsideration> jobConsiderations) {
                    this.JobConsiderations = jobConsiderations;
                    return this;
                }

                public Builder SetMeetings(IEnumerable<Meeting> meetings) {
                    this.Meetings = meetings;
                    return this;
                }

                public Builder SetResultsFunc(Func<Genetic.GeneticResults, bool> resultsFunc) {
                    this.ResultsFunc = resultsFunc;
                    return this;
                }

                public Builder SetSolutionAction(Action<int[]> solutionAction) {
                    this.SolutionAction = solutionAction;
                    return this;
                }

                public ActionFillMeetingsAll Build() {
                    return new ActionFillMeetingsAll(this);
                }
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