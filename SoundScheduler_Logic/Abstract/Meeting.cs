using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Meeting {
        private Data _data;

        public string Name {
            get { return _data.Name; }
            set { _data.Name = value; }
        }

        public DateTime Date {
            get { return _data.Date; }
            set { _data.Date = value; }
        }

        public List<Job> Jobs {
            get { return _data.Jobs; }
            set { _data.Jobs = value; }
        }

        public Template TemplateParent {
            get { return _data.TemplateParent; }
        }

        public User UserForJob(Job job) {
            if (_data.JobUserSlots.ContainsKey(job)) {
                return _data.JobUserSlots[job];
            } else {
                return null;
            }
        }

        public Job JobForUser(User user) {
            if (_data.UserJobSlots.ContainsKey(user)) {
                return _data.UserJobSlots[user];
            } else {
                return null;
            }
        }

        public void AddUserForJob(User user, Job job) {
            _data.JobUserSlots.Add(job, user);
            _data.UserJobSlots.Add(user, job);
        }

        public Meeting(Template templateParent) {
            _data = new Data();
            _data.TemplateParent = templateParent;
        }

        public Meeting(Data data) {
            _data = data;
        }

        public class Data {
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public List<Job> Jobs { get; set; }
            public Template TemplateParent { get; set; }
            public Dictionary<Job, User> JobUserSlots { get; set; }
            public Dictionary<User, Job> UserJobSlots { get; set; }

            public Data() {
                this.Jobs = new List<Job>();
                this.JobUserSlots = new Dictionary<Job, User>();
                this.UserJobSlots = new Dictionary<User, Job>();
            }
        }
    }

    
}
