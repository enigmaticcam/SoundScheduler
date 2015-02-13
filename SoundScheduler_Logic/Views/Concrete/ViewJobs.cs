using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Views.Abstract;

namespace SoundScheduler_Logic.Views.Concrete {
    public class ViewJobs : IViewJobs {
        private Factory _factory;
        private Repository _repository;

        private List<Job> _jobs;
        public List<Job> Jobs {
            get {
                if (_jobs == null) {
                    return new List<Job>();
                } else {
                    return _jobs;
                }
            }
        }

        public IEnumerable<Job> RelatedJobs(Job selectedJob) {
            foreach (Job job in _jobs) {
                if (job != selectedJob && selectedJob.IsSameJob(job)) {
                    yield return job;
                }
            }
        }

        public virtual IEnumerable<Job> ApplicableRelatedJobs(Job selectedJob) {
            foreach (Job job in _jobs) {
                if (job != selectedJob && !selectedJob.IsSameJob(job)) {
                    yield return job;
                }
            }
        }

        public void AddRelatedJob(Job selectedJob, Job relatedJob) {
            selectedJob.AddSameJob(relatedJob);
        }

        public void RemoveRelatedJob(Job selectedJob, Job relatedJob) {
            selectedJob.RemoveSameJob(relatedJob);
        }

        public void LoadFromSource() {
            _repository = _factory.CreateRepository();
            _repository.LoadFromSource();
            _jobs = _repository.Jobs;
        }

        public void SaveToSource() {
            _repository.SaveToSource();
        }

        public ViewJobs(Factory factory) {
            _factory = factory;
        }
    }
}
