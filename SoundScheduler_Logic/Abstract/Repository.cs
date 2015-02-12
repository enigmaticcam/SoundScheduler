using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SoundScheduler_Logic.Abstract {
    public class Repository {
        private RepositoryData _data = new RepositoryData();

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

        public void SaveToSource() {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(typeof(RepositoryData));
            using (MemoryStream stream = new MemoryStream()) {
                serializer.Serialize(stream, _data);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save("SoundScheduler.dat");
                stream.Close();
            }
        }

        public void LoadFromSource() {
            if (File.Exists("SoundScheduler.dat")) {
                string attributeXml = string.Empty;

                RepositoryData objectOut = default(RepositoryData);

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load("SoundScheduler.dat");
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString)) {

                    XmlSerializer serializer = new XmlSerializer(typeof(RepositoryData));
                    using (XmlReader reader = new XmlTextReader(read)) {
                        objectOut = (RepositoryData)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
                _data = objectOut;
            } else {
                _data = new RepositoryData();
            }
        }

        [Serializable]
        public class RepositoryData {
            public List<Job> Jobs { get; set; }
            public List<Template> Templates { get; set; }
            public List<User> Users { get; set; }
            public List<MeetingException> MeetingExceptions { get; set; }

            public RepositoryData() {
                this.Jobs = new List<Job>();
                this.Templates = new List<Template>();
                this.Users = new List<User>();
                this.MeetingExceptions = new List<MeetingException>();
            }
        }
    }
}
