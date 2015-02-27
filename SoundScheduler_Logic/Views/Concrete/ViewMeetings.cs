using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Views.Abstract;

namespace SoundScheduler_Logic.Views.Concrete {
    public class ViewMeetings : IViewMeetings {
        private Factory _factory;
        private Repository _repository;

        private List<Meeting> _meetings;
        public IEnumerable<Meeting> Meetings {
            get {
                if (_meetings == null) {
                    return new List<Meeting>();
                } else {
                    return _meetings;
                }
            }
        }

        public IEnumerable<Template> Templates {
            get { return _repository.Templates; }
        }

        private List<MeetingNode> _nodes;
        public IEnumerable<MeetingNode> Nodes {
            get {
                if (_nodes == null) {
                    return new List<MeetingNode>();
                } else {
                    return _nodes.OrderBy(x => x.OrderById);
                }
            }
        }

        public string NodeDescriptionMeeting(Meeting meeting) {
            return meeting.Date.ToShortDateString();
        }

        public string NodeDescriptionJob(Meeting meeting, Job job) {
            User user = meeting.UserForJob(job);
            if (user == null) {
                return job.Name + ": <empty>";
            } else {
                return job.Name + ": " + user.Name;
            }
        }

        public void AddMeeting(Template template, DateTime date) {
            Meeting meeting = template.ToMeeting(date);
            _meetings.Add(meeting);
        }

        public void LoadFromSource() {
            _repository = _factory.CreateRepository();
            _repository.LoadFromSource();
            _meetings = _repository.Meetings.OrderBy(x => x.Date).ToList();
            BuildNodes();
        }

        public void SaveToSource() {
            _repository.SaveToSource();
        }

        private void BuildNodes() {
            _nodes = new List<MeetingNode>();
            int sortOrder = 0;
            foreach (Meeting meeting in this.Meetings.OrderBy(x => x.Date)) {

                sortOrder++;
            }
        }

        public ViewMeetings(Factory factory) {
            _factory = factory;
        }
    }
}
