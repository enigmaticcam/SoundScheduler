using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Meeting {
        public string Name { get; set; }
        public DateTime Date { get; set; }

        private Dictionary<int, Job> _jobs;
        public IEnumerable<Job> Jobs {
            get { return _jobs.Values; }
        }

        private Template _templateParent;
        public Template TemplateParent {
            get { return _templateParent; }
        }

        public void AddJob(Job job) {
            _jobs.Add(job.Id, job);
        }

        private Dictionary<int, User> _jobUserSlots;
        public User UserForJob(Job job) {
            if (_jobUserSlots.ContainsKey(job.Id)) {
                return _jobUserSlots[job.Id];
            } else {
                return null;
            }
        }

        private Dictionary<int, Job> _userJobSlots;
        public Job JobForUser(User user) {
            if (_userJobSlots.ContainsKey(user.Id)) {
                return _userJobSlots[user.Id];
            } else {
                return null;
            }
        }

        public void AddUserForJob(User user, Job job) {
            _jobUserSlots.Add(job.Id, user);
            _userJobSlots.Add(user.Id, job);
        }

        public Meeting(Template templateParent) {
            _templateParent = templateParent;
            _jobs = new Dictionary<int, Job>();
            _jobUserSlots = new Dictionary<int, User>();
            _userJobSlots = new Dictionary<int, Job>();
        }
    }

    
}
