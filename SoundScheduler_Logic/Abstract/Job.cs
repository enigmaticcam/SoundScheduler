using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SoundScheduler_Logic.Abstract {
    public class Job {
        private HashSet<Job> _sameJobs;

        public string Name { get; set; }
        public bool IsVoidedOnSoftException { get; set; }

        public void AddSameJob(Job job) {
            if (!_sameJobs.Contains(job)) {
                _sameJobs.Add(job);
                job.AddSameJob(this);
            }
        }

        public void RemoveSameJob(Job job) {
            if (_sameJobs.Contains(job)) {
                _sameJobs.Remove(job);
                job.RemoveSameJob(this);
            }
        }

        public bool IsSameJob(Job job) {
            return _sameJobs.Contains(job);
        }

        public Job() {
            _sameJobs = new HashSet<Job> { this };
        }
    }
}
