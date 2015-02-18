using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Job  {
        public List<Job> SameJobs { get; set; }
        public string Name { get; set; }
        public bool IsVoidedOnSoftException { get; set; }

        public void AddSameJob(Job job) {
            if (!this.SameJobs.Contains(job)) {
                this.SameJobs.Add(job);
                job.AddSameJob(this);
            }
        }

        public void RemoveSameJob(Job job) {
            if (this.SameJobs.Contains(job)) {
                this.SameJobs.Remove(job);
                job.RemoveSameJob(this);
            }
        }

        public bool IsSameJob(Job job) {
            if (job == this || this.SameJobs.Contains(job)) {
                return true;
            } else {
                return false;
            }
        }

        public Job() {
            this.SameJobs = new List<Job>();
        }
    }
}
