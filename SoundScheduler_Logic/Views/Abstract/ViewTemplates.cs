using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Views.Abstract {
    public interface IViewTemplates {
        List<Template> Templates { get; }
        IEnumerable<Job> Jobs(Template selectedUser);
        IEnumerable<Job> ApplicableJobs(Template selectedTemplate);
        void AddJob(Template selectedTemplate, Job job);
        void RemoveJob(Template selectedTemplate, Job job);
        void LoadFromSource();
        void SaveToSource();
    }

    public abstract class ViewTemplatesDecorator : IViewTemplates {
        private IViewTemplates _view;

        public List<Template> Templates {
            get { return _view.Templates; }
        }

        public IEnumerable<Job> Jobs(Template selectedTemplate) {
            return _view.Jobs(selectedTemplate);
        }

        public IEnumerable<Job> ApplicableJobs(Template selectedTemplate) {
            return _view.ApplicableJobs(selectedTemplate);
        }

        public void AddJob(Template selectedTemplate, Job job) {
            _view.AddJob(selectedTemplate, job);
        }

        public void RemoveJob(Template selectedTemplate, Job job) {
            _view.RemoveJob(selectedTemplate, job);
        }

        public void LoadFromSource() {
            _view.LoadFromSource();
        }

        public void SaveToSource() {
            _view.SaveToSource();
        }

        public ViewTemplatesDecorator(IViewTemplates view) {
            _view = view;
        }
    }
}
