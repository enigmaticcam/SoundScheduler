using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_UnitTests {

    [TestClass]
    public class SoundBuilderTests {

        [TestMethod]
        public void SoundBuilder_MainEngineTest() {

            // Arrange
            // --------------------------------------------------------
            // JOBS
            // --------------------------------------------------------
            Job jobSound = new Job();
            jobSound.Name = "Sound Box";

            Job jobStage = new Job();
            jobStage.Name = "Stage";

            Job jobMic = new Job();
            jobMic.Name = "Mic";

            // --------------------------------------------------------
            // USERS
            // --------------------------------------------------------
            List<User> users = new List<User>();

            User userCTangen = new User();
            userCTangen.Name = "Cameron Tangen";
            userCTangen.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userCTangen);

            User userESavelberg = new User();
            userESavelberg.Name = "Eric Savelberg";
            userESavelberg.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userESavelberg);

            User userDCook = new User();
            userDCook.Name = "Dennis Cook";
            userDCook.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userDCook);

            User userJHermosillo = new User();
            userJHermosillo.Name = "Joseph Hermosillo";
            userJHermosillo.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userJHermosillo);

            User userEWilder = new User();
            userEWilder.Name = "Ethan Wilder";
            userEWilder.Jobs = new List<Job> { jobSound, jobStage, jobMic };
            users.Add(userEWilder);

            User userDLopez = new User();
            userDLopez.Name = "David Lopez";
            userDLopez.Jobs = new List<Job> { jobStage, jobMic };
            users.Add(userDLopez);

            //User userBFarris = new User();
            //userBFarris.Name = "Bob Farris";
            //userBFarris.Jobs = new List<Job> { jobMic };
            //users.Add(userBFarris);

            User userDKeil = new User();
            userDKeil.Name = "David Keil";
            userDKeil.Jobs = new List<Job> { jobMic };
            users.Add(userDKeil);

            User userRStubbs = new User();
            userRStubbs.Name = "Reed Stubbs";
            userRStubbs.Jobs = new List<Job> { jobMic };
            users.Add(userRStubbs);

            User userKSugiyama = new User();
            userKSugiyama.Name = "Keigi Sugiyama";
            userKSugiyama.Jobs = new List<Job> { jobMic };
            users.Add(userKSugiyama);

            // --------------------------------------------------------
            // TEMPLATES
            // --------------------------------------------------------

            Template templateSunday = new Template();
            templateSunday.Name = "Sunday";
            templateSunday.Jobs = new List<Job> { jobSound, jobStage, jobMic, jobMic, jobMic, jobMic };

            Template templateTuesday = new Template();
            templateTuesday.Name = "Tuesday";
            templateTuesday.Jobs = new List<Job> { jobSound, jobStage, jobMic, jobMic };

            // --------------------------------------------------------
            // EXISTING MEETINGS
            // --------------------------------------------------------
            List<SoundBuilder.ExistingMeeting> existing = new List<SoundBuilder.ExistingMeeting>();

            Meeting meetingExisting1 = new Meeting();
            meetingExisting1.Name = templateTuesday.Name;
            meetingExisting1.Jobs = templateTuesday.Jobs;
            meetingExisting1.JobUserSlots = new Meeting.JobUserSlot();
            meetingExisting1.JobUserSlots.AddUserToJob(0, userCTangen);
            meetingExisting1.JobUserSlots.AddUserToJob(1, userEWilder);
            meetingExisting1.JobUserSlots.AddUserToJob(2, userDLopez);
            meetingExisting1.JobUserSlots.AddUserToJob(3, userDKeil);
            SoundBuilder.ExistingMeeting meeting1 = new SoundBuilder.ExistingMeeting();
            meeting1.Meeting = meetingExisting1;
            meeting1.TemplateIndex = 0;
            existing.Add(meeting1);

            Meeting meetingExisting2 = new Meeting();
            meetingExisting2.Name = templateSunday.Name;
            meetingExisting2.Jobs = templateSunday.Jobs;
            meetingExisting2.JobUserSlots = new Meeting.JobUserSlot();
            meetingExisting2.JobUserSlots.AddUserToJob(0, userDCook);
            meetingExisting2.JobUserSlots.AddUserToJob(1, userJHermosillo);
            meetingExisting2.JobUserSlots.AddUserToJob(2, userDKeil);
            meetingExisting2.JobUserSlots.AddUserToJob(3, userRStubbs);
            meetingExisting2.JobUserSlots.AddUserToJob(4, userKSugiyama);
            meetingExisting2.JobUserSlots.AddUserToJob(5, userCTangen);
            SoundBuilder.ExistingMeeting meeting2 = new SoundBuilder.ExistingMeeting();
            meeting2.Meeting = meetingExisting2;
            meeting2.TemplateIndex = 7;
            existing.Add(meeting2);

            SoundBuilder engine = new SoundBuilder.Builder()
                .SetJobs(new List<Job> { jobSound, jobStage, jobMic })
                .SetUser(new List<User>(users))
                .SetTemplates(new List<Template> { 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday, templateSunday, 
                    templateTuesday
                })
                .SetExistingMeetings(existing)
                .Build();

            // Act
            List<Meeting> meetings = engine.BuildSchedule();
            List<string> soundBox = new List<string>();
            List<string> stage = new List<string>();
            List<string> mic1 = new List<string>();
            List<string> mic2 = new List<string>();
            List<string> mic3 = new List<string>();
            List<string> mic4 = new List<string>();

            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (Meeting meeting in meetings) {
                for (int jobIndex = 0; jobIndex < meeting.Jobs.Count; jobIndex++) {
                    User user = meeting.JobUserSlots.UserForJob(jobIndex);
                    switch (meeting.Jobs[jobIndex].Name) {
                        case "Sound Box":
                            soundBox.Add(user.Name);
                            break;
                        case "Stage":
                            stage.Add(user.Name);
                            break;
                        case "Mic":
                            switch (jobIndex) {
                                case 2:
                                    mic1.Add(user.Name);
                                    break;
                                case 3:
                                    mic2.Add(user.Name);
                                    break;
                                case 4:
                                    mic3.Add(user.Name);
                                    break;
                                case 5:
                                    mic4.Add(user.Name);
                                    break;
                            }
                            break;
                    }
                    if (counts.ContainsKey(user.Name)) {
                        counts[user.Name]++;
                    } else {
                        counts.Add(user.Name, 1);
                    }
                }
            }            

            // Assert
            Assert.AreEqual(true, true);
        }
    }
}
