using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    [Serializable]
    public class Template {
        public string Name { get; set; }

        private Dictionary<int, Job> _jobs;
        public IEnumerable<Job> Jobs {
            get { return _jobs.Values; }
        }

        public void AddJob(Job job) {
            _jobs.Add(job.Id, job);
        }

        public Meeting ToMeeting(DateTime date) {
            Meeting meeting = new Meeting(this);
            meeting.Date = date;
            meeting.Name = this.Name + " - " + date.ToShortDateString();
            foreach (Job job in this.Jobs) {
                meeting.AddJob(job);
            }
            return meeting;
        }

        public Template() {
            _jobs = new Dictionary<int, Job>();
        }
    }
}
