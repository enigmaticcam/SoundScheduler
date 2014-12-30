using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Engine {
    public class SoundBuilderV2 {
        private SoundMetrics _metrics;
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

        public void AddException(DateTime date, User user) {
            _exceptions.AddException(date, user);
        }

        public List<Meeting> BuildSchedule() {
            CreateMeetingsFromTemplates();
            FillMeetingsAll();
            return null;
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
            foreach (DateTime date in _meetings.Keys) {
                FillMeeting(date);
            }
        }

        private void FillMeeting(DateTime date) {
            Meeting meeting = _meetings.GetMeeting(date);
            foreach (Job job in meeting.Jobs) {
                if (meeting.UserForJob(job) == null) {
                    FillJob(date, job);
                }
            }
        }

        private void FillJob(DateTime date, Job job) {
            ActionFillJob action = new ActionFillJob.Builder()
                .SetDate(date)
                .SetJob(job)
                .SetExceptions(_exceptions)
                .SetJobCancelers(GetJobCancelers())
                .SetMeetings(_meetings)
                .SetUsers(_users)
                .Build();
            action.PerformAction();
            
        }

        private List<JobCancel> GetJobCancelers() {
            List<JobCancel> jobCancelers = new List<JobCancel>();
            jobCancelers.Add(new JobCancelUsersWhoAlreadyHaveJob());
            jobCancelers.Add(new JobCancelUsersWhoHaveExceptions());
            jobCancelers.Add(new JobCancelUsersWhoCantDoJob());
            jobCancelers.Add(new JobCancelUserWhoNeedABreak());
            return jobCancelers;
        }

        public class ActionFillJob {
            private MeetingsByDate _meetings;
            private ExceptionsByDate _exceptions;
            private DateTime _date;
            private Job _job;
            private HashSet<User> _users;
            private HashSet<User> _usersAvailable;
            private IEnumerable<JobCancel> _jobCancelers;
            private List<HashSet<User>> _usersCanceled;
            private Random _random;

            public User PerformAction() {
                CancelJobsForAll();
                ReverseCancels();
                RemoveUsersWhoHaveBeenUsedTheMost();
                PickAUserRandomly();
                return GetOnlyUser();
            }

            private void CancelJobsForAll() {
                _usersAvailable = new HashSet<User>(_users);
                for (int i = 0; i < _jobCancelers.Count(); i++) {
                    SoundMetrics metrics = CreateMetrics();
                    HashSet<User> usersToRemove = _jobCancelers.ElementAt(i).CancelUsers(metrics);
                    RemoveUsersFromAvailableUsers(usersToRemove);
                    _usersCanceled.Add(usersToRemove);
                }
            }

            private void ReverseCancels() {
                int jobCancelerIndex = _jobCancelers.Count() - 1;
                while (_usersAvailable.Count == 0) {
                    if (!_jobCancelers.ElementAt(jobCancelerIndex).CanAddBack) {
                        throw new Exception("could not find a user for job");
                    } else {
                        AddBackUsers(_usersCanceled[jobCancelerIndex]);
                    }
                    jobCancelerIndex--;
                }
            }

            private void RemoveUsersWhoHaveBeenUsedTheMost() {
                SoundMetrics metrics = CreateMetrics();
                JobCancel jobCancel = new JobCancelUsersWhoHaveBeenUsedMore();
                HashSet<User> usersToRemove = jobCancel.CancelUsers(metrics);
                RemoveUsersFromAvailableUsers(usersToRemove);
            }

            private void PickAUserRandomly() {
                SoundMetrics metrics = CreateMetrics();
                JobCancel jobCancel = new JobCancelPickARandomUser(_random);
                HashSet<User> usersToRemove = jobCancel.CancelUsers(metrics);
                RemoveUsersFromAvailableUsers(usersToRemove);
            }

            private User GetOnlyUser() {
                if (_usersAvailable.Count > 1) {
                    throw new SoundBuilderTooManyUsersException();
                } else {
                    return _usersAvailable.ElementAt(0);
                }
            }

            private SoundMetrics CreateMetrics() {
                return new SoundMetrics.Builder()
                    .SetCurrentDate(_date)
                    .SetCurrentJob(_job)
                    .SetExceptions(_exceptions)
                    .SetMeetings(_meetings)
                    .SetUsers(_usersAvailable)
                    .Build();
            }

            private void RemoveUsersFromAvailableUsers(HashSet<User> usersToRemove) {
                foreach (User user in usersToRemove) {
                    _usersAvailable.Remove(user);
                }
            }

            private void AddBackUsers(HashSet<User> usersToAdd) {
                foreach (User user in usersToAdd) {
                    _usersAvailable.Add(user);
                }
            }

            public ActionFillJob(Builder builder) {
                _meetings = builder.Meetings;
                _date = builder.Date;
                _job = builder.Job;
                _exceptions = builder.Exceptions;
                _users = builder.Users;
                _jobCancelers = builder.JobCancelers;
                _random = builder.Rnd;
                if (_random == null) {
                    _random = new Random();
                }
                _usersCanceled = new List<HashSet<User>>();
                _usersAvailable = new HashSet<User>();
            }

            public class Builder {
                public MeetingsByDate Meetings;
                public ExceptionsByDate Exceptions;
                public DateTime Date;
                public Job Job;
                public HashSet<User> Users;
                public IEnumerable<JobCancel> JobCancelers;
                public Random Rnd;

                public Builder SetMeetings(MeetingsByDate meetings) {
                    this.Meetings = meetings;
                    return this;
                }

                public Builder SetExceptions(ExceptionsByDate exceptions) {
                    this.Exceptions = exceptions;
                    return this;
                }

                public Builder SetDate(DateTime date) {
                    this.Date = date;
                    return this;
                }

                public Builder SetJob(Job job) {
                    this.Job = job;
                    return this;
                }

                public Builder SetUsers(HashSet<User> users) {
                    this.Users = users;
                    return this;
                }

                public Builder SetJobCancelers(IEnumerable<JobCancel> jobCancelers) {
                    this.JobCancelers = jobCancelers;
                    return this;
                }

                public Builder SetRandom(Random rnd) {
                    this.Rnd = rnd;
                    return this;
                }

                public ActionFillJob Build() {
                    return new ActionFillJob(this);
                }
            }
        }

        public SoundBuilderV2(Builder builder) {
            _jobs = builder.Jobs;
            _users = builder.Users;
            _templates = new Dictionary<DateTime, Template>();
            _meetings = new MeetingsByDate();
            _exceptions = new ExceptionsByDate();
        }

        public class Builder {
            public List<Job> Jobs;
            public HashSet<User> Users;

            public Builder SetJobs(List<Job> jobs) {
                this.Jobs = jobs;
                return this;
            }

            public Builder SetUser(HashSet<User> Users) {
                this.Users = Users;
                return this;
            }

            public SoundBuilderV2 Build() {
                return new SoundBuilderV2(this);
            }
        }

        public class SoundMetrics {
            private MeetingsByDate _meetings;
            private ExceptionsByDate _exceptions;
            private HashSet<User> _users;

            private DateTime _currentDate;
            public DateTime CurrentDate {
                get { return _currentDate; }
            }

            private Job _currentJob;
            public Job CurrentJob {
                get { return _currentJob; }
            }

            public IEnumerable<User> Users {
                get { return _users; }
            }

            public Job JobForUser(User user) {
                return _meetings.GetMeeting(_currentDate).JobForUser(user);
            }

            public int UserContinuousCount(User user) {
                int userCount = 0;
                foreach (DateTime date in _meetings.Keys) {
                    if (DateTime.Compare(date, _currentDate) < 0) {
                        Meeting meeting = _meetings.GetMeeting(date);
                        if (meeting.JobForUser(user) != null) {
                            userCount++;
                        } else {
                            userCount = 0;
                        }
                    }
                }
                return userCount;
            }

            public int UserTotalCount(User user) {
                int userCount = 0;
                foreach (DateTime date in _meetings.Keys) {
                    if (DateTime.Compare(date, _currentDate) < 0) {
                        Meeting meeting = _meetings.GetMeeting(date);
                        if (meeting.JobForUser(user) != null) {
                            userCount++;
                        }
                    }
                }
                return userCount;
            }

            public bool DoesUserHaveException(User user) {
                return _exceptions.DoesUserHaveException(_currentDate, user);
            }

            public SoundMetrics(Builder builder) {
                _meetings = builder.Meetings;
                _users = builder.Users;
                _currentDate = builder.CurrentDate;
                _currentJob = builder.CurrentJob;
                _exceptions = builder.Exceptions;
            }

            public class Builder {
                public MeetingsByDate Meetings;
                public ExceptionsByDate Exceptions;
                public HashSet<User> Users;
                public DateTime CurrentDate;
                public Job CurrentJob;

                public Builder SetMeetings(MeetingsByDate meetings) {
                    this.Meetings = meetings;
                    return this;
                }

                public Builder SetExceptions(ExceptionsByDate exceptions) {
                    this.Exceptions = exceptions;
                    return this;
                }

                public Builder SetUsers(HashSet<User> users) {
                    this.Users = users;
                    return this;
                }

                public Builder SetCurrentDate(DateTime currentDate) {
                    this.CurrentDate = currentDate;
                    return this;
                }

                public Builder SetCurrentJob(Job currentJob) {
                    this.CurrentJob = currentJob;
                    return this;
                }

                public SoundMetrics Build() {
                    return new SoundMetrics(this);
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
            private Dictionary<DateTime, HashSet<User>> _exceptions;

            public void AddException(DateTime date, User user) {
                if (!_exceptions.ContainsKey(date)) {
                    _exceptions.Add(date, new HashSet<User>());
                }
                _exceptions[date].Add(user);
            }

            public bool DoesUserHaveException(DateTime date, User user) {
                if (_exceptions.ContainsKey(date) && _exceptions[date].Contains(user)) {
                    return true;
                } else {
                    return false;
                }
            }

            public ExceptionsByDate() {
                _exceptions = new Dictionary<DateTime, HashSet<User>>();
            }
        }
    }

    public class SoundBuilderTooManyUsersException : Exception {
        
    }
}
