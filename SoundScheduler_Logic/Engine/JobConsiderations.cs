using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Engine {

    public abstract class JobConsideration {
        public abstract bool IsConsiderationSoft { get; }
        public abstract float IsValid(int[] usersInJobs);
        public abstract string JobName { get; }
        public int JobRank { get; set; }

        private IEnumerable<Template> _templates;
        public IEnumerable<Template> Templates {
            get { return _templates; }
        }

        private IEnumerable<User> _users;
        public IEnumerable<User> Users {
            get { return _users; }
        }

        private IEnumerable<Job> _jobs;
        public IEnumerable<Job> Jobs {
            get { return _jobs; }
        }

        private int _solutionCount;
        public int SolutionCount {
            get { return _solutionCount; }
        }

        private int GetSolutionCount() {
            int count = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    ++count;
                }
            }
            return count;
        }

        protected int CountBitsInBitsArray(BitArray bitArray) {
            Int32[] ints = new Int32[(bitArray.Count >> 5) + 1];
            bitArray.CopyTo(ints, 0);
            Int32 count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[ints.Length - 1] &= ~(-1 << (bitArray.Count % 32));

            for (Int32 i = 0; i < ints.Length; i++) {
                Int32 c = ints[i];

                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked {
                    c = c - ((c >> 1) & 0x55555555);
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }
                count += c;
            }
            return count;
        }

        public JobConsideration(BuilderBase builder) {
            _templates = builder.Templates;
            _users = builder.Users;
            _jobs = builder.Jobs;
            _solutionCount = GetSolutionCount();
        }

        public abstract class BuilderBase {
            public abstract JobConsideration Build();
            public IEnumerable<Template> Templates;
            public IEnumerable<User> Users;
            public IEnumerable<Job> Jobs;

            public BuilderBase SetTemplates(IEnumerable<Template> templates) {
                this.Templates = templates;
                return this;
            }

            public BuilderBase SetUsers(IEnumerable<User> users) {
                this.Users = users;
                return this;
            }

            public BuilderBase SetJobs(IEnumerable<Job> jobs) {
                this.Jobs = jobs;
                return this;
            }
        }
    }

    public class JobConsiderationUsersWhoCantDoJob : JobConsideration {
        private BitArray _matrix;

        public override string JobName {
            get { return "Users Who Can't Do Job"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public override float IsValid(int[] usersInJobs) {
            BitArray solution = new BitArray(this.SolutionCount * this.Users.Count());
            for (int i = 0; i <= usersInJobs.GetUpperBound(0); i++) {
                int position = usersInJobs[i] * this.SolutionCount + i;
                solution.Set(position, true);
            }
            BitArray cantDoJobs = solution.And(_matrix);
            int bitCount = CountBitsInBitsArray(cantDoJobs);
            return bitCount;
        }

        public JobConsiderationUsersWhoCantDoJob(Builder builder) : base(builder) {
            BuildMatrixOfUsersWhoCantDoJobs();
        }

        private void BuildMatrixOfUsersWhoCantDoJobs() {
            _matrix = new BitArray(this.SolutionCount * this.Users.Count());
            LoopThroughJobsInTemplates();
        }

        private void LoopThroughJobsInTemplates() {
            int matrixIndex = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    FindUsersWhoCantDoJob(job, matrixIndex);
                    ++matrixIndex;
                }
            }
        }

        private void FindUsersWhoCantDoJob(Job job, int matrixIndex) {
            for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                int positionStart = userIndex * this.SolutionCount;
                User user = this.Users.ElementAt(userIndex);
                if (user.Jobs.IndexOf(job) == -1) {
                    _matrix.Set(positionStart + matrixIndex, true);
                }
            }
        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoCantDoJob(this);
            }
        }
    }

    public class JobConsiderationUsersWhoAlreadyHaveJob : JobConsideration {
        private Dictionary<int, bool> _usersInDay = new Dictionary<int, bool>();
        private Dictionary<bool, Dictionary<bool, bool>> _softExceptionChange = new Dictionary<bool, Dictionary<bool, bool>>();
        private int _counter;
        private float _exceptionCount;
        private float _zeroPointFive = (float)0.5;

        public override string JobName {
            get { return "Users Who Already Have Job"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public override float IsValid(int[] usersInJobs) {
            _counter = 0;
            _exceptionCount = 0;
            foreach (Template template in this.Templates) {
                _usersInDay.Clear();
                foreach (Job job in template.Jobs) {
                    if (_usersInDay.ContainsKey(usersInJobs[_counter])) {
                        if (!_usersInDay[usersInJobs[_counter]] || !job.IsVoidedOnSoftException) {
                            _exceptionCount += _zeroPointFive;
                        } else {
                            _exceptionCount += 1;
                        }
                        _usersInDay[usersInJobs[_counter]] = _softExceptionChange[_usersInDay[usersInJobs[_counter]]][job.IsVoidedOnSoftException];
                    } else {
                        _usersInDay.Add(usersInJobs[_counter], job.IsVoidedOnSoftException);
                    }
                    ++_counter;
                }
            }
            return _exceptionCount;
        }

        public JobConsiderationUsersWhoAlreadyHaveJob(Builder builder) : base(builder) {
            BuildSoftExceptionChange();
        }

        private void BuildSoftExceptionChange() {
            // this ensures that regardless if the first job that is added to _usersInDay is or is not a soft exception,
            // if a hard exception is found, it's changed to a hard exception.
            _softExceptionChange.Add(true, new Dictionary<bool, bool>());
            _softExceptionChange.Add(false, new Dictionary<bool, bool>());
            _softExceptionChange[true].Add(true, true);
            _softExceptionChange[true].Add(false, true);
            _softExceptionChange[false].Add(true, true);
            _softExceptionChange[false].Add(false, false);
        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoAlreadyHaveJob(this);
            }
        }
    }

    public class JobConsiderationEvenUserDistributionPerJob : JobConsideration {
        private Dictionary<Job, Dictionary<int, int>> _matrix = new Dictionary<Job, Dictionary<int, int>>();
        private int _counter;
        private int _minMaxTotal;
        private int _minMaxSubTotal;
        private int _min;
        private int _max;

        public override string JobName {
            get { return "Even Distribution Per Job"; }
        }

        public override bool IsConsiderationSoft {
            get { return true; }
        }

        public override float IsValid(int[] usersInJobs) {
            ResetToZeros();
            CountUsersPerJob(usersInJobs);
            return CountMinMax();
        }

        private void ResetToZeros() {
            foreach (Job jobIndex in _matrix.Keys) {
                for (int user = 0; user < this.Users.Count(); user++) {
                    _matrix[jobIndex][user] = 0;
                }
            }
        }

        private void CountUsersPerJob(int[] usersInJobs) {
            _counter = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    _matrix[job][usersInJobs[_counter]] += 1;
                    ++_counter;
                }
            }
        }

        private int CountMinMax() {
            _minMaxTotal = 0;
            foreach (Job jobIndex in _matrix.Keys) {
                _min = int.MaxValue;
                _max = int.MinValue;
                foreach (int user in _matrix[jobIndex].Keys) {
                    if (_matrix[jobIndex][user] < _min) {
                        _min = _matrix[jobIndex][user];
                    }
                    if (_matrix[jobIndex][user] > _max) {
                        _max = _matrix[jobIndex][user];
                    }
                }
                _minMaxSubTotal = _max - _min;
                if (_minMaxSubTotal > 1) {
                    _minMaxTotal += (_minMaxSubTotal - 1);
                }
            }
            return _minMaxTotal;
        }

        public JobConsiderationEvenUserDistributionPerJob(Builder builder) : base(builder) {
            InstantiateMatrix();
        }

        private void InstantiateMatrix() {
            foreach (Job job in this.Jobs) {
                _matrix.Add(job, new Dictionary<int, int>());
                for (int i = 0; i < this.Users.Count(); i++) {
                    _matrix[job].Add(i, 0);
                }
            }
        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationEvenUserDistributionPerJob(this);
            }
        }
    }

    public class JobConsiderationGiveUsersABreak : JobConsideration {
        private BitArray[] _matrix;
        private List<List<OverlapVector>> _overlapReference;
        private int _giveBreakOnDay;
        private int _dayCountPerOverlap;
        private BitArray _bitCount;

        public override string JobName {
            get { return "Give Users A Break"; }
        }

        public override bool IsConsiderationSoft {
            get { return true; }
        }

        public override float IsValid(int[] usersInJobs) {
            SetAllToZero();
            BuildMatrixFromSolution(usersInJobs);
            return PerforANDandCount();
        }

        private void SetAllToZero() {
            _bitCount.SetAll(false);
            for (int matrixIndex = 0; matrixIndex <= _matrix.GetUpperBound(0); matrixIndex++) {
                _matrix[matrixIndex].SetAll(false);
            }
        }

        private void BuildMatrixFromSolution(int[] solution) {
            int counter = 0;
            int day = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    foreach (OverlapVector vector in _overlapReference[day]) {
                        _matrix[vector.Y].Set(this.Users.Count() * vector.X + solution[counter], true);
                    }
                    ++counter;
                }
                ++day;
            }
        }

        private int PerforANDandCount() {
            _bitCount = _matrix[0].And(_matrix[1]);
            for (int day = 2; day < _giveBreakOnDay; day++) {
                _bitCount = _bitCount.And(_matrix[day]);
            }
            return this.CountBitsInBitsArray(_bitCount);
        }

        private string OutputToOneLine(BitArray bitArray) {
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < bitArray.Count; i++) {
                if (bitArray.Get(i) == true) {
                    text.Append("1");
                } else {
                    text.Append("0");
                }
            }
            return text.ToString();
        }

        public JobConsiderationGiveUsersABreak(Builder builder) : base(builder) {
            _giveBreakOnDay = builder.GiveBreakOnDay;
            _bitCount = new BitArray(_dayCountPerOverlap * this.Users.Count());
            InstantiateMatrix();
            BuildOverlapReference();
        }

        private void InstantiateMatrix() {
            _matrix = new BitArray[_giveBreakOnDay];
            _dayCountPerOverlap = this.Templates.Count() - _giveBreakOnDay + 1;
            for (int matrixIndex = 0; matrixIndex <= _matrix.GetUpperBound(0); matrixIndex++) {
                _matrix[matrixIndex] = new BitArray((_dayCountPerOverlap + 1) * this.Users.Count());
            }
        }

        private void BuildOverlapReference() {
            InstantiateOverlapReference();
            for (int day = 0; day <= _dayCountPerOverlap; day++) {
                for (int daysForward = day; daysForward < _giveBreakOnDay + day; daysForward++) {
                    if (daysForward < _overlapReference.Count) {
                        OverlapVector vector = new OverlapVector(day, daysForward - day);
                        _overlapReference[daysForward].Add(vector);
                    }
                }
            }
        }

        private void InstantiateOverlapReference() {
            _overlapReference = new List<List<OverlapVector>>();
            for (int i = 0; i < this.Templates.Count(); i++) {
                _overlapReference.Add(new List<OverlapVector>());
            }
        }

        private class OverlapVector {
            private int _x;
            public int X { get { return _x; } }

            private int _y;
            public int Y { get { return _y; } }

            public OverlapVector(int x, int y) {
                _x = x;
                _y = y;
            }
        }

        public class Builder : JobConsideration.BuilderBase {
            public int GiveBreakOnDay;

            public Builder SetGiveBreakOnDay(int giveBreakOnDay) {
                this.GiveBreakOnDay = giveBreakOnDay;
                return this;
            }

            public override JobConsideration Build() {
                return new JobConsiderationGiveUsersABreak(this);
            }
        }
    }

    public class JobConsiderationUsersWhoDidSameJobLastMeeting : JobConsideration {
        private Dictionary<int, Job> _usersLastJob = new Dictionary<int, Job>();
        private int _counter;

        public override bool IsConsiderationSoft {
            get { return true; }
        }

        public override string JobName {
            get { return "Users Who Did Same Job Last Meeting"; }
        }

        public override float IsValid(int[] usersInJobs) {
            _usersLastJob.Clear();
            _counter = 0;
            int sameJobCount = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    if (_usersLastJob.ContainsKey(usersInJobs[_counter])) {
                        if (_usersLastJob[usersInJobs[_counter]] == job || _usersLastJob[usersInJobs[_counter]].IsSameJob(job)) {
                            ++sameJobCount;
                        }
                        _usersLastJob[usersInJobs[_counter]] = job;
                    } else {
                        _usersLastJob.Add(usersInJobs[_counter], job);
                    }
                    ++_counter;
                }
            }
            return sameJobCount;
        }

        public JobConsiderationUsersWhoDidSameJobLastMeeting(Builder builder) : base(builder) {

        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoDidSameJobLastMeeting(this);
            }
        }
    }
}
