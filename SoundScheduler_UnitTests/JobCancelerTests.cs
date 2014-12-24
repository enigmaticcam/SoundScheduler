﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_UnitTests {
    [TestClass]
    public class JobCancelerTests {

        [TestMethod]
        public void JobCanceler_DoesRemoveUsersWhoHaveExceptions() {

            // Arrange
            Job job1 = new Job();
            Job job2 = new Job();
            User user1 = new User();
            User user2 = new User();

            Template template1 = new Template();
            template1.Jobs.Add(job1);
            template1.Jobs.Add(job2);

            Meeting meeting = template1.ToMeeting(DateTime.Parse("12/1/2014"));
            meeting.AddUserForJob(user1, job1);

            SoundBuilderV2.MeetingsByDate meetings = new SoundBuilderV2.MeetingsByDate();
            meetings.AddMeeting(meeting);

            SoundBuilderV2.ExceptionsByDate exceptions = new SoundBuilderV2.ExceptionsByDate();
            exceptions.AddException(DateTime.Parse("12/1/2014"), user1);

            SoundBuilderV2.SoundMetrics metrics = new SoundBuilderV2.SoundMetrics.Builder()
                .SetCurrentDate(DateTime.Parse("12/1/2014"))
                .SetCurrentJob(job1)
                .SetMeetings(meetings)
                .SetExceptions(exceptions)
                .SetUsers(new HashSet<User> { user1, user2 })
                .Build();

            // Act
            JobCancelUsersWhoHaveExceptions jobCancel = new JobCancelUsersWhoHaveExceptions();
            HashSet<User> canceledUsers = jobCancel.CancelUsers(metrics);

            // Assert
            Assert.AreEqual(true, canceledUsers.Contains(user1));
            Assert.AreEqual(false, canceledUsers.Contains(user2));
        }

        [TestMethod]
        public void JobCanceler_DoesRemoveUsersWhoAlreadyHaveJob() {

            // Arrange
            Job job1 = new Job();
            Job job2 = new Job();
            User user1 = new User();
            User user2 = new User();

            Template template1 = new Template();
            template1.Jobs.Add(job1);
            template1.Jobs.Add(job2);

            Meeting meeting = template1.ToMeeting(DateTime.Parse("12/1/2014"));
            meeting.AddUserForJob(user1, job1);

            SoundBuilderV2.MeetingsByDate meetings = new SoundBuilderV2.MeetingsByDate();
            meetings.AddMeeting(meeting);

            SoundBuilderV2.SoundMetrics metrics = new SoundBuilderV2.SoundMetrics.Builder()
                .SetCurrentDate(DateTime.Parse("12/1/2014"))
                .SetCurrentJob(job1)
                .SetMeetings(meetings)
                .SetUsers(new HashSet<User> { user1, user2})
                .Build();

            // Act
            JobCancelUsersWhoAlreadyHaveJob jobCancel = new JobCancelUsersWhoAlreadyHaveJob();
            HashSet<User> canceledUsers = jobCancel.CancelUsers(metrics);

            // Assert
            Assert.AreEqual(true, canceledUsers.Contains(user1));
            Assert.AreEqual(false, canceledUsers.Contains(user2));
        }

        [TestMethod]
        public void JobCanceler_DoesRemoveUsersWhoCantDoJob() {

            // Arrange
            Job job1 = new Job();
            Job job2 = new Job();

            User user1 = new User();
            user1.Jobs.Add(job2);
            User user2 = new User();
            user2.Jobs.Add(job1);
            user2.Jobs.Add(job2);

            Template template1 = new Template();
            template1.Jobs.Add(job1);
            template1.Jobs.Add(job2);

            Meeting meeting = template1.ToMeeting(DateTime.Parse("12/1/2014"));
            meeting.AddUserForJob(user1, job1);

            SoundBuilderV2.MeetingsByDate meetings = new SoundBuilderV2.MeetingsByDate();
            meetings.AddMeeting(meeting);

            SoundBuilderV2.SoundMetrics metrics = new SoundBuilderV2.SoundMetrics.Builder()
                .SetCurrentDate(DateTime.Parse("12/1/2014"))
                .SetCurrentJob(job1)
                .SetMeetings(meetings)
                .SetUsers(new HashSet<User> { user1, user2 })
                .Build();

            // Act
            JobCancelUsersWhoCantDoJob jobCancel = new JobCancelUsersWhoCantDoJob();
            HashSet<User> canceledUsers = jobCancel.CancelUsers(metrics);

            // Assert
            Assert.AreEqual(true, canceledUsers.Contains(user1));
            Assert.AreEqual(false, canceledUsers.Contains(user2));
        }

        [TestMethod]
        public void JobCanceler_DoesRemoveUsersWhoNeedABreak() {

            // Arrange
            Job job1 = new Job();
            Job job2 = new Job();

            User user1 = new User();
            User user2 = new User();

            Template template1 = new Template();
            template1.Jobs.Add(job1);
            template1.Jobs.Add(job2);

            Meeting meeting1 = template1.ToMeeting(DateTime.Parse("12/1/2014"));
            Meeting meeting2 = template1.ToMeeting(DateTime.Parse("12/2/2014"));
            Meeting meeting3 = template1.ToMeeting(DateTime.Parse("12/3/2014"));
            Meeting meeting4 = template1.ToMeeting(DateTime.Parse("12/4/2014"));
            Meeting meeting5 = template1.ToMeeting(DateTime.Parse("12/5/2014"));
            Meeting meeting6 = template1.ToMeeting(DateTime.Parse("12/6/2014"));
            
            meeting2.AddUserForJob(user1, job1);
            meeting3.AddUserForJob(user1, job1);
            meeting4.AddUserForJob(user1, job1);
            meeting5.AddUserForJob(user1, job1);

            meeting1.AddUserForJob(user2, job2);
            meeting3.AddUserForJob(user2, job2);
            meeting4.AddUserForJob(user2, job2);
            meeting5.AddUserForJob(user2, job2);

            SoundBuilderV2.MeetingsByDate meetings = new SoundBuilderV2.MeetingsByDate();
            meetings.AddMeeting(meeting1);
            meetings.AddMeeting(meeting2);
            meetings.AddMeeting(meeting3);
            meetings.AddMeeting(meeting4);
            meetings.AddMeeting(meeting5);
            meetings.AddMeeting(meeting6);

            SoundBuilderV2.SoundMetrics metrics = new SoundBuilderV2.SoundMetrics.Builder()
                .SetCurrentDate(DateTime.Parse("12/6/2014"))
                .SetCurrentJob(job1)
                .SetMeetings(meetings)
                .SetUsers(new HashSet<User> { user1, user2 })
                .Build();

            // Act
            JobCancelUserWhoNeedABreak jobCancel = new JobCancelUserWhoNeedABreak();
            HashSet<User> canceledUsers = jobCancel.CancelUsers(metrics);

            // Assert
            Assert.AreEqual(true, canceledUsers.Contains(user1));
            Assert.AreEqual(false, canceledUsers.Contains(user2));
        }
    }
}
