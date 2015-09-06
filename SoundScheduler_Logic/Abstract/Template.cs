using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    
    public class Template {
        private Dictionary<Job, HashSet<int>> _partitions = new Dictionary<Job, HashSet<int>>();
        private HashSet<int> _validPartitions = new HashSet<int>();

        public string Name { get; set; }
        public List<Job> Jobs { get; set; }

        private int _partitionCount;
        public int ParitionCount {
            get { return _partitionCount; }
        }

        public HashSet<int> PartitionsForJob(Job job) {
            if (_partitions.ContainsKey(job)) {
                return _partitions[job];
            } else {
                return new HashSet<int>();
            }
        }

        private List<int> _validPartitionsAsEnumerable = new List<int>();
        public IEnumerable<int> ValidPartitions() {
            return _validPartitionsAsEnumerable;
        }

        public bool HasPartitionForJob(Job job) {
            return _partitions.ContainsKey(job);
        }

        public Meeting ToMeeting(DateTime date) {
            Meeting meeting = new Meeting(this);
            meeting.Date = date;
            meeting.Name = this.Name + " - " + date.ToShortDateString();
            foreach (Job job in this.Jobs) {
                meeting.Jobs.Add(job);
                if (HasPartitionForJob(job)) {
                    AddJobToPartition(job, 0);
                }
                foreach (int partition in PartitionsForJob(job)) {
                    meeting.AddJobToPartition(job, partition);
                }
            }
            return meeting;
        }

        public void AddJobToPartition(Job job, int partition) {
            if (partition > _partitionCount || partition < 1) {
                throw new Exception("Invalid partition");
            }
            if (!_partitions.ContainsKey(job)) {
                _partitions.Add(job, new HashSet<int>());
            }
            _partitions[job].Add(partition);
            AddValidPartition(partition);
        }

        private void AddValidPartition(int partition) {
            _validPartitions.Add(partition);
            _validPartitionsAsEnumerable = _validPartitions.ToList();
        }

        public void AddJobToAllPartitions(Job job) {
            for (int i = 1; i <= _partitionCount; i++) {
                AddJobToPartition(job, i);
            }
        }

        public Template() {
            _partitionCount = 1;
            Instantiate();
        }

        public Template(int partitionCount)  {
            _partitionCount = partitionCount;
            Instantiate();
        }

        private void Instantiate() {
            this.Jobs = new List<Job>();
        }
    }
}
