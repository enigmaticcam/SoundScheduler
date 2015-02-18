using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace SoundScheduler_Logic.Abstract {
    public class Job : IXmlSerializable {
        private HashSet<int> _sameJobs;

        private int _id;
        public int Id {
            get { return _id; }
        }

        public string Name { get; set; }
        public bool IsVoidedOnSoftException { get; set; }

        public void AddSameJob(Job job) {
            if (!_sameJobs.Contains(job.Id)) {
                _sameJobs.Add(job.Id);
                job.AddSameJob(this);
            }
        }

        public void RemoveSameJob(Job job) {
            if (_sameJobs.Contains(job.Id)) {
                _sameJobs.Remove(job.Id);
                job.RemoveSameJob(this);
            }
        }

        public bool IsSameJob(Job job) {
            if (job.Id == _id || _sameJobs.Contains(job.Id)) {
                return true;
            } else {
                return false;
            }
        }

        public Job() {
            _sameJobs = new HashSet<int>();
        }

        public Job(int id) : this() {
            _id = id;
        }

        public void WriteXml(XmlWriter writer) {
            
        }

        public void ReadXml(XmlReader reader) {

        }

        public XmlSchema GetSchema() {
            return null;
        }
    }
}
