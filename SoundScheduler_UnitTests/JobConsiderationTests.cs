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
            int invalidCount1 = consideration.IsValid(solution1);
            int invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(3, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        [TestMethod]
        public void JobConsideration_UsersWhoAlreadyHaveJob() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job(), new Job(), new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

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
            solution2[jobs.IndexOf(jobs[3])] = users.IndexOf(users[0]);
            solution2[jobs.IndexOf(jobs[4])] = users.IndexOf(users[2]);

            // Act
            JobConsideration consideration = new JobConsiderationUsersWhoAlreadyHaveJob.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUsers(users)
                .Build();
            int invalidCount1 = consideration.IsValid(solution1);
            int invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(1, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }

        [TestMethod]
        public void JobConsideration_EvenUserDistributionPerJob() {

            // Arrange
            List<Job> jobs = new List<Job> { new Job(), new Job() };
            List<User> users = new List<User> { new User(), new User(), new User(), new User(), new User() };

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
            int invalidCount1 = consideration.IsValid(solution1);
            int invalidCount2 = consideration.IsValid(solution2);

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
            int invalidCount1 = consideration.IsValid(solution1);
            int invalidCount2 = consideration.IsValid(solution2);

            // Assert
            Assert.AreEqual(6, invalidCount1);
            Assert.AreEqual(0, invalidCount2);
        }
    }
}
