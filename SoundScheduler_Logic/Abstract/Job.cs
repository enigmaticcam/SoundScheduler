using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Job  {
        private Data _data;
        public Data JobData {
            get { return _data; }
            set { _data = value; }
        }

        public IEnumerable<Job> SameJobs {
            get { return _data.SameJobs.Select(x => new Job(x)); }
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
            if (!_data.SameJobs.Contains(job.JobData)) {
                _data.SameJobs.Add(job.JobData);
                job.AddSameJob(this);
            }
        }

        public void RemoveSameJob(Job job) {
            if (_data.SameJobs.Contains(job.JobData)) {
                _data.SameJobs.Remove(job.JobData);
                job.RemoveSameJob(this);
            }
        }

        public bool IsSameJob(Job job) {
            if (job.JobData == _data || _data.SameJobs.Contains(job.JobData)) {
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

        public bool IsSameReference(Job job) {
            return _data == job.JobData;
        }

        public class Data {
            public List<Job.Data> SameJobs { get; set; }
            public string Name { get; set; }
            public bool IsVoidedOnSoftException { get; set; }

            public Data() {
                this.SameJobs = new List<Job.Data>();
            }
        }
    }
}
