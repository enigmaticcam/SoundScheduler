using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Views.Abstract;

namespace SoundScheduler_Logic.Views.Concrete {
    public class ViewTemplates : IViewTemplates {
        private Factory _factory;
        private Repository _repository;

        private List<Template> _templates;
        public List<Template> Templates {
            get {
                if (_templates == null) {
                    return new List<Template>();
                } else {
                    return _templates;
                }
            }
        }

        public IEnumerable<Job> Jobs(Template selectedTemplate) {
            foreach (Job job in selectedTemplate.Jobs) {
                yield return job;
            }
        }

        public IEnumerable<Job> ApplicableJobs(Template selectedTemplate) {
            foreach (Job availableJob in _repository.Jobs) {
                bool isApplicable = true;
                foreach (Job userJob in selectedTemplate.Jobs) {
                    if (userJob == availableJob) {
                        isApplicable = false;
                        break;
                    }
                }
                if (isApplicable) {
                    yield return availableJob;
                }
            }
        }

        public void AddJob(Template selectedTemplate, Job job) {
            selectedTemplate.Jobs.Add(job);
        }

        public void RemoveJob(Template selectedTemplate, Job job) {
            selectedTemplate.Jobs.Remove(job);
        }

        public void LoadFromSource() {
            _repository = _factory.CreateRepository();
            _repository.LoadFromSource();
            _templates = _repository.Templates;
        }

        public void SaveToSource() {
            _repository.SaveToSource();
        }

        public ViewTemplates(Factory factory) {
            _factory = factory;
        }
    }
}
