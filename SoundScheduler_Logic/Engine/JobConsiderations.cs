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
        private Dictionary<int, Dictionary<int, UserExceptionType>> _exceptionAvailability = new Dictionary<int, Dictionary<int, UserExceptionType>>();
        private Dictionary<int, int> _dayToSubstituteJob = new Dictionary<int, int>();
        private int _day = 0;
        private int _counter = 0;
        private float _score;

        public void AddNeedForAvailability(int userIndex, int partition, UserExceptionType exception) {
            if (!_exceptionAvailability.ContainsKey(userIndex)) {
                _exceptionAvailability.Add(userIndex, new Dictionary<int, UserExceptionType>());
            }
            _exceptionAvailability[userIndex].Add(partition, exception);
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
                if (_dayToSubstituteJob.ContainsKey(_counter)) {
                    foreach (Job job in template.Jobs) {
                        foreach (int partition in template.PartitionsForJob(job)) {
                            if (DoesUserHaveRequirementForSubstitute(usersInJobs[_counter], partition) && IsRequirementApplicableForJob(usersInJobs[_counter], partition, job)) {
                                _score += IsUserInSubstituteJobAvailable(usersInJobs[_dayToSubstituteJob[_day]], partition, job);
                            }
                        }
                        _counter++;
                    }
                }
                _day++;
            }
            return _score;
        }

        private bool DoesUserHaveRequirementForSubstitute(int userIndex, int partition) {
            if (_exceptionAvailability.ContainsKey(userIndex) && _exceptionAvailability[userIndex].ContainsKey(partition)) {
                return true;
            } else {
                return false;
            }
        }

        private bool IsRequirementApplicableForJob(int userIndex, int partition, Job job) {
            return _exceptionAvailability[userIndex][partition].GetSubRequiresAvailability(job);
        }

        private float IsUserInSubstituteJobAvailable(int userIndex, int partition, Job job) {
            if (this.UserExceptions.HasUserException(partition, userIndex)) {
                return this.UserExceptions.GetUserException(partition, userIndex).GetJobExceptionValue(job);
            } else {
                return 0;
            }
        }

        public override JobConsideration ToCopy() {
            throw new NotImplementedException();
        }


        public JobConsiderationSubstituteJobAvailability(Builder builder) : base(builder) {
            _substituteJob = builder.SubstituteJob;
            BuildDayToSubstituteJobReference();
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

        public void AddAllJobCombos(Job job1, float reduction) {
            foreach (Job job2 in this.Jobs) {
                if (job1 != job2) {
                    AddJobComboPoints(job1, job2, reduction);
                    AddJobComboPoints(job2, job1, reduction);
                }
            }
        }

        public void AddJobComboPoints(Job job1, Job job2, float reduction) {
            if (!_jobComboToPoints.ContainsKey(job1)) {
                _jobComboToPoints.Add(job1, new Dictionary<Job, float>());
            }
            _jobComboToPoints[job1].Add(job2, reduction);
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
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    foreach (int partition in template.PartitionsForJob(job)) {
                        if (this.UserExceptions.HasUserException(partition, usersInJobs[_counter])) {
                            _score += this.UserExceptions.GetUserException(partition, usersInJobs[_counter]).GetJobExceptionValue(job);
                        }
                    }
                    _counter++;
                }
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
            JobConsiderationUsersWhoAlreadyHaveJobOld consideration = (JobConsiderationUsersWhoAlreadyHaveJobOld)new JobConsiderationUsersWhoAlreadyHaveJobOld.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .SetUserExceptions(this.UserExceptions.ToCopy())
                .Build();
            return consideration;
        }
    }

    public class JobConsiderationUsersWhoAlreadyHaveJobOld : JobConsideration {
        private Dictionary<Job, Dictionary<Job, float>> _jobComboToPoints = new Dictionary<Job, Dictionary<Job, float>>();
        private Dictionary<ulong, float> _jobComboAsBitToPoints = new Dictionary<ulong, float>();
        private Dictionary<Job, ulong> _jobToBit = new Dictionary<Job, ulong>();
        private Dictionary<int, ulong> _userCombos = new Dictionary<int, ulong>();
        private Dictionary<int, Dictionary<int, Dictionary<int, float>>> _exceptions = new Dictionary<int, Dictionary<int, Dictionary<int, float>>>();
        private Dictionary<Job, float> _jobToException = new Dictionary<Job, float>();
        private HashSet<int> _usersForDay = new HashSet<int>();
        private List<Job> _jobsForCombo = new List<Job>();
        private Dictionary<int, HashSet<int>> _partitions = new Dictionary<int, HashSet<int>>();
        private Dictionary<Job, Job> _jobToSubjob = new Dictionary<Job, Job>();
        private Dictionary<int, int> _subjobNeedInPartition = new Dictionary<int, int>();
        private Dictionary<Template, Dictionary<Job, int>> _subJobIndexOfJob = new Dictionary<Template, Dictionary<Job, int>>();
        private int _counter;
        private ulong _bit;
        private float _score;
        private float _tempScore;
        private int _day;
        private int _startOfDay;

        public override string JobName {
            get { return "Users Who Already Have Job/Exception"; }
        }

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public void AddSubjobToJob(Job mainJob, Job subJob) {
            _jobToSubjob.Add(mainJob, subJob);
        }

        public void AddNeedForSubjob(int userIndex, int partition) {
            _subjobNeedInPartition.Add(userIndex, partition);
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
                _startOfDay = _counter;
                _usersForDay.Clear();
                foreach (Job job in template.Jobs) {
                    _score += AddJobToDayJobCombo(job, usersInJobs[_counter]);
                    foreach (int partition in _partitions[_counter]) {
                        _score += IsSubJobNeededAndFulfilled(job, template, usersInJobs[_counter], partition, usersInJobs);
                        if (_exceptions[partition][_day][usersInJobs[_counter]] > 0) {
                            _score += Math.Max(_exceptions[partition][_day][usersInJobs[_counter]], _jobToException[job]);
                        }
                        _usersForDay.Add(usersInJobs[_counter]);
                    }
                    
                    ++_counter;
                }
                foreach (int user in _usersForDay) {
                    _score += ScoreForDay(template, _userCombos[user]);
                    _userCombos[user] = 0;
                }
                ++_day;
            }
            return _score;
        }

        private float IsSubJobNeededAndFulfilled(Job job, Template template, int userIndex, int partition, int[] usersInJobs) {
            if (IsJobInPartition(job, template, partition) && DoesJobHaveSubJob(job) && DoesUserHaveNeedForSubJobInPartition(userIndex, partition)) {
                return IsUserInSubjobAvailableForPartition(template, job, partition, usersInJobs);
            } else {
                return 0;
            }
        }

        private bool IsJobInPartition(Job job, Template template, int partition) {
            return template.PartitionsForJob(job).Contains(partition);
        }

        private bool DoesJobHaveSubJob(Job job) {
            return _jobToSubjob.ContainsKey(job);
        }
        
        private bool DoesUserHaveNeedForSubJobInPartition(int userIndex, int partition) {
            if (_subjobNeedInPartition.ContainsKey(userIndex) && _subjobNeedInPartition[userIndex] == partition) {
                return true;
            } else {
                return false;
            }
        }

        private float IsUserInSubjobAvailableForPartition(Template template, Job job, int partition, int[] usersInJobs) {
            int userInSubjob = usersInJobs[_subJobIndexOfJob[template][job]];
            if (!_exceptions.ContainsKey(partition)) {
                return 0;
            } else if (!_exceptions[partition].ContainsKey(_day)) {
                return 0;
            } else if (!_exceptions[partition][_day].ContainsKey(userInSubjob)) {
                return 0;
            } else {
                return _exceptions[partition][_day][userInSubjob];
            }
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

        public JobConsiderationUsersWhoAlreadyHaveJobOld(Builder builder) : base(builder) {
            BuildSubJobIndexOfJob();
            BuildPartitionsAndUserCombos();
            BuildJobIndex();
        }

        private void BuildSubJobIndexOfJob() {
            int startOfDay = 0;
            foreach (Template template in this.Templates) {
                _subJobIndexOfJob.Add(template, new Dictionary<Job, int>());
                foreach (Job job in template.Jobs) {
                    if (_jobToSubjob.ContainsKey(job)) {
                        _subJobIndexOfJob[template].Add(job, template.Jobs.IndexOf(_jobToSubjob[job]) + startOfDay);
                    }
                }
                startOfDay += template.Jobs.Count;
            }
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
            BuildUserCombos();
            HashSet<int> uniquePartitions = new HashSet<int>();
            int counter = 0;
            foreach (Template template in this.Templates) {
                foreach (Job job in template.Jobs) {
                    _partitions.Add(counter, new HashSet<int>());
                    foreach (int partition in template.PartitionsForJob(job)) {
                        InstantiateWithPartition(partition, uniquePartitions, counter);
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
                BuildExceptions(partition);
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
                return new JobConsiderationUsersWhoAlreadyHaveJobOld(this);
            }
        }

        public override JobConsideration ToCopy() {
            JobConsiderationUsersWhoAlreadyHaveJobOld consideration = (JobConsiderationUsersWhoAlreadyHaveJobOld)new JobConsiderationUsersWhoAlreadyHaveJobOld.Builder()
                .SetJobs(this.Jobs)
                .SetTemplates(this.Templates)
                .SetUsers(this.Users)
                .SetJobRank(this.JobRank)
                .SetUserExceptions(this.UserExceptions)
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
                            if (!_hasOnlyOneJob[usersInJobs[_counter]]) {
                                ++sameJobCount;
                            }
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
            UserExceptionType copy = new UserExceptionType();
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
        private Dictionary<int, Dictionary<int, UserExceptionType>> _userExceptions = new Dictionary<int,Dictionary<int,UserExceptionType>>();

        public void AddUserException(UserExceptionType exception, int partition, int userIndex) {
            if (!_userExceptions.ContainsKey(partition)) {
                _userExceptions.Add(partition, new Dictionary<int,UserExceptionType>());
            }
            _userExceptions[partition].Add(userIndex, exception);
        }

        public bool HasUserException(int partition, int userIndex) {
            if (_userExceptions.ContainsKey(partition)) {
                return _userExceptions[partition].ContainsKey(userIndex);
            } else {
                return false;
            }
        }

        public UserExceptionType GetUserException(int partition, int userIndex) {
            return _userExceptions[partition][userIndex];
        }

        public UserExceptionDictionary ToCopy() {
            UserExceptionDictionary copy = new UserExceptionDictionary();
            foreach (int partition in _userExceptions.Keys) {
                foreach (int userIndex in _userExceptions[partition].Keys) {
                    copy.AddUserException(_userExceptions[partition][userIndex], partition, userIndex);
                }
            }
            return copy;
        }
    }
}
