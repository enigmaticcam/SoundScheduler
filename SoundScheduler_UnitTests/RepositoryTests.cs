using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Abstract;
using Moq;

namespace SoundScheduler_UnitTests {

    [TestClass]
    public class RepositoryTests {

        [TestMethod]
        public void Repository_DoesSaveAndReloadCorrectly() {

            // Arrange
            Repository rep1 = new Repository();

            Job job1 = new Job();            
            Job job2 = new Job();
            job1.Name = "Job1";
            job2.Name = "Job2";
            rep1.Jobs = new System.Collections.Generic.List<Job> { job1, job2 };

            User user1 = new User();
            User user2 = new User();
            user1.Name = "User1";
            user2.Name = "User2";
            user1.Jobs = new System.Collections.Generic.List<Job> { job1 };
            user2.Jobs = new System.Collections.Generic.List<Job> { job1, job2 };
            rep1.Users = new System.Collections.Generic.List<User> { user1, user2 };

            Meeting meeting1 = new Meeting();
            meeting1.Date = DateTime.Now;
            meeting1.Jobs = new System.Collections.Generic.List<Job> { job1 };
            meeting1.JobUserSlots = new Meeting.JobUserSlot();
            meeting1.JobUserSlots.AddUserToJob(0, user1);
            Meeting meeting2 = new Meeting();
            meeting2.Date = DateTime.Now.AddYears(1);
            meeting2.Jobs = new System.Collections.Generic.List<Job> { job1, job2 };
            meeting2.JobUserSlots = new Meeting.JobUserSlot();
            meeting2.JobUserSlots.AddUserToJob(0, user1);
            meeting2.JobUserSlots.AddUserToJob(1, user2);
            rep1.Meetings = new System.Collections.Generic.List<Meeting> { meeting1, meeting2 };

            Template template1 = new Template();
            template1.Name = "Template1";
            template1.Jobs = new System.Collections.Generic.List<Job> { job1 };
            Template template2 = new Template();
            template2.Name = "Template2";
            template2.Jobs = new System.Collections.Generic.List<Job> { job1, job2 };
            rep1.Templates = new System.Collections.Generic.List<Template> { template1, template2 };

            // Act
            rep1.SaveToSource();
            Repository rep2 = new Repository();
            rep2.LoadFromSource();


            // Assert
            Assert.AreEqual(rep1.Jobs.Count, rep2.Jobs.Count);
            Assert.AreEqual(rep1.Users.Count, rep2.Users.Count);
            Assert.AreEqual(rep1.Meetings.Count, rep2.Meetings.Count);
            Assert.AreEqual(rep1.Templates.Count, rep2.Templates.Count);
        }
    }
}
