using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_Logic.Engine {
    public abstract class JobCancel {
        public abstract HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics);
        public abstract bool CanAddBack { get; }
    }

    public class JobCancelUsersWhoHaveBeenUsedMore : JobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;
        Dictionary<User, int> _userCounts;

        public override bool CanAddBack {
            get { return true; }
        }

        public override HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
            _metrics = metrics;
            _usersToBeCancelled = new HashSet<User>();
            CancelUsers();
            return _usersToBeCancelled;
        }

        private void CancelUsers() {
            BuildUserCounts();
            int lowestCount = GetLowestCount();
            CancelUsersHigherThanCount(lowestCount);
        }

        private void BuildUserCounts() {
            foreach (User user in _metrics.Users) {
                _userCounts.Add(user, _metrics.UserTotalCount(user));
            }
        }

        private int GetLowestCount() {
            return _userCounts.Values.OrderBy(c => c).Take(1).ElementAt(0);
        }

        private void CancelUsersHigherThanCount(int count) {
            foreach (User user in _metrics.Users) {
                int userCount = 0;
                if (_userCounts.ContainsKey(user)) {
                    userCount = _userCounts[user];
                }
                if (userCount > count) {
                    _usersToBeCancelled.Add(user);
                }
            }
        }

        public JobCancelUsersWhoHaveBeenUsedMore() {
            _userCounts = new Dictionary<User, int>();
        }
    }

    public class JobCancelUsersWhoHaveExceptions : JobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;

        public override bool CanAddBack {
            get { return false; }
        }

        public override HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
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

    public class JobCancelUsersWhoAlreadyHaveJob : JobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;

        public override bool CanAddBack {
            get { return false; }
        }

        public override HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
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

    public class JobCancelUsersWhoCantDoJob : JobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;

        public override bool CanAddBack {
            get { return false; }
        }

        public override HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
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

    public class JobCancelUserWhoNeedABreak : JobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;
        private int _breakThreshold = 4;

        public override bool CanAddBack {
            get { return true; }
        }

        public override HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
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

    public class JobCancelPickARandomUser : JobCancel {
        SoundBuilderV2.SoundMetrics _metrics;
        HashSet<User> _usersToBeCancelled;
        private Random _random;

        public override bool CanAddBack {
            get { return true; }
        }

        public override HashSet<User> CancelUsers(SoundBuilderV2.SoundMetrics metrics) {
            _metrics = metrics;
            _usersToBeCancelled = new HashSet<User>();
            CancelUsers();
            return _usersToBeCancelled;
        }

        private void CancelUsers() {
            int randomUser = _random.Next(0, _metrics.Users.Count());
            for (int i = 0; i < _metrics.Users.Count(); i++) {
                if (i != randomUser) {
                    _usersToBeCancelled.Add(_metrics.Users.ElementAt(i));
                }
            }
        }

        public JobCancelPickARandomUser(Random random) {
            _random = random;
        }
    }
}
