using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_UnitTests {
    [TestClass]
    public class JobConsiderationTests {

        public void JobConsideration_DoesEvaluate() {

            // Arrange
            List<Job> jobs = new List<Job>();
            List<User> users = new List<User>();
            List<Meeting> meetings = new List<Meeting>();
            List<JobConsideration> jobConsiderations = new List<JobConsideration>();
            List<Template> templates = new List<Template>();

            Job jobSound = new Job();
            jobSound.Name = "Sound Box";
            jobs.Add(jobSound);
            jobSound.IsVoidedOnSoftException = true;

            Job jobStage = new Job();
            jobStage.Name = "Stage";
            jobs.Add(jobStage);
            jobStage.IsVoidedOnSoftException = false;

            Job jobMic1 = new Job();
            jobMic1.Name = "Mic 1";
            jobs.Add(jobMic1);
            jobMic1.IsVoidedOnSoftException = true;

            Job jobMic2 = new Job();
            jobMic2.Name = "Mic 2";
            jobs.Add(jobMic2);
            jobMic2.IsVoidedOnSoftException = true;

            Job jobMic3 = new Job();
            jobMic3.Name = "Mic 3";
            jobs.Add(jobMic3);
            jobMic3.IsVoidedOnSoftException = true;

            Job jobMic4 = new Job();
            jobMic4.Name = "Mic 4";
            jobs.Add(jobMic4);
            jobMic4.IsVoidedOnSoftException = true;

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userDCook);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userDLopez);

            User userEWilder = new User();
            userEWilder.Name = "Ethan Wilder";
            userEWilder.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userEWilder);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userRStubbs);

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userDKeil);

            User userBDeaver = new User();
            userBDeaver.Name = "Beau Deaver";
            userBDeaver.Jobs = new List<Job> { jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };
            users.Add(userBDeaver);

            Meeting.Data templateSunday = new Meeting.Data();
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2, jobMic3, jobMic4 };

            Meeting.Data templateTuesday = new Meeting.Data();
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic1, jobMic2 };

            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));
            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));
            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));
            meetings.Add(new Meeting(templateSunday));
            meetings.Add(new Meeting(templateTuesday));

            foreach (Meeting meeting in meetings) {
                templates.Add(meeting.ToTemplate());
            }

            meetings[7].AddUserForJob(userCTangen, jobSound);

            JobConsideration consideration = new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            consideration = new JobConsiderationUsersWhoCantDoJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            consideration = new JobConsiderationEvenUserDistributionPerJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            consideration = new JobConsiderationGiveUsersABreak.Builder()
                .SetGiveBreakOnDay(4)
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            jobConsiderations.Add(consideration);

            // Act
            SoundBuilderV3.ActionFillMeetingsAll action = new SoundBuilderV3.ActionFillMeetingsAll.Builder()
                .SetJobConsiderations(jobConsiderations)
                .SetMeetings(meetings)
                .SetUsers(users)
                .Build();
            action.PerformAction();

            // Assert
            Assert.AreEqual(true, true);
        }

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
            float invalidCount1 = consideration.IsValid(solution1);
            float invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(3, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        [TestMethod]
        public void JobConsideration_SubstituteJobAvailability_DoesFail() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";
            jobs[0].CanBeSubstituded = true;
            jobs[1].CanBeSubstituded = true;
            jobs[2].CanBeSubstituded = true;
            jobs[3].CanBeSubstituded = true;

            Template template1 = new Template(2);
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1 };

            template1.AddJobToAllPartitions(jobs[0]);
            template1.AddJobToPartition(jobs[1], 1);

            int[] solution1 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);

            UserExceptionType substituteRequired = new UserExceptionType();
            substituteRequired.AddSubRequiresAvailability(jobs[1], true);

            UserExceptionType substituteException = new UserExceptionType();
            substituteException.AddJobExceptionValue(jobs[1], (float)0.5);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();
            exceptions.AddUserException(substituteException, users.IndexOf(users[0]), 0, 1);

            // Act
            JobConsideration consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobs[0])
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .SetUserExceptions(exceptions)
                .Build();
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(0, users.IndexOf(users[1]), 1, substituteRequired);
            float invalidCount1 = consideration.IsValid(solution1);

            // Assert
            Assert.AreEqual((float)0.5, invalidCount1);
        }

        [TestMethod]
        public void JobConsideration_SubstituteJobAvailability_DoesPassNoSubJob() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";

            Template template1 = new Template(2);
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1 };

            template1.AddJobToAllPartitions(jobs[0]);
            template1.AddJobToPartition(jobs[1], 1);

            int[] solution1 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);

            UserExceptionType substituteRequired = new UserExceptionType();
            substituteRequired.AddSubRequiresAvailability(jobs[1], true);

            UserExceptionType substituteException = new UserExceptionType();
            substituteException.AddJobExceptionValue(jobs[1], (float)0.5);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();
            exceptions.AddUserException(substituteException, users.IndexOf(users[0]), 0, 1);

            // Act
            JobConsideration consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobs[2])
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .SetUserExceptions(exceptions)
                .Build();
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(0, users.IndexOf(users[1]), 1, substituteRequired);
            float invalidCount1 = consideration.IsValid(solution1);

            // Assert
            Assert.AreEqual(0, invalidCount1);
        }

        [TestMethod]
        public void JobConsideration_SubstituteJobAvailability_DoesPassNoRequirement() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";

            Template template1 = new Template(2);
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1 };

            template1.AddJobToAllPartitions(jobs[0]);
            template1.AddJobToPartition(jobs[1], 1);

            int[] solution1 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);

            UserExceptionType substituteException = new UserExceptionType();
            substituteException.AddJobExceptionValue(jobs[1], (float)0.5);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();
            exceptions.AddUserException(substituteException, users.IndexOf(users[0]), 0, 1);

            // Act
            JobConsideration consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobs[0])
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .SetUserExceptions(exceptions)
                .Build();
            float invalidCount1 = consideration.IsValid(solution1);

            // Assert
            Assert.AreEqual(0, invalidCount1);
        }

        [TestMethod]
        public void JobConsideration_SubstituteJobAvailability_DoesPassRequiresDifferentJob() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";
            jobs[0].CanBeSubstituded = true;
            jobs[1].CanBeSubstituded = true;
            jobs[2].CanBeSubstituded = true;
            jobs[3].CanBeSubstituded = true;

            Template template1 = new Template(2);
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1 };

            template1.AddJobToAllPartitions(jobs[0]);
            template1.AddJobToPartition(jobs[1], 1);

            int[] solution1 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);

            UserExceptionType substituteRequired = new UserExceptionType();
            substituteRequired.AddSubRequiresAvailability(jobs[1], true);

            UserExceptionType substituteException = new UserExceptionType();
            substituteException.AddJobExceptionValue(jobs[3], (float)0.5);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();
            exceptions.AddUserException(substituteException, users.IndexOf(users[0]), 0, 1);

            // Act
            JobConsideration consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobs[0])
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .SetUserExceptions(exceptions)
                .Build();
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(0, users.IndexOf(users[1]), 1, substituteRequired);
            float invalidCount1 = consideration.IsValid(solution1);

            // Assert
            Assert.AreEqual(0, invalidCount1);
        }

        [TestMethod]
        public void JobConsideration_SubstituteJobAvailability_DoesPassSubUserAvailable() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";
            jobs[0].CanBeSubstituded = true;
            jobs[1].CanBeSubstituded = true;
            jobs[2].CanBeSubstituded = true;
            jobs[3].CanBeSubstituded = true;

            Template template1 = new Template(2);
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1 };

            template1.AddJobToAllPartitions(jobs[0]);
            template1.AddJobToPartition(jobs[1], 1);

            int[] solution1 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);

            UserExceptionType substituteRequired = new UserExceptionType();
            substituteRequired.AddSubRequiresAvailability(jobs[1], true);

            UserExceptionType substituteException = new UserExceptionType();
            substituteException.AddJobExceptionValue(jobs[3], (float)0.5);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();
            exceptions.AddUserException(substituteException, users.IndexOf(users[0]), 0, 2);

            // Act
            JobConsideration consideration = new JobConsiderationSubstituteJobAvailability.Builder()
                .SetSubstituteJob(jobs[0])
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .SetUserExceptions(exceptions)
                .Build();
            ((JobConsiderationSubstituteJobAvailability)consideration).AddNeedForAvailability(0, users.IndexOf(users[1]), 1, substituteRequired);
            float invalidCount1 = consideration.IsValid(solution1);

            // Assert
            Assert.AreEqual(0.5, invalidCount1);
        }

        //[TestMethod]
        //public void JobConsideration_UsersWhoAlreadyHaveJob_Paritions() {

        //    // Arrange
        //    List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job(), new Job() };
        //    List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

        //    jobs[0].Name = "Job0";
        //    jobs[1].Name = "Job1";
        //    jobs[2].Name = "Job2";
        //    jobs[3].Name = "Job3";

        //    Template template1 = new Template(2);
        //    template1.Jobs.Add(jobs[0]);
        //    template1.Jobs.Add(jobs[1]);
        //    template1.Jobs.Add(jobs[2]);
        //    template1.Jobs.Add(jobs[3]);
        //    List<Template> templates = new List<Template> { template1 };

        //    template1.AddJobToAllPartitions(jobs[0]);
        //    template1.AddJobToPartition(jobs[1], 1);
        //    template1.AddJobToPartition(jobs[2], 2);
        //    template1.AddJobToPartition(jobs[3], 1);

        //    int[] solution1 = new int[jobs.Count];
        //    int[] solution2 = new int[jobs.Count];

        //    solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
        //    solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
        //    solution1[jobs.IndexOf(jobs[2])] = users.IndexOf(users[0]);
        //    solution1[jobs.IndexOf(jobs[3])] = users.IndexOf(users[2]);

        //    solution2[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
        //    solution2[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
        //    solution2[jobs.IndexOf(jobs[2])] = users.IndexOf(users[1]);
        //    solution2[jobs.IndexOf(jobs[3])] = users.IndexOf(users[3]);

        //    // Act
        //    JobConsideration consideration = new JobConsiderationUsersWhoAlreadyHaveJobOld.Builder()
        //        .SetJobs(jobs)
        //        .SetTemplates(templates)
        //        .SetUsers(users)
        //        .Build();
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(0, users.IndexOf(users[2]), 1, 1);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(0, users.IndexOf(users[3]), 1, 2);
        //    float invalidCount1 = consideration.IsValid(solution1);
        //    float invalidCount2 = consideration.IsValid(solution2);

        //    // Assert
        //    Assert.AreEqual(2, invalidCount1);
        //    Assert.AreEqual(1, invalidCount2);
        //}

        [TestMethod]
        public void JobConsideration_UsersWhoAlreadyHaveJob() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";
            jobs[2].Name = "Job2";
            jobs[3].Name = "Job3";
            jobs[4].Name = "Job4";

            Template template1 = new Template();
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            template1.Jobs.Add(jobs[2]);
            template1.Jobs.Add(jobs[3]);
            template1.Jobs.Add(jobs[4]);
            List<Template> templates = new List<Template> { template1 };

            foreach (Job job in jobs) {
                template1.AddJobToPartition(job, 1);
            }

            int[] solution1 = new int[jobs.Count];
            int[] solution2 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[3]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);
            solution1[jobs.IndexOf(jobs[3])] = users.IndexOf(users[1]);
            solution1[jobs.IndexOf(jobs[4])] = users.IndexOf(users[3]);

            solution2[jobs.IndexOf(jobs[0])] = users.IndexOf(users[4]);
            solution2[jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);
            solution2[jobs.IndexOf(jobs[2])] = users.IndexOf(users[1]);
            solution2[jobs.IndexOf(jobs[3])] = users.IndexOf(users[0]);
            solution2[jobs.IndexOf(jobs[4])] = users.IndexOf(users[2]);

            // Act
            JobConsideration consideration = new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            ((JobConsiderationUsersWhoAlreadyHaveJob)consideration).AddAllJobCombos(jobs[3], (float)0.5);
            float invalidCount1 = consideration.IsValid(solution1);
            float invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(1.5, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        //[TestMethod]
        //public void JobConsideration_UsersWhoAlreadyHaveJob_WithExceptions() {

        //    // Arrange
        //    List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job(), new Job(), new Job() };
        //    List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User(), new User(), new User(), new User(), new User(), new User() };

        //    jobs[0].Name = "Job0";
        //    jobs[1].Name = "Job1";
        //    jobs[2].Name = "Job2";
        //    jobs[3].Name = "Job3";
        //    jobs[4].Name = "Job4";
        //    jobs[5].Name = "Job5";

        //    Template template1 = new Template();
        //    template1.Jobs.Add(jobs[0]);
        //    template1.Jobs.Add(jobs[1]);
        //    template1.Jobs.Add(jobs[2]);
        //    template1.Jobs.Add(jobs[3]);
        //    template1.Jobs.Add(jobs[4]);
        //    template1.Jobs.Add(jobs[5]);
        //    List<Template> templates = new List<Template> { template1 };

        //    foreach (Job job in jobs) {
        //        template1.AddJobToPartition(job, 1);
        //    }

        //    int[] solution1 = new int[jobs.Count];
        //    int[] solution2 = new int[jobs.Count];

        //    solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
        //    solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
        //    solution1[jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);
        //    solution1[jobs.IndexOf(jobs[3])] = users.IndexOf(users[3]);
        //    solution1[jobs.IndexOf(jobs[4])] = users.IndexOf(users[4]);
        //    solution1[jobs.IndexOf(jobs[5])] = users.IndexOf(users[5]);

        //    solution2[jobs.IndexOf(jobs[0])] = users.IndexOf(users[4]);
        //    solution2[jobs.IndexOf(jobs[1])] = users.IndexOf(users[5]);
        //    solution2[jobs.IndexOf(jobs[2])] = users.IndexOf(users[6]);
        //    solution2[jobs.IndexOf(jobs[3])] = users.IndexOf(users[7]);
        //    solution2[jobs.IndexOf(jobs[4])] = users.IndexOf(users[8]);
        //    solution2[jobs.IndexOf(jobs[5])] = users.IndexOf(users[9]);

        //    // Act
        //    JobConsideration consideration = new JobConsiderationUsersWhoAlreadyHaveJobOld.Builder()
        //        .SetJobs(jobs)
        //        .SetTemplates(templates)
        //        .SetUsers(users)
        //        .Build();
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(0, users.IndexOf(users[0]), 1);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(0, users.IndexOf(users[1]), 1);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(0, users.IndexOf(users[2]), (float)0.5);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddException(0, users.IndexOf(users[3]), (float)0.5);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddJobToException(jobs[1], 0);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddJobToException(jobs[3], 0);
        //    ((JobConsiderationUsersWhoAlreadyHaveJobOld)consideration).AddJobToException(jobs[5], 0);
        //    float invalidCount1 = consideration.IsValid(solution1);
        //    float invalidCount2 = consideration.IsValid(solution2);

        //    // Assert
        //    Assert.AreEqual(3.5, invalidCount1);
        //    Assert.AreEqual(0, invalidCount2);
        //}

        [TestMethod]
        public void JobConsideration_UsersWhoHaveExceptions() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User(), new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";
            jobs[2].Name = "Job2";

            Template template1 = new Template(2);
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            template1.Jobs.Add(jobs[2]);
            List<Template> templates = new List<Template> { template1 };

            template1.AddJobToPartition(jobs[0], 1);
            template1.AddJobToPartition(jobs[1], 2);
            template1.AddJobToAllPartitions(jobs[2]);

            int[] solution1 = new int[jobs.Count];
            int[] solution2 = new int[jobs.Count];

            solution1[jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);

            solution2[jobs.IndexOf(jobs[0])] = users.IndexOf(users[1]);
            solution2[jobs.IndexOf(jobs[1])] = users.IndexOf(users[0]);
            solution1[jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);

            UserExceptionType exception1 = new UserExceptionType();
            exception1.AddJobExceptionValue(jobs[0], 1);

            UserExceptionType exception2 = new UserExceptionType();
            exception2.AddJobExceptionValue(jobs[1], (float)0.5);

            UserExceptionDictionary exceptions = new UserExceptionDictionary();
            exceptions.AddUserException(exception1, users.IndexOf(users[0]), 0, 1);
            exceptions.AddUserException(exception2, users.IndexOf(users[1]), 0, 2);
            exceptions.AddUserException(exception1, users.IndexOf(users[2]), 0, 1);

            // Act
            JobConsideration consideration = new JobConsiderationUsersWhoHaveExceptions.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .SetUserExceptions(exceptions)
                .Build();
            float invalidCount1 = consideration.IsValid(solution1);
            float invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(1.5, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        [TestMethod]
        public void JobConsideration_EvenUserDistributionPerJob_WithoutSameJobs() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].Name = "Job0";
            jobs[1].Name = "Job1";

            users[0].Jobs.Add(jobs[0]);
            users[1].Jobs.Add(jobs[0]);
            users[2].Jobs.Add(jobs[0]);
            users[2].Jobs.Add(jobs[1]);
            users[3].Jobs.Add(jobs[1]);
            users[4].Jobs.Add(jobs[1]);

            Template template1 = new Template();
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1, template1, template1, template1 };

            int[] solution1 = new int[jobs.Count * templates.Count];
            int[] solution2 = new int[jobs.Count * templates.Count];

            solution1[0 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[4]);
            solution1[0 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[0]);
            solution1[2 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[1]);
            solution1[2 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[4]);
            solution1[4 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[4]);
            solution1[4 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[2]);
            solution1[6 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[3]);
            solution1[6 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[4]);


            solution2[0 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[0 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[2 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[2]);
            solution2[2 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);
            solution2[4 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[4]);
            solution2[4 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[0]);
            solution2[6 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[1]);
            solution2[6 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[2]);

            // Act
            JobConsideration consideration = new JobConsiderationEvenUserDistributionPerJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            float invalidCount1 = consideration.IsValid(solution1);
            float invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(2, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        [TestMethod]
        public void JobConsideration_GiveUsersABreak() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User() };

            Template template1 = new Template();
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            List<Template> templates = new List<Template> { template1, template1, template1, template1, template1, template1, template1, template1 };

            int[] solution1 = new int[jobs.Count * templates.Count];
            int[] solution2 = new int[jobs.Count * templates.Count];

            solution1[0 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[2]);
            solution1[0 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);
            solution1[2 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[2 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[4 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[4 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[6 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[6 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[8 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[8 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[10 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[10 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[12 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[12 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[14 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[2]);
            solution1[14 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);


            solution2[0 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[0 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[2 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[2 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[4 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[2]);
            solution2[4 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);
            solution2[6 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[6 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[8 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[8 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[10 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[10 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[12 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[2]);
            solution2[12 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[3]);
            solution2[14 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[14 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);

            // Act
            JobConsideration consideration = new JobConsiderationGiveUsersABreak.Builder()
                .SetGiveBreakOnDay(4)
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            float invalidCount1 = consideration.IsValid(solution1);
            float invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(6, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        [TestMethod]
        public void JobConsideration_UsersWhoDidSameJobLastMeeting() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

            jobs[0].AddSameJob(jobs[2]);

            jobs[0].Name = "Job 0";
            jobs[1].Name = "Job 1";
            jobs[2].Name = "Job 2";

            users[0].Jobs = new List<Job> { jobs[0], jobs[1], jobs[2] };
            users[1].Jobs = new List<Job> { jobs[0], jobs[1], jobs[2] };
            users[2].Jobs = new List<Job> { jobs[0], jobs[1], jobs[2] };
            users[3].Jobs = new List<Job> { jobs[0], jobs[1], jobs[2] };
            users[4].Jobs = new List<Job> { jobs[0], jobs[1], jobs[2] };

            Template template1 = new Template();
            template1.Jobs.Add(jobs[0]);
            template1.Jobs.Add(jobs[1]);
            template1.Jobs.Add(jobs[2]);
            List<Template> templates = new List<Template> { template1, template1, template1 };

            int[] solution1 = new int[jobs.Count * templates.Count];
            int[] solution2 = new int[jobs.Count * templates.Count];

            solution1[0 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[0 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[0 + jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);
            solution1[3 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution1[3 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution1[3 + jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);
            solution1[6 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[3]);
            solution1[6 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[2]);
            solution1[6 + jobs.IndexOf(jobs[2])] = users.IndexOf(users[0]);

            solution2[0 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[0 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[0 + jobs.IndexOf(jobs[2])] = users.IndexOf(users[2]);
            solution2[3 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[3]);
            solution2[3 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[0]);
            solution2[3 + jobs.IndexOf(jobs[2])] = users.IndexOf(users[1]);
            solution2[6 + jobs.IndexOf(jobs[0])] = users.IndexOf(users[0]);
            solution2[6 + jobs.IndexOf(jobs[1])] = users.IndexOf(users[1]);
            solution2[6 + jobs.IndexOf(jobs[2])] = users.IndexOf(users[4]);

            // Act
            JobConsideration consideration = new JobConsiderationUsersWhoDidSameJobLastMeeting.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            float invalidCount1 = consideration.IsValid(solution1);
            float invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(4, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }
    }
}
