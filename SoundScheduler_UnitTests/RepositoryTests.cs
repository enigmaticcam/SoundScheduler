using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Abstract;
using Moq;

namespace SoundScheduler_UnitTests {

    [TestClass]
    public class RepositoryTests {

        private string RepositorySaveFile {
            get { return "UnitTestSaveFile.xml"; }
        }

        [TestMethod]
        public void Repository_DoesSaveJobs() {

            // Arrange
            Job job1 = new Job();
            job1.Name = "Job1";
            job1.IsVoidedOnSoftException = true;

            Job job2 = new Job();
            job2.Name = "Job2";
            job2.IsVoidedOnSoftException = false;

            Job job3 = new Job();
            job3.Name = "Job3";
            job3.IsVoidedOnSoftException = true;
            job3.AddSameJob(job2);

            // Act
            Repository sourceRepository = new Repository(this.RepositorySaveFile);
            sourceRepository.Jobs.Add(job1);
            sourceRepository.Jobs.Add(job2);
            sourceRepository.Jobs.Add(job3);
            sourceRepository.SaveToSource();

            Repository targetRepository = new Repository(this.RepositorySaveFile);
            targetRepository.LoadFromSource();

            // Assert
            Assert.AreEqual(sourceRepository.Jobs.Count, targetRepository.Jobs.Count);
            Assert.AreEqual(sourceRepository.Jobs[0].Name, targetRepository.Jobs[0].Name);
            Assert.AreEqual(sourceRepository.Jobs[1].Name, targetRepository.Jobs[1].Name);
            Assert.AreEqual(sourceRepository.Jobs[2].Name, targetRepository.Jobs[2].Name);
            Assert.AreEqual(sourceRepository.Jobs[0].IsVoidedOnSoftException, targetRepository.Jobs[0].IsVoidedOnSoftException);
            Assert.AreEqual(sourceRepository.Jobs[1].IsVoidedOnSoftException, targetRepository.Jobs[1].IsVoidedOnSoftException);
            Assert.AreEqual(sourceRepository.Jobs[2].IsVoidedOnSoftException, targetRepository.Jobs[2].IsVoidedOnSoftException);
            Assert.AreEqual(sourceRepository.Jobs[0].SameJobs.Count(), targetRepository.Jobs[0].SameJobs.Count());
            Assert.AreEqual(sourceRepository.Jobs[1].SameJobs.Count(), targetRepository.Jobs[1].SameJobs.Count());
            Assert.AreEqual(sourceRepository.Jobs[2].SameJobs.Count(), targetRepository.Jobs[2].SameJobs.Count());
            Assert.AreEqual("Job2", targetRepository.Jobs[2].SameJobs.ElementAt(0).Name);
        }

        [TestMethod]
        public void Repository_DoesSaveUsers() {

            // Arrange
            Job job1 = new Job();
            job1.Name = "Job1";

            Job job2 = new Job();
            job2.Name = "Job2";

            User user1 = new User();
            user1.Name = "1";
            user1.Jobs.Add(job1);
            user1.Jobs.Add(job2);

            User user2 = new User();
            user2.Name = "User2";
            user2.Jobs.Add(job2);

            // Act
            Repository sourceRepository = new Repository(this.RepositorySaveFile);
            sourceRepository.Jobs.Add(job1);
            sourceRepository.Jobs.Add(job2);
            sourceRepository.Users.Add(user1);
            sourceRepository.Users.Add(user2);
            sourceRepository.SaveToSource();

            Repository targetRepository = new Repository(this.RepositorySaveFile);
            targetRepository.LoadFromSource();

            // Assert
            Assert.AreEqual(sourceRepository.Users.Count, targetRepository.Users.Count);

        }

    }
}
