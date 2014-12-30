using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Template {
        public string Name { get; set; }
        public List<Job> Jobs { get; set; }

        public Meeting ToMeeting(DateTime date) {
            Meeting meeting = new Meeting(this);
            meeting.Date = date;
            meeting.Name = this.Name + " - " + date.ToShortDateString();
            foreach (Job job in this.Jobs) {
                meeting.Jobs.Add(job);
            }
            return meeting;
        }

        public Template() {
            this.Jobs = new List<Job>();
        }
    }
}
