using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Job  {
        private Data _data;

        public IEnumerable<Job> SameJobs {
            get { return _data.SameJobs; }
            set { _data.SameJobs = value.ToList(); }
        }

        public string Name {
            get { return _data.Name; }
            set { _data.Name = value; }
        }

        public bool IsVoidedOnSoftException {
            get { return _data.IsVoidedOnSoftException; }
            set { _data.IsVoidedOnSoftException = value; }
        }

        public void AddSameJob(Job job) {
            if (!_data.SameJobs.Contains(job)) {
                _data.SameJobs.Add(job);
                job.AddSameJob(this);
            }
        }

        public void RemoveSameJob(Job job) {
            if (_data.SameJobs.Contains(job)) {
                _data.SameJobs.Remove(job);
                job.RemoveSameJob(this);
            }
        }

        public bool IsSameJob(Job job) {
            if (job == this || _data.SameJobs.Contains(job)) {
                return true;
            } else {
                return false;
            }
        }

        public Job() {
            _data = new Data();
        }

        public Job(Data data) {
            _data = data;
        }

        public class Data {
            public List<Job> SameJobs { get; set; }
            public string Name { get; set; }
            public bool IsVoidedOnSoftException { get; set; }

            public Data() {
                this.SameJobs = new List<Job>();
            }
        }
    }
}
