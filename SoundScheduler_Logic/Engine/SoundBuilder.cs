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
    //public class Repository {
    //    private string _filename;

    //    public SoundBuilder LoadSoundBuilder() {
    //        SoundBuilder sound;
    //        Stream stream = File.Open(_filename, FileMode.Open);
    //        BinaryFormatter formatter = new BinaryFormatter();
    //        sound = (SoundBuilder)formatter.Deserialize(stream);
    //        stream.Close();
    //        return sound;
            
    //    }
    //    public void SaveSoundBuilder(SoundBuilder sound) {
    //        Stream stream = File.Open(_filename, FileMode.Create);
    //        BinaryFormatter formatter = new BinaryFormatter();
    //        formatter.Serialize(stream, sound);
    //        stream.Close();
    //    }

    //    public Repository(string filename) {
    //        _filename = filename;
    //    }
    //}

    public class SoundBuilder {
        private int _consecutiveMeetingCountPerUserMax = 3;
        private List<Template> _templates;
        private List<Job> _jobs;
        private List<User> _users;
        private List<Meeting> _meetings;
        private List<ExistingMeeting> _existingMeetings;
        private List<int> _jobOrder;
        Dictionary<int, int> _jobCount;

        public List<Meeting> BuildSchedule() {
            CreateMeetings();
            AssembleJobsInOrder();
            AssignJobsToUsers();
            return _meetings;
        }

        private void CreateMeetings() {
            _meetings = new List<Meeting>();
            bool existingMeetingFound;
            for (int templateIndex = 0; templateIndex < _templates.Count; templateIndex++ ) {
                Template template = _templates[templateIndex];
                existingMeetingFound = false;
                for (int existingMeetingIndex = 0; existingMeetingIndex < _existingMeetings.Count; existingMeetingIndex++) {
                    ExistingMeeting existing = _existingMeetings[existingMeetingIndex];
                    if (existing.TemplateIndex == templateIndex) {
                        if (existing.Meeting.Name != template.Name) {
                            throw new Exception("Existing Template name must match Template name");
                        } else {
                            _meetings.Add(existing.Meeting);
                            existingMeetingFound = true;
                        }
                    }
                }
                if (!existingMeetingFound) {
                    Meeting meeting = new Meeting();
                    meeting.Name = template.Name;
                    meeting.Jobs = new List<Job>(template.Jobs);
                    meeting.JobUserSlots = new Meeting.JobUserSlot();
                    _meetings.Add(meeting);
                }                
            }
        }

        private void AssembleJobsInOrder() {
            FigureJobCount();
            FigureJobOrderBasedOnCount();
        }

        private void FigureJobCount() {
            _jobCount = new Dictionary<int, int>();
            foreach (User user in _users) {
                foreach (Job userJob in user.Jobs) {
                    for (int i = 0; i < _jobs.Count; i++) {
                        if (_jobs[i].Name == userJob.Name) {
                            if (_jobCount.ContainsKey(i)) {
                                _jobCount[i]++;
                            } else {
                                _jobCount.Add(i, 1);
                            }
                        }
                    }
                }
            }
        }

        private void FigureJobOrderBasedOnCount() {
            _jobOrder = new List<int>();
            foreach (KeyValuePair<int, int> keyValuePair in _jobCount.OrderBy(d => d.Value)) {
                _jobOrder.Add(keyValuePair.Key);
            }
        }

        private void AssignJobsToUsers() {
            int userIndex = 0;
            foreach (int jobOrderIndex in _jobOrder) {
                string jobName = _jobs[_jobOrder[jobOrderIndex]].Name;
                for (int meetingIndex = 0; meetingIndex < _meetings.Count; meetingIndex++) {
                    Meeting meeting = _meetings[meetingIndex];
                    for (int jobIndex = 0; jobIndex < meeting.Jobs.Count; jobIndex++) {
                        Job job = meeting.Jobs[jobIndex];
                        if (job.Name == jobName) {
                            if (meeting.JobUserSlots.UserForJob(jobIndex) == null) {
                                userIndex = AttemptToAssignUserToJob(meetingIndex, jobIndex, userIndex);                                
                            }
                        }
                    }
                }
            }
        }

        private int AttemptToAssignUserToJob(int meetingIndex, int jobIndex, int userIndex) {
            int initialUserIndex = userIndex;
            bool foundUser = false;
            do {
                if (CanUserDoJob(_meetings[meetingIndex].Jobs[jobIndex], _users[userIndex])) {
                    _meetings[meetingIndex].JobUserSlots.AddUserToJob(jobIndex, _users[userIndex]);
                    if (!IsMeetingStateValid()) {
                        _meetings[meetingIndex].JobUserSlots.RemoveUserFromJob(jobIndex);
                    } else {
                        foundUser = true;
                    }
                }
                userIndex++;
                if (userIndex == _users.Count) {
                    userIndex = 0;
                }
                if (userIndex == initialUserIndex && !foundUser) {
                    throw new Exception("Unable to find a valid user for meeting slot");
                }
            } while (!foundUser);
            return userIndex;
        }

        private bool IsMeetingStateValid() {
            HashSet<User> _usersWhoHaveJobInMeeting;
            for (int i = 0; i < _meetings.Count; i++ ) {
                Meeting meeting = _meetings[i];
                _usersWhoHaveJobInMeeting = new HashSet<User>();
                for (int j = 0; j < meeting.Jobs.Count; j++) {
                    if (meeting.JobUserSlots.UserForJob(j) != null) {
                        if (_usersWhoHaveJobInMeeting.Contains(meeting.JobUserSlots.UserForJob(j))) {
                            return false;
                        } else {
                            _usersWhoHaveJobInMeeting.Add(meeting.JobUserSlots.UserForJob(j));
                        }
                        if (ConsecutiveJobCountForUser(i, meeting.JobUserSlots.UserForJob(j)) > _consecutiveMeetingCountPerUserMax) {
                            return false;
                        }
                    }
                }                
            }
            return true;
        }

        private bool CanUserDoJob(Job job, User user) {
            for (int jobIndex = 0; jobIndex < user.Jobs.Count; jobIndex++) {
                if (user.Jobs[jobIndex].Name == job.Name) {
                    return true;
                }
            }
            return false;
        }

        private int ConsecutiveJobCountForUser(int currentMeetingIndex, User user) {
            if (user.Name == "Dennis Cook") {
                int x = 0;
                x++;
            }
            int count = 0;
            int meetingIndex = currentMeetingIndex;
            bool userFound;
            do {
                userFound = false;
                for (int jobIndex = 0; jobIndex < _meetings[meetingIndex].Jobs.Count; jobIndex++) {
                    if (_meetings[meetingIndex].JobUserSlots.UserForJob(jobIndex) == user) {
                        userFound = true;
                        count++;
                    }
                }
                meetingIndex--;
            } while (userFound && meetingIndex >= 0);
            return count;
        }

        public SoundBuilder(Builder builder) {
            _templates = builder.Templates;
            _jobs = builder.Jobs;
            _users = builder.Users;
            _existingMeetings = builder.ExistingMeetings;
        }

        public class Builder {
            public List<Template> Templates;
            public List<Job> Jobs;
            public List<User> Users;
            public List<ExistingMeeting> ExistingMeetings;

            public Builder SetTemplates(List<Template> templates) {
                this.Templates = templates;
                return this;
            }

            public Builder SetJobs(List<Job> jobs) {
                this.Jobs = jobs;
                return this;
            }

            public Builder SetUser(List<User> Users) {
                this.Users = Users;
                return this;
            }

            public Builder SetExistingMeetings(List<ExistingMeeting> existingMeetings) {
                this.ExistingMeetings = existingMeetings;
                return this;
            }

            public SoundBuilder Build() {
                return new SoundBuilder(this);
            }
        }

        public class ExistingMeeting {
            public int TemplateIndex { get; set; }
            public Meeting Meeting { get; set; }
        }
    }
}
