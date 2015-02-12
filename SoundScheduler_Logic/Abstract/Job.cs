using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    [Serializable]
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

        public bool IsSameJob(Job job) {
            return _sameJobs.Contains(job);
        }

        public Job() {
            _sameJobs = new HashSet<Job> { this };
        }
    }
}
