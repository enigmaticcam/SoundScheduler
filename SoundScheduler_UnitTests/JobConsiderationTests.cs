using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_UnitTests {
    [TestClass]
    public class JobConsiderationTests {

        [TestMethod]
        public void JobConsideration_UsersWhoCantDoJob() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            users[0].Jobs.Add(jobs[0]);
            users[0].Jobs.Add(jobs[1]);
            users[1].Jobs.Add(jobs[2]);
            users[1].Jobs.Add(jobs[3]);
            users[2].Jobs.Add(jobs[4]);
            users[2].Jobs.Add(jobs[0]);
            users[3].Jobs.Add(jobs[1]);
            users[3].Jobs.Add(jobs[2]);
            users[4].Jobs.Add(jobs[4]);
            users[4].Jobs.Add(jobs[0]);

            Template template1 = new Template();
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            template1.Jobs.Add(jobs[2]);
            template1.Jobs.Add(jobs[3]);
            template1.Jobs.Add(jobs[4]);
            List<Template> templates = new List<Template> { template1 };

            int[] solution1 = new int[jobs.Count];
            int[] solution2 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);
            solution1[jobs.IndexOf(jobs[3])] = users.IndexOf(users[1]);
            solution1[jobs.IndexOf(jobs[4])] = users.IndexOf(users[3]);

            solution2[jobs.IndexOf(jobs[0])] = users.IndexOf(users[4]);
            solution2[jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);
            solution2[jobs.IndexOf(jobs[2])] = users.IndexOf(users[1]);
            solution2[jobs.IndexOf(jobs[3])] = users.IndexOf(users[1]);
            solution2[jobs.IndexOf(jobs[4])] = users.IndexOf(users[2]);

            // Act
            JobConsideration consideration = new JobConsiderationUsersWhoCantDoJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            bool isValid1 = consideration.IsValid(solution1);
            bool isValid2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(false, isValid1);
            Assert.AreEqual(true, isValid2);
        }
    }
}
