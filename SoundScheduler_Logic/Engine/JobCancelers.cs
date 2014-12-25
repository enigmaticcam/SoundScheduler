using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_Logic.Engine {
    public interface IJobCancel {
        HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics);
    }

    public class JobCancelUsersWhoHaveExceptions : IJobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;

        public HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
            _metrics = metrics;
            _usersToBeCancelled = new HashSet<User>();
            CancelUsers();
            return _usersToBeCancelled;
        }

        private void CancelUsers() {
            foreach (User user in _metrics.Users) {
                if (_metrics.DoesUserHaveException(user)) {
                    _usersToBeCancelled.Add(user);
                }
            }
        }
    }

    public class JobCancelUsersWhoAlreadyHaveJob : IJobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;

        public HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
            _metrics = metrics;
            _usersToBeCancelled = new HashSet<User>();
            CancelUsers();
            return _usersToBeCancelled;
        }

        private void CancelUsers() {
            foreach (User user in _metrics.Users) {
                if (_metrics.JobForUser(user) != null) {
                    _usersToBeCancelled.Add(user);
                }
            }
        }
    }

    public class JobCancelUsersWhoCantDoJob : IJobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;

        public HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
            _metrics = metrics;
            _usersToBeCancelled = new HashSet<User>();
            CancelUsers();
            return _usersToBeCancelled;
        }

        private void CancelUsers() {
            foreach (User user in _metrics.Users) {
                bool jobFound = false;
                foreach (Job job in user.Jobs) {
                    if (job == _metrics.CurrentJob) {
                        jobFound = true;
                        break;
                    }
                }
                if (!jobFound) {
                    _usersToBeCancelled.Add(user);
                }
            }
        }
    }

    public class JobCancelUserWhoNeedABreak : IJobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;
        private int _breakThreshold = 4;

        public HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
            _metrics = metrics;
            _usersToBeCancelled = new HashSet<User>();
            CancelUsers();
            return _usersToBeCancelled;
        }

        private void CancelUsers() {
            foreach (User user in _metrics.Users) {
                if (_metrics.UserContinuousCount(user) >= _breakThreshold) {
                    _usersToBeCancelled.Add(user);
                }
            }
        }
    }
}
