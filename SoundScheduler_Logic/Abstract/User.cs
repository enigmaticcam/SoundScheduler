using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    [Serializable]
    public class User {
        private int _id;
        public int Id {
            get { return _id; }
        }

        public string Name { get; set; }

        private Dictionary<int, Job> _jobs;
        public IEnumerable<Job> Jobs {
            get { return _jobs.Values; }
        }

        public void AddJob(Job job) {
            _jobs.Add(job.Id, job);
        }

        public User() {
            _jobs = new Dictionary<int, Job>();
        }

        public User(int id) : this() {
            _id = id;
        }
    }
}
