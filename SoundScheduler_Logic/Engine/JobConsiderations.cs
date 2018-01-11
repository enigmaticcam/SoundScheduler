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

        private UserExceptionDictionary _userExceptions;
        public UserExceptionDictionary UserExceptions {
            get { return _userExceptions; }
        }

        private int _solutionCount;
        public int SolutionCount {
            get { return _solutionCount; }
        }

        public virtual void Begin() {

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
            _userExceptions = builder.UserExceptions;
            this.JobRank = builder.JobRank;
            _solutionCount = GetSolutionCount();
        }

        public abstract class BuilderBase {
            public abstract JobConsideration Build();
            public IEnumerable<Template> Templates;
            public IEnumerable<User> Users;
            public IEnumerable<Job> Jobs;
            public UserExceptionDictionary UserExceptions;
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

            public BuilderBase SetUserExceptions(UserExceptionDictionary userExceptions) {
                this.UserExceptions = userExceptions;
                return this;
            }
        }
    }

    public class JobConsiderationVariety : JobConsideration {
        private HashSet<string> _keys = new HashSet<string>();
        private int _maxComboCount;

        public JobConsiderationVariety(Builder builder) : base(builder) {
            HashSet<int> uniqueJobs = new HashSet<int>();
            HashSet<int> uniqueTemplates = new HashSet<int>();
            foreach (Job job in this.Jobs) {
                uniqueJobs.Add(job.UniqueId);
            }
            foreach (Template template in this.Templates) {
                uniqueTemplates.Add(template.UniqueId);
            }
            _maxComboCount = this.Users.Count() * uniqueJobs.Count * uniqueTemplates.Count;
        }

        public override string JobName {
            get { return "Variety"; }
        }

        public override bool IsConsiderationSoft {
            get { return true; }
        }

        public override float IsValid(int[] usersInJobs) {
            _keys.Clear();
            int index = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    _keys.Add(template.UniqueId + ":" + job.UniqueId + ":" + usersInJobs[index].ToString());
                    index++;
                }
            }
            return (_maxComboCount - _keys.Count) / 10;
        }

        public class Builder : JobConsideration.BuilderBase {
            public Job SubstituteJob;

            public Builder SetSubstituteJob(Job substituteJob) {
                this.SubstituteJob = substituteJob;
                return this;
            }

            public override JobConsideration Build() {
                return new JobConsiderationVariety(this);
            }
        }

        public override JobConsideration ToCopy() {
            JobConsiderationVariety consideration = (JobConsiderationVariety)new JobConsiderationVariety.Builder()
                .SetJobRank(this.JobRank)
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUserExceptions(this.UserExceptions)
                .SetUsers(this.Users)
                .Build();
            return consideration;
        }
    }

    public class JobConsiderationLimitsPerPeriod : JobConsideration {
        private HashSet<int> _jobsToIgnore = new HashSet<int>();
        private int[] _actuals;
        private List<int> _jobsToNotIgnore = new List<int>();
        
        private int[] _limits;
        public void AddLimit(int userIndex, int maxCount) {
            _limits[userIndex] = maxCount;
        }

        private HashSet<Job> _ignoreLimitCountOnJob = new HashSet<Job>();
        public void AddIgnoreLimitCountOnJob(Job job) {
            _ignoreLimitCountOnJob.Add(job);
        }

        public override string JobName {
            get { return "Avoid Using Elders"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }
        private Dictionary<int, Job> _someJobs = new Dictionary<int, Job>();

        public override float IsValid(int[] usersInJobs) {
            float result = 0;
            Array.Clear(_actuals, 0, _actuals.Length);
            foreach (int counter in _jobsToNotIgnore) {
                _actuals[usersInJobs[counter]] += 1;
                if (_actuals[usersInJobs[counter]] > _limits[usersInJobs[counter]]) {
                    result += (float)0.25;
                }
            }
            return result;
        }

        public override void Begin() {
            int counter = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    if (!_ignoreLimitCountOnJob.Contains(job)) {
                        _jobsToNotIgnore.Add(counter);
                        _someJobs.Add(counter, job);
                    }
                    counter++;
                }
            }
        }

        public JobConsiderationLimitsPerPeriod(BuilderBase builder) : base(builder) {
            _limits = new int[this.Users.Count()];
            _actuals = new int[this.Users.Count()];
            for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                _limits[userIndex] = 999;
            }
        }

        public class Builder : JobConsideration.BuilderBase {
            public Job SubstituteJob;

            public Builder SetSubstituteJob(Job substituteJob) {
                this.SubstituteJob = substituteJob;
                return this;
            }

            public override JobConsideration Build() {
                return new JobConsiderationLimitsPerPeriod(this);
            }
        }

        public override JobConsideration ToCopy() {
            JobConsiderationLimitsPerPeriod consideration = (JobConsiderationLimitsPerPeriod)new JobConsiderationLimitsPerPeriod.Builder()
                .SetJobRank(this.JobRank)
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUserExceptions(this.UserExceptions)
                .SetUsers(this.Users)
                .Build();
            for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                if (_limits[userIndex] != 999) {
                    consideration.AddLimit(userIndex, _limits[userIndex]);
                }
            }
            foreach (Job job in _ignoreLimitCountOnJob) {
                consideration.AddIgnoreLimitCountOnJob(job);
            }
            return consideration;
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
            public Job SubstituteJob;

            public Builder SetSubstituteJob(Job substituteJob) {
                this.SubstituteJob = substituteJob;
                return this;
            }

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
                .SetUserExceptions(this.UserExceptions)
                .Build();
            return consideration;
        }
    }

    public class JobConsiderationSubstituteJobAvailability : JobConsideration {
        private Job _substituteJob;
        private Dictionary<int, Dictionary<int, Dictionary<int, UserExceptionType>>> _exceptionAvailability = new Dictionary<int, Dictionary<int, Dictionary<int, UserExceptionType>>>();
        private Dictionary<int, int> _dayToSubstituteJob = new Dictionary<int, int>();
        private HashSet<Job> _jobsCantBeSubstituted = new HashSet<Job>();
        private int _day = 0;
        private int _counter = 0;
        private float _score;

        public void AddNeedForAvailability(int templateIndex, int userIndex, int partition, UserExceptionType exception) {
            if (!_exceptionAvailability.ContainsKey(templateIndex)) {
                _exceptionAvailability.Add(templateIndex, new Dictionary<int, Dictionary<int, UserExceptionType>>());
            }
            if (!_exceptionAvailability[templateIndex].ContainsKey(userIndex)) {
                _exceptionAvailability[templateIndex].Add(userIndex, new Dictionary<int, UserExceptionType>());
            }
            _exceptionAvailability[templateIndex][userIndex].Add(partition, exception);
        }

        public override string JobName {
            get { return "Substitute Job Availability"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public override float IsValid(int[] usersInJobs) {
            _day = 0;
            _counter = 0;
            _score = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    if (_dayToSubstituteJob.ContainsKey(_day)) {
                        foreach (int partition in template.PartitionsForJob(job)) {
                            if (DoesUserHaveRequirementForSubstitute(_day, usersInJobs[_counter], partition) && IsRequirementApplicableForJob(_day, usersInJobs[_counter], partition, job)) {
                                _score += (float)0.5;
                                _score += IsUserInSubstituteJobAvailable(_day, usersInJobs[_dayToSubstituteJob[_day]], partition, job);
                                if (!CanJobBeSubstituted(job)) {
                                    _score += 1;
                                }
                            }
                        }
                    }
                    _counter++;
                }
                _day++;
            }
            return _score;
        }

        private bool DoesUserHaveRequirementForSubstitute(int templateIndex, int userIndex, int partition) {
            if (_exceptionAvailability.ContainsKey(templateIndex) && _exceptionAvailability[templateIndex].ContainsKey(userIndex)) {
                return _exceptionAvailability[templateIndex][userIndex].ContainsKey(partition);
            } else {
                return false;
            }
        }

        private bool IsRequirementApplicableForJob(int templateIndex, int userIndex, int partition, Job job) {
            return _exceptionAvailability[templateIndex][userIndex][partition].GetSubRequiresAvailability(job);
        }

        private float IsUserInSubstituteJobAvailable(int templateIndex, int userIndex, int partition, Job job) {
            if (this.UserExceptions.HasUserException(partition, userIndex, templateIndex)) {
                return this.UserExceptions.GetUserException(partition, userIndex, templateIndex).GetJobExceptionValue(job);
            } else {
                return 0;
            }
        }

        private bool CanJobBeSubstituted(Job job) {
            if (_jobsCantBeSubstituted.Contains(job)) {
                return false;
            } else {
                return true;
            }
        }

        public override JobConsideration ToCopy() {
            JobConsiderationSubstituteJobAvailability consideration = (JobConsiderationSubstituteJobAvailability)new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(_substituteJob)
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .SetUserExceptions(this.UserExceptions)
                .Build();
            foreach (int templateIndex in _exceptionAvailability.Keys) {
                foreach (int userIndex in _exceptionAvailability[templateIndex].Keys) {
                    foreach (int partition in _exceptionAvailability[templateIndex][userIndex].Keys) {
                        consideration.AddNeedForAvailability(templateIndex, userIndex, partition, _exceptionAvailability[templateIndex][userIndex][partition]);
                    }
                }
            }
            return consideration; 
        }


        public JobConsiderationSubstituteJobAvailability(Builder builder) : base(builder) {
            _substituteJob = builder.SubstituteJob;
            BuildDayToSubstituteJobReference();
            BuildJobsCantBeSubstituted();
        }

        private void BuildDayToSubstituteJobReference() {
            int day = 0;
            int counter = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    if (_substituteJob == job) {
                        _dayToSubstituteJob.Add(day, counter);
                    }
                    counter++;
                }
                day++;
            }
        }

        private void BuildJobsCantBeSubstituted() {
            foreach (Job job in this.Jobs) {
                if (!job.CanBeSubstituded) {
                    _jobsCantBeSubstituted.Add(job);
                }
            }
        }
        

        public class Builder : JobConsideration.BuilderBase {
            public Job SubstituteJob;

            public Builder SetSubstituteJob(Job substituteJob) {
                this.SubstituteJob = substituteJob;
                return this;
            }

            public override JobConsideration Build() {
                return new JobConsiderationSubstituteJobAvailability(this);
            }
        }
    }

    public class JobConsiderationUsersWhoAlreadyHaveJob : JobConsideration {
        private Dictionary<Job, Dictionary<Job, float>> _jobComboToPoints = new Dictionary<Job, Dictionary<Job, float>>();
        private Dictionary<Job, ulong> _jobToBit = new Dictionary<Job, ulong>();
        private Dictionary<int, ulong> _userCombos = new Dictionary<int, ulong>();
        private Dictionary<ulong, float> _jobComboAsBitToPoints = new Dictionary<ulong, float>();
        private HashSet<int> _usersForDay = new HashSet<int>();
        private List<Job> _jobsForCombo = new List<Job>();
        private ulong _bit;
        private float _tempScore;
        private float _score;
        private int _counter;

        public override string JobName {
            get { return "Users Who Already Have Job"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
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
            if (!_jobComboToPoints[job1].ContainsKey(job2)) {
                _jobComboToPoints[job1].Add(job2, reduction);
            } else {
                _jobComboToPoints[job1][job2] = reduction;
            }
        }

        public override float IsValid(int[] usersInJobs) {
            _score = 0;
            _counter = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    _score += AddJobToDayJobCombo(job, usersInJobs[_counter]);
                    _usersForDay.Add(usersInJobs[_counter]);
                    _counter++;
                }
                foreach (int user in _usersForDay) {
                    _score += ScoreForDay(template, _userCombos[user]);
                    _userCombos[user] = 0;
                }
            }
            return _score;
        }

        private float AddJobToDayJobCombo(Job job, int userComboIndex) {
            _bit = _jobToBit[job];
            if ((_userCombos[userComboIndex] & _bit) == _bit) {
                return 1;
            } else {
                _userCombos[userComboIndex] += _bit;
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

        public override JobConsideration ToCopy() {
            JobConsiderationUsersWhoAlreadyHaveJob consideration = (JobConsiderationUsersWhoAlreadyHaveJob)new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .SetUserExceptions(this.UserExceptions)
                .Build();
            foreach (Job job1 in _jobComboToPoints.Keys) {
                foreach (Job job2 in _jobComboToPoints[job1].Keys) {
                    consideration.AddJobComboPoints(job1, job2, _jobComboToPoints[job1][job2]);
                }
            }
            return consideration; 
        }

        public JobConsiderationUsersWhoAlreadyHaveJob(Builder builder): base(builder) {
            BuildJobIndex();
            BuildUserCombos();
        }

        private void BuildJobIndex() {
            for (int jobIndex = 0; jobIndex < this.Jobs.Count(); jobIndex++) {
                ulong bit = (ulong)Math.Pow(2, jobIndex);
                _jobToBit.Add(this.Jobs.ElementAt(jobIndex), bit);
            }
        }

        private void BuildUserCombos() {
            _userCombos = new Dictionary<int, ulong>();
            for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                _userCombos.Add(userIndex, 0);
            }
        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoAlreadyHaveJob(this);
            }
        }
    }

    public class JobConsiderationUsersWhoHaveExceptions : JobConsideration {
        private int _counter;
        private int _day;
        private float _score;

        public override string JobName {
            get { return "Users Who Have Exceptions"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public override float IsValid(int[] usersInJobs) {
            _score = 0;
            _counter = 0;
            _day = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    foreach (int partition in template.PartitionsForJob(job)) {
                        if (this.UserExceptions.HasUserException(partition, usersInJobs[_counter], _day)) {
                            _score += this.UserExceptions.GetUserException(partition, usersInJobs[_counter], _day).GetJobExceptionValue(job);
                        }
                    }
                    _counter++;
                }
                _day++;
            }
            return _score;
        }

        public JobConsiderationUsersWhoHaveExceptions(Builder builder) : base(builder) {

        }

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoHaveExceptions(this);
            }
        }

        public override JobConsideration ToCopy() {
            JobConsiderationUsersWhoHaveExceptions consideration = (JobConsiderationUsersWhoHaveExceptions)new JobConsiderationUsersWhoHaveExceptions.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .SetUserExceptions(this.UserExceptions.ToCopy())
                .Build();
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
                .SetUserExceptions(this.UserExceptions)
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
                .SetUserExceptions(this.UserExceptions)
                .Build();
            return consideration;
        }
    }

    public class JobConsiderationUsersWhoDidSameJobLastMeeting : JobConsideration {
        private Dictionary<int, Job> _usersLastJob = new Dictionary<int, Job>();
        private Dictionary<int, bool> _hasOnlyOneJob = new Dictionary<int, bool>();
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
                            //if (!_hasOnlyOneJob[usersInJobs[_counter]]) {
                            ++sameJobCount;
                            //}
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
            BuildUniqueJobsPerUser();
        }

        private void BuildUniqueJobsPerUser() {
            for (int userIndex = 0; userIndex < this.Users.Count(); userIndex++) {
                User user = this.Users.ElementAt(userIndex);
                bool hasOnlyOneJob = true;
                for (int job = 0; job < user.Jobs.Count; job++) {
                    for (int compare = job + 1; compare < user.Jobs.Count; compare++) {
                        if (!user.Jobs.ElementAt(job).IsSameJob(user.Jobs.ElementAt(compare))) {
                            hasOnlyOneJob = false;
                        }
                    }
                }
                _hasOnlyOneJob.Add(userIndex, hasOnlyOneJob);
            }
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
                .SetUserExceptions(this.UserExceptions)
                .Build();
            return consideration;
        }
    }

    public class UserExceptionType {
        private Dictionary<Job, float> _jobExceptionValue = new Dictionary<Job, float>();
        private Dictionary<Job, bool> _subRequiresAvailability = new Dictionary<Job, bool>();

        private string _name;
        public string Name {
            get { return _name; }
        }

        public UserExceptionType(string name) {
            _name = name;
        }

        public void AddJobExceptionValue(Job job, float value) {
            _jobExceptionValue.Add(job, value);
        }

        public float GetJobExceptionValue(Job job) {
            if (_jobExceptionValue.ContainsKey(job)) {
                return _jobExceptionValue[job];
            } else {
                return 0;
            }
        }

        public void AddSubRequiresAvailability(Job job, bool requires) {
            _subRequiresAvailability.Add(job, requires);
        }

        public bool GetSubRequiresAvailability(Job job) {
            if (_subRequiresAvailability.ContainsKey(job)) {
                return _subRequiresAvailability[job];
            } else {
                return false;
            }
        }

        public UserExceptionType ToCopy() {
            UserExceptionType copy = new UserExceptionType(_name);
            foreach (Job job in _jobExceptionValue.Keys) {
                copy.AddJobExceptionValue(job, _jobExceptionValue[job]);
            }
            foreach (Job job in _subRequiresAvailability.Keys) {
                copy.AddSubRequiresAvailability(job, _subRequiresAvailability[job]);
            }
            return copy;
        }
    }

    public class UserExceptionDictionary {
        private Dictionary<int, Dictionary<int, Dictionary<int, UserExceptionType>>> _userExceptions = new Dictionary<int, Dictionary<int, Dictionary<int, UserExceptionType>>>();

        public void AddUserException(UserExceptionType exception, int userIndex, int templateIndex, int partition) {
            if (!_userExceptions.ContainsKey(userIndex)) {
                _userExceptions.Add(userIndex, new Dictionary<int, Dictionary<int, UserExceptionType>>());
            }
            if (!_userExceptions[userIndex].ContainsKey(templateIndex)) {
                _userExceptions[userIndex].Add(templateIndex, new Dictionary<int, UserExceptionType>());
            }
            _userExceptions[userIndex][templateIndex].Add(partition, exception);
        }

        public void AddUserExceptionToAllPartitions(UserExceptionType exception, int userIndex, int templateIndex, Template template) {
            for (int partition = 1; partition <= template.ParitionCount; partition++) {
                this.AddUserException(exception, userIndex, templateIndex, partition);
            }
        }

        public bool HasUserException(int partition, int userIndex, int templateIndex) {
            if (_userExceptions.ContainsKey(userIndex) && _userExceptions[userIndex].ContainsKey(templateIndex)) {
                return _userExceptions[userIndex][templateIndex].ContainsKey(partition);
            } else {
                return false;
            }
        }

        public UserExceptionType GetUserException(int partition, int userIndex, int templateIndex) {
            return _userExceptions[userIndex][templateIndex][partition];
        }

        public UserExceptionDictionary ToCopy() {
            UserExceptionDictionary copy = new UserExceptionDictionary();
            foreach (int userIndex in _userExceptions.Keys) {
                foreach (int templateIndex in _userExceptions[userIndex].Keys) {
                    foreach (int partition in _userExceptions[userIndex][templateIndex].Keys) {
                        copy.AddUserException(_userExceptions[userIndex][templateIndex][partition].ToCopy(), userIndex, templateIndex, partition);
                    }
                }
            }
            return copy;
        }
    }
}
