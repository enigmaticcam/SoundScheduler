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
            ActionFillMeetingsAll action = new ActionFillMeetingsAll(_templates.Values, _users);
            action.PerformAction();
        }

        private class ActionFillMeetingsAll {
            private IEnumerable<Template> _templates;
            private IEnumerable<User> _users;

            public void PerformAction() {
                Genetic genetic = new Genetic();
                genetic.Begin(GetBitLength(), GetBitCount(), Fitness);
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
                return 0;
            }

            public ActionFillMeetingsAll(IEnumerable<Template> templates, IEnumerable<User> users) {
                _templates = templates;
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
