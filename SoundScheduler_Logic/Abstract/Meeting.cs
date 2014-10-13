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
        public JobUserSlot JobUserSlots { get; set; }

        public class JobUserSlot {
            private Dictionary<int, User> _data = new Dictionary<int, User>();

            public void AddUserToJob(int jobIndex, User user) {
                _data.Add(jobIndex, user);
            }

            public void RemoveUserFromJob(int jobIndex) {
                _data.Remove(jobIndex);
            }

            public User UserForJob(int jobIndex) {
                if (_data.ContainsKey(jobIndex)) {
                    return _data[jobIndex];
                } else {
                    return null;
                }
            }
        }
    }

    
}
