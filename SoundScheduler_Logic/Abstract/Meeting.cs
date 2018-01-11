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

        public int UniqueId { get; set; }

        public List<Job> Jobs {
            get { return _data.Jobs; }
            set { _data.Jobs = value; }
        }

        public Template TemplateParent {
            get { return _data.TemplateParent; }
        }

        public int PartitionCount {
            get { return _data.PartitionCount; }
            set { _data.PartitionCount = value; }
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

        public HashSet<int> PartitionsForJob(Job job) {
            if (_data.JobPartitions.ContainsKey(job)) {
                return _data.JobPartitions[job];
            } else {
                return new HashSet<int>();
            }
        }

        public void AddUserForJob(User user, Job job) {
            _data.JobUserSlots.Add(job, user);
            _data.UserJobSlots.Add(user, job);
        }

        public void AddJobToPartition(Job job, int partition) {
            if (!_data.JobPartitions.ContainsKey(job)) {
                _data.JobPartitions.Add(job, new HashSet<int>());
            }
            _data.JobPartitions[job].Add(partition);
        }

        public Template ToTemplate() {
            Template template = new Template(this.PartitionCount);
            template.UniqueId = this.UniqueId;
            foreach (Job job in this.Jobs) {
                template.Jobs.Add(job);
                foreach (int partition in this.PartitionsForJob(job)) {
                    template.AddJobToPartition(job, partition);
                }
            }
            return template;
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
            public Dictionary<Job, HashSet<int>> JobPartitions { get; set; }
            public int PartitionCount { get; set; }

            public Data() {
                this.Jobs = new List<Job>();
                this.JobUserSlots = new Dictionary<Job, User>();
                this.UserJobSlots = new Dictionary<User, Job>();
                this.JobPartitions = new Dictionary<Job, HashSet<int>>();
            }
        }
    }

    
}
