using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Abstract {
    public class Repository {
        private string _saveFile;
        private RepositoryData _data;

        public List<Job> Jobs {
            get { return _data.Jobs; }
            set { _data.Jobs = value; }
        }

        public List<Template> Templates {
            get { return _data.Templates; }
            set { _data.Templates = value; }
        }

        public List<User> Users {
            get { return _data.Users; }
            set { _data.Users = value; }
        }

        public List<Meeting> Meetings {
            get { return _data.Meetings; }
            set { _data.Meetings = value; }
        }

        public List<MeetingException> MeetingExceptions {
            get { return _data.MeetingExceptions; }
            set { _data.MeetingExceptions = value; }
        }

        public void SaveToSource() {
            using (FileStream writer = new FileStream("_SoundScheduler.xml", FileMode.Create, FileAccess.Write)) {
                DataContractSerializer serializer = new DataContractSerializer(typeof(RepositoryData), SerializerSettings());
                serializer.WriteObject(writer, _data);
                writer.Close();
            }
            File.Copy("_SoundScheduler.xml", "SoundScheduler.xml", true);
        }

        public void LoadFromSource() {
            if (File.Exists("SoundScheduler.xml")) {
                using (FileStream reader = new FileStream("SoundScheduler.xml", FileMode.Open, FileAccess.Read)) {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(RepositoryData), SerializerSettings());
                    _data = (RepositoryData)serializer.ReadObject(reader);
                    reader.Close();
                }
            }
        }

        private DataContractSerializerSettings SerializerSettings() {
            DataContractSerializerSettings settings = new DataContractSerializerSettings();
            settings.PreserveObjectReferences = true;
            return settings;
        }

        public Repository() {
            _saveFile = "SoundScheduler.xml";
            Instantiate();
        }

        public Repository(string saveFile) {
            _saveFile = saveFile;
            Instantiate();
        }

        private void Instantiate() {
            _data = new RepositoryData();
        }
        
        public class RepositoryData {
            public List<Job> Jobs { get; set; }
            public List<Template> Templates { get; set; }
            public List<User> Users { get; set; }
            public List<MeetingException> MeetingExceptions { get; set; }
            public List<Meeting> Meetings { get; set; }

            public RepositoryData() {
                this.Jobs = new List<Job>();
                this.Templates = new List<Template>();
                this.Users = new List<User>();
                this.MeetingExceptions = new List<MeetingException>();
                this.Meetings = new List<Meeting>();
            }
        }
    }
}
