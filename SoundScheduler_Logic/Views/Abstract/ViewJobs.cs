using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Views.Abstract {
    public interface IViewJobs {
        List<Job> Jobs { get; }
        IEnumerable<Job> RelatedJobs(Job selectedJob);
        IEnumerable<Job> ApplicableRelatedJobs(Job selectedJob);
        void AddRelatedJob(Job selectedJob, Job relatedJob);
        void RemoveRelatedJob(Job selectedJob, Job relatedJob);
        void LoadFromSource();
        void SaveToSource();
    }

    public class ViewJobDecorator : IViewJobs {
        private IViewJobs _view;

        public virtual List<Job> Jobs {
            get { return _view.Jobs; }
        }

        public virtual IEnumerable<Job> RelatedJobs(Job selectedJob) {
            return _view.RelatedJobs(selectedJob);
        }

        public virtual IEnumerable<Job> ApplicableRelatedJobs(Job selectedJob) {
            return _view.ApplicableRelatedJobs(selectedJob);
        }

        public virtual void AddRelatedJob(Job selectedJob, Job relatedJob) {
            _view.AddRelatedJob(selectedJob, relatedJob);
        }

        public virtual void RemoveRelatedJob(Job selectedJob, Job relatedJob) {
            _view.RemoveRelatedJob(selectedJob, relatedJob);
        }

        public virtual void LoadFromSource() {
            _view.LoadFromSource();
        }

        public virtual void SaveToSource() {
            _view.SaveToSource();
        }

        public ViewJobDecorator(IViewJobs view) {
            _view = view;
        }
    }
}
