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
