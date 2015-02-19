using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Views.Abstract;

namespace SoundScheduler_Logic.Views.Concrete {
    public class ViewUsers : IViewUsers {
        private Factory _factory;
        private Repository _repository;

        private List<User> _users;
        public List<User> Users {
            get {
                if (_users == null) {
                    return new List<User>();
                } else {
                    return _users;
                }
            }
        }

        public IEnumerable<Job> Jobs(User selectedUser) {
            foreach (Job job in selectedUser.Jobs) {
                yield return job;
            }
        }

        public IEnumerable<Job> ApplicableJobs(User selectedUser) {
            foreach (Job availableJob in _repository.Jobs) {
                bool isApplicable = true;
                foreach (Job userJob in selectedUser.Jobs) {
                    if (userJob.IsSameJob(availableJob)) {
                        isApplicable = false;
                        break;
                    }
                }
                if (isApplicable) {
                    yield return availableJob;
                }
            }
        }

        public void AddJob(User user, Job job) {
            user.Jobs.Add(job);
        }

        public void RemoveJob(User selectedUser, Job job) {
            selectedUser.Jobs.Remove(job);
        }

        public void LoadFromSource() {
            _repository = _factory.CreateRepository();
            _repository.LoadFromSource();
            _users = _repository.Users;
        }

        public void SaveToSource() {
            _repository.SaveToSource();
        }

        public ViewUsers(Factory factory) {
            _factory = factory;
        }
    }
}
