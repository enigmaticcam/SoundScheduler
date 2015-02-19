using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Repository {
        public List<Job> Jobs { get; set; }
        public List<Template> Templates { get; set; }
        public List<User> Users { get; set; }
        public List<Meeting> Meetings { get; set; }

        public void SaveToSource() {
            using (FileStream writer = new FileStream("SoundScheduler.xml", FileMode.Create, FileAccess.Write)) {
                DataContractSerializer serializer = new DataContractSerializer(typeof(RepositoryData), SerializerSettings());
                serializer.WriteObject(writer, CreateRepositoryData());
                writer.Close();
            }
        }

        public void LoadFromSource() {
            if (File.Exists("SoundScheduler.xml")) {
                using (FileStream reader = new FileStream("SoundScheduler.xml", FileMode.Open, FileAccess.Read)) {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(RepositoryData), SerializerSettings());
                    ExtractFromRepositoryData((RepositoryData)serializer.ReadObject(reader));
                    reader.Close();
                }
            }
        }

        private DataContractSerializerSettings SerializerSettings() {
            DataContractSerializerSettings settings = new DataContractSerializerSettings();
            settings.PreserveObjectReferences = true;
            return settings;
        }

        private RepositoryData CreateRepositoryData() {
            RepositoryData data = new RepositoryData();
            AddToRepositoryJobs(data);
            AddToRepositoryMeetings(data);
            return data;
        }

        private void ExtractFromRepositoryData(RepositoryData data) {
            this.Jobs = data.Jobs.Select(x => new Job(x)).ToList();
            this.Meetings = data.Meetings.Select(x => new Meeting(x)).ToList();
        }

        private void AddToRepositoryJobs(RepositoryData data) {
            foreach (Job job in this.Jobs) {
                data.Jobs.Add(job.JobData);
            } 
        }

        private void AddToRepositoryMeetings(RepositoryData data) {
            foreach (Meeting meeting in this.Meetings) {
                data.Meetings.Add(meeting.MeetingData);
            }
        }

        public Repository() {
            this.Jobs = new List<Job>();
            this.Templates = new List<Template>();
            this.Users = new List<User>();
            this.Meetings = new List<Meeting>();
        }
        
        public class RepositoryData {
            public List<Job.Data> Jobs { get; set; }
            public List<Template> Templates { get; set; }
            public List<User> Users { get; set; }
            public List<MeetingException> MeetingExceptions { get; set; }
            public List<Meeting.Data> Meetings { get; set; }

            public RepositoryData() {
                this.Jobs = new List<Job.Data>();
                this.Templates = new List<Template>();
                this.Users = new List<User>();
                this.MeetingExceptions = new List<MeetingException>();
                this.Meetings = new List<Meeting.Data>();
            }
        }
    }
}
