using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class User {
        public string Name { get; set; }
        public List<Job> Jobs { get; set; }
    }
}
