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
        public abstract bool IsValid(int[] usersInJobs);

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

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public override bool IsValid(int[] usersInJobs) {
            BitArray solution = new BitArray(this.SolutionCount * this.Users.Count());
            for (int i = 0; i <= usersInJobs.GetUpperBound(0); i++) {
                int position = usersInJobs[i] * this.SolutionCount + i;
                solution.Set(position, true);
            }
            BitArray cantDoJobs = solution.And(_matrix);
            int bitCount = CountBitsInBitsArray(cantDoJobs);
            if (bitCount > 1) {
                return false;
            } else {
                return true;
            }
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
        private BitArray _matrix;

        public override bool IsConsiderationSoft {
            get { return false; }
        }

        public override bool IsValid(int[] usersInJobs) {
            int counter = 0;
            HashSet<int> users = new HashSet<int>();
            foreach (Template template in this.Templates) {
                users.Clear();
                foreach (Job job in template.Jobs) {
                    if (users.Contains(usersInJobs[counter])) {
                        return false;
                    }
                    ++counter;
                }
            }
            return true;
        }

        public JobConsiderationUsersWhoAlreadyHaveJob(Builder builder) : base(builder) {
            BuildMatrixOfUsersWhoAlreadyHaveJob();
        }

        private void BuildMatrixOfUsersWhoAlreadyHaveJob() {
            _matrix = new BitArray(this.SolutionCount * this.Users.Count());
        }

        

        public class Builder : JobConsideration.BuilderBase {
            public override JobConsideration Build() {
                return new JobConsiderationUsersWhoAlreadyHaveJob(this);
            }
        }
    }
}
