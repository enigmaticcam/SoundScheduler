using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Abstract {
    
    public class MeetingException {
        public DateTime Date { get; set; }
        public User User { get; set; }
        public bool IsSoftException { get; set; }
    }
}
