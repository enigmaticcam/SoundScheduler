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
        public abstract JobConsideration ToCopy();
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
            this.JobRank = builder.JobRank;
            _solutionCount = GetSolutionCount();
        }

        public abstract class BuilderBase {
            public abstract JobConsideration Build();
            public IEnumerable<Template> Templates;
            public IEnumerable<User> Users;
            public IEnumerable<Job> Jobs;
            public int JobRank = 1;

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

            public BuilderBase SetJobRank(int jobRank) {
                this.JobRank = jobRank;
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

        public override JobConsideration ToCopy() {
            JobConsiderationUsersWhoCantDoJob consideration = (JobConsiderationUsersWhoCantDoJob)new JobConsiderationUsersWhoCantDoJob.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .Build();
            return consideration;
        }
    }

    public class JobConsiderationUsersWhoAlreadyHaveJob : JobConsideration {
        private Dictionary<Job, Dictionary<Job, float>> _jobComboToPoints = new Dictionary<Job, Dictionary<Job, float>>();
        private Dictionary<ulong, float> _jobComboAsBitToPoints = new Dictionary<ulong, float>();
        private Dictionary<Job, ulong> _jobToBit = new Dictionary<Job, ulong>();
        private Dictionary<int, Dictionary<int, ulong>> _userCombos = new Dictionary<int, Dictionary<int, ulong>>();
        private Dictionary<int, Dictionary<int, Dictionary<int, float>>> _exceptions = new Dictionary<int, Dictionary<int, Dictionary<int, float>>>();
        private Dictionary<Job, float> _jobToException = new Dictionary<Job, float>();
        private HashSet<int> _usersForDay = new HashSet<int>();
        private List<Job> _jobsForCombo = new List<Job>();
        private Dictionary<int, HashSet<int>> _partitions = new Dictionary<int, HashSet<int>>();
        private int _counter;
        private ulong _bit;
        private float _score;
        private float _tempScore;
        private int _day;

        public override string JobName {
            get { return "Users Who Already Have Job"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public void AddException(int templateIndex, int userIndex, float reductionCoefficient, int partition) {
            if (!_exceptions.ContainsKey(partition)) {
                _exceptions.Add(partition, new Dictionary<int, Dictionary<int, float>>());
            }
            _exceptions[partition][templateIndex][userIndex] = reductionCoefficient;
        }

        public void AddException(int templateIndex, int userIndex, float reductionCoefficient) {
            for (int partition = 1; partition <= this.Templates.ElementAt(templateIndex).ParitionCount; partition++) {
                _exceptions[partition][templateIndex][userIndex] = reductionCoefficient;
            }
        }

        public void AddJobToException(Job job, float reduction) {
            _jobToException[job] = reduction;
        }

        public void AddSingleJobCombo(Job job1, Job job2, float reduction) {
            AddJobComboPoints(job1, job2, reduction);
            AddJobComboPoints(job2, job1, reduction);
        }

        public void AddAllJobCombos(Job job1, float reduction) {
            foreach (Job job2 in this.Jobs) {
                if (job1 != job2) {
                    AddJobComboPoints(job1, job2, reduction);
                    AddJobComboPoints(job2, job1, reduction);
                }
            }
        }

        private void AddJobComboPoints(Job job1, Job job2, float reduction) {
            if (!_jobComboToPoints.ContainsKey(job1)) {
                _jobComboToPoints.Add(job1, new Dictionary<Job, float>());
            }
            _jobComboToPoints[job1].Add(job2, reduction);
        }

        public override float IsValid(int[] usersInJobs) {
            _score = 0;
            _counter = 0;
            _day = 0;
            foreach (Template template in this.Templates) {
                _usersForDay.Clear();
                foreach (Job job in template.Jobs) {
                    foreach (int partition in _partitions[_counter]) {
                        _score += AddJobToDayJobCombo(job, usersInJobs[_counter], partition);
                        if (_exceptions[partition][_day][usersInJobs[_counter]] > 0) {
                            _score += Math.Max(_exceptions[partition][_day][usersInJobs[_counter]], _jobToException[job]);
                        }
                        _usersForDay.Add(usersInJobs[_counter]);
                    }
                    
                    ++_counter;
                }
                foreach (int partition in template.ValidPartitions()) {
                    foreach (int user in _usersForDay) {
                        _score += ScoreForDay(template, _userCombos[partition][user]);
                        _userCombos[partition][user] = 0;
                    }
                }
                ++_day;
            }
            return _score;
        }
        
        private float AddJobToDayJobCombo(Job job, int userComboIndex, int partition) {
            _bit = _jobToBit[job];
            if (!_userCombos.ContainsKey(partition)) {
                _userCombos.Add(partition, new Dictionary<int, ulong>());
            }
            if ((_userCombos[partition][userComboIndex] & _bit) == _bit) {
                return 1;
            } else {
                _userCombos[partition][userComboIndex] += _bit;
                return 0;
            }
        }

        private float ScoreForDay(Template template, ulong combo) {
            if (!_jobComboAsBitToPoints.ContainsKey(combo)) {
                _jobsForCombo.Clear();
                foreach (Job job in template.Jobs) {
                    if ((_jobToBit[job] & combo) == _jobToBit[job]) {
                        _jobsForCombo.Add(job);
                    }
                }
                _jobComboAsBitToPoints.Add(combo, ScoreForJobCombo(_jobsForCombo));
            }
            return _jobComboAsBitToPoints[combo];
        }

        private float ScoreForJobCombo(List<Job> jobs) {
            _tempScore = 0;
            int jobIndex = 0;
            foreach (Job job in jobs) {
                for (int jobAgainstIndex = jobIndex + 1; jobAgainstIndex < jobs.Count; jobAgainstIndex++) {
                    if (_jobComboToPoints.ContainsKey(job) && _jobComboToPoints[job].ContainsKey(jobs[jobAgainstIndex])) {
                        _tempScore += _jobComboToPoints[job][jobs[jobAgainstIndex]];
                    } else {
                        _tempScore += 1;
                    }
                }
                ++jobIndex;
            }
            return _tempScore;
        }

        public JobConsiderationUsersWhoAlreadyHaveJob(Builder builder) : base(builder) {
            BuildPartitionsAndUserCombos();
            BuildJobIndex();
        }

        private void BuildJobIndex() {
            for (int jobIndex = 0; jobIndex < this.Jobs.Count(); jobIndex++) {
                ulong bit = (ulong)Math.Pow(2, jobIndex);
                _jobToBit.Add(this.Jobs.ElementAt(jobIndex), bit);
            }
        }

        private void BuildExceptions(int partition) {
            _exceptions.Add(partition, new Dictionary<int, Dictionary<int, float>>());
            for (int templateIndex = 0; templateIndex < this.Templates.Count(); templateIndex++) {
                _exceptions[partition].Add(templateIndex, new Dictionary<int, float>());
                for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                    _exceptions[partition][templateIndex].Add(userIndex, 0);
                }
            }
        }

        private void BuildPartitionsAndUserCombos() {
            HashSet<int> uniquePartitions = new HashSet<int>();
            int counter = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    _partitions.Add(counter, new HashSet<int>());
                    if (template.PartitionsForJob(job).Count == 0) {
                        InstantiateWithPartition(1, uniquePartitions, counter);
                    } else {
                        foreach (int partition in template.PartitionsForJob(job)) {
                            InstantiateWithPartition(partition, uniquePartitions, counter);
                        }
                    }
                    counter++;
                }
            }
            foreach (Job job in this.Jobs) {
                _jobToException.Add(job, 1);
            }
        }

        private void InstantiateWithPartition(int partition, HashSet<int> uniquePartitions, int counter) {
            _partitions[counter].Add(partition);
            if (!uniquePartitions.Contains(partition)) {
                uniquePartitions.Add(partition);
                BuildUserCombos(partition);
                BuildExceptions(partition);
            }
        }

        private void BuildUserCombos(int partition) {
            _userCombos.Add(partition, new Dictionary<int, ulong>());
            for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                _userCombos[partition].Add(userIndex, 0);
            }
        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoAlreadyHaveJob(this);
            }
        }

        public override JobConsideration ToCopy() {
            JobConsiderationUsersWhoAlreadyHaveJob consideration = (JobConsiderationUsersWhoAlreadyHaveJob)new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .Build();
            foreach (int partition in _exceptions.Keys) {
                foreach (int templateIndex in _exceptions[partition].Keys) {
                    foreach (int userIndex in _exceptions[partition][templateIndex].Keys) {
                        consideration.AddException(templateIndex, userIndex, _exceptions[partition][templateIndex][userIndex], partition);
                    }
                }
            }
            
            foreach (Job job in _jobToException.Keys) {
                consideration.AddJobToException(job, _jobToException[job]);
            }
            foreach (Job job1 in _jobComboToPoints.Keys) {
                foreach (Job job2 in _jobComboToPoints[job1].Keys) {
                    consideration.AddJobComboPoints(job1, job2, _jobComboToPoints[job1][job2]);
                }
            }
            return consideration;   
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

        public override JobConsideration ToCopy() {
            JobConsiderationEvenUserDistributionPerJob consideration = (JobConsiderationEvenUserDistributionPerJob)new JobConsiderationEvenUserDistributionPerJob.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .Build();
            return consideration;
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

        public override JobConsideration ToCopy() {
            JobConsiderationGiveUsersABreak consideration = (JobConsiderationGiveUsersABreak)new JobConsiderationGiveUsersABreak.Builder()
                .SetGiveBreakOnDay(_giveBreakOnDay)
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .Build();
            return consideration;
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

        public override JobConsideration ToCopy() {
            JobConsiderationUsersWhoDidSameJobLastMeeting consideration = (JobConsiderationUsersWhoDidSameJobLastMeeting)new JobConsiderationUsersWhoDidSameJobLastMeeting.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .Build();
            return consideration;
        }
    }
}
