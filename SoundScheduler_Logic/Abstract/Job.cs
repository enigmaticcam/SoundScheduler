using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Job {
        public string Name { get; set; }
        public bool IsVoidedOnSoftException { get; set; }
    }
}
