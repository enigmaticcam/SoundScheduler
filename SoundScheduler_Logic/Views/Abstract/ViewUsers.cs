using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Views.Abstract {
    public interface IViewUsers {
        List<User> Users { get; }
        IEnumerable<Job> Jobs(User selectedUser);
        IEnumerable<Job> ApplicableJobs(User selectedUser);
        void AddJob(User selectedUser, Job job);
        void RemoveJob(User selectedUser, Job job);
        void LoadFromSource();
        void SaveToSource();
    }

    public abstract class ViewUsersDecorator : IViewUsers {
        private IViewUsers _view;

        public List<User> Users {
            get { return _view.Users; }
        }

        public IEnumerable<Job> Jobs(User selectedUser) {
            return _view.Jobs(selectedUser);
        }

        public IEnumerable<Job> ApplicableJobs(User selectedUser) {
            return _view.ApplicableJobs(selectedUser);
        }

        public void AddJob(User selectedUser, Job job) {
            _view.AddJob(selectedUser, job);
        }

        public void RemoveJob(User selectedUser, Job job) {
            _view.RemoveJob(selectedUser, job);
        }

        public void LoadFromSource() {
            _view.LoadFromSource();
        }

        public void SaveToSource() {
            _view.SaveToSource();
        }

        public ViewUsersDecorator(IViewUsers view) {
            _view = view;
        }
    }
}
