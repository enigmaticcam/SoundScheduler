using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Meeting {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public List<Job> Jobs { get; set; }

        private Dictionary<Job, User> _jobUserSlots;
        public User UserForJob(Job job) {
            if (_jobUserSlots.ContainsKey(job)) {
                return _jobUserSlots[job];
            } else {
                return null;
            }
        }

        private Dictionary<User, Job> _userJobSlots;
        public Job JobForUser(User user) {
            if (_userJobSlots.ContainsKey(user)) {
                return _userJobSlots[user];
            } else {
                return null;
            }
        }

        public void AddUserForJob(User user, Job job) {
            _jobUserSlots.Add(job, user);
            _userJobSlots.Add(user, job);
        }
        

        public Meeting() {
            this.Jobs = new List<Job>();
            _jobUserSlots = new Dictionary<Job, User>();
            _userJobSlots = new Dictionary<User, Job>();
        }
    }

    
}
