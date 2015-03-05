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

        private List<NodeBase> _nodes;
        public IEnumerable<NodeBase> Nodes {
            get {
                if (_nodes == null) {
                    return new List<NodeBase>();
                } else {
                    return _nodes.OrderBy(x => x.OrderById);
                }
            }
        }

        public IEnumerable<User> Users {
            get { return _repository.Users; }
        }

        public void AddMeeting(Template template, DateTime date) {
            Meeting meeting = template.ToMeeting(date);
            _meetings.Add(meeting);
            BuildNodes();
        }

        public void AddMeetingException(DateTime date, User user, bool isSoftException) {
            MeetingException meetingException = new MeetingException();
            meetingException.Date = date;
            meetingException.User = user;
            meetingException.IsSoftException = isSoftException;
            _repository.MeetingExceptions.Add(meetingException);
            BuildNodes();
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
            ActionBuildNodes action = new ActionBuildNodes(_repository.Meetings, _repository);
            _nodes = action.PerformAction();
        }

        public ViewMeetings(Factory factory) {
            _factory = factory;
        }

        private class ActionBuildNodes {
            private List<NodeBase> _nodes;
            private List<Meeting> _meetings;
            private Repository _repository;

            public List<NodeBase> PerformAction() {
                BuildNodes();
                return _nodes;
            }

            private void BuildNodes() {
                _nodes = new List<NodeBase>();
                ExceptionsByDate meetingExceptions = GetExceptionsByDate();
                int sortOrder = 0;
                foreach (Meeting meeting in _meetings.OrderBy(x => x.Date)) {
                    NodeMeeting node = new NodeMeeting();
                    node.Name = meeting.Date.ToShortDateString() + " - " + meeting.Date.DayOfWeek.ToString();
                    node.OrderById = sortOrder;
                    BuildNodesExceptions(node, meeting, meetingExceptions);
                    BuildNodesJobs(node, meeting);
                    _nodes.Add(node);
                    sortOrder++;
                }
            }

            private void BuildNodesExceptions(NodeBase parentNode, Meeting meeting, ExceptionsByDate exceptions) {
                NodeExceptionList nodeList = new NodeExceptionList();
                nodeList.Name = "Exceptions";
                int sortOrder = 0;
                foreach (MeetingException meetingException in exceptions.ExceptionsForDate(meeting.Date)) {
                    NodeException node = new NodeException();
                    node.Name = NodeDescriptionException(meetingException);
                    node.OrderById = sortOrder;
                    nodeList.AddChild(node);
                    sortOrder++;
                }
                parentNode.AddChild(nodeList);
            }

            private void BuildNodesJobs(NodeBase parentNode, Meeting meeting) {
                int sortOrder = 0;
                foreach (Job job in meeting.Jobs) {
                    NodeJob node = new NodeJob();
                    node.Name = NodeDescriptionJob(meeting, job);
                    node.OrderById = sortOrder;
                    parentNode.AddChild(node);
                    sortOrder++;
                }
            }

            private string NodeDescriptionJob(Meeting meeting, Job job) {
                User user = meeting.UserForJob(job);
                if (user == null) {
                    return job.Name + ": <empty>";
                } else {
                    return job.Name + ": " + user.Name;
                }
            }

            private string NodeDescriptionException(MeetingException meetingException) {
                return meetingException.User.Name + ": " + (meetingException.IsSoftException ? "Soft" : "Hard");
            }

            private ExceptionsByDate GetExceptionsByDate() {
                ExceptionsByDate exceptions = new ExceptionsByDate();
                foreach (MeetingException meetingException in _repository.MeetingExceptions) {
                    exceptions.AddException(meetingException);
                }
                return exceptions;
            }

            public ActionBuildNodes(List<Meeting> meetings, Repository repository) {
                _meetings = meetings;
                _repository = repository;
            }
        }

        private class ExceptionsByDate {
            private Dictionary<string, List<MeetingException>> _data = new Dictionary<string, List<MeetingException>>();

            public void AddException(MeetingException meetingException) {
                string key = DateToKey(meetingException.Date);
                if (!_data.ContainsKey(key)) {
                    _data.Add(key, new List<MeetingException>());
                }
                _data[key].Add(meetingException);
            }

            public IEnumerable<MeetingException> ExceptionsForDate(DateTime date) {
                string key = DateToKey(date);
                if (_data.ContainsKey(key)) {
                    return _data[key];
                } else {
                    return new List<MeetingException>();
                }
            }

            private string DateToKey(DateTime date) {
                return date.Year.ToString() + date.Month.ToString() + date.Day.ToString();
            }
        }

        private class NodeMeeting : NodeBase {
            public override bool CanDelete {
                get { return true; }
            }

            public override enumViewMeetingsAction DeleteAction {
                get { return enumViewMeetingsAction.DeleteMeeting; }
            }
        }

        private class NodeJob : NodeBase {
            public override bool CanDelete {
                get { return true; }
            }

            public override enumViewMeetingsAction DeleteAction {
                get { return enumViewMeetingsAction.DeleteJob; }
            }
        }

        private class NodeExceptionList : NodeBase {
            public override bool CanDelete {
                get { return false; }
            }

            public override enumViewMeetingsAction DeleteAction {
                get { return enumViewMeetingsAction.DeleteNothing; }
            }
        }

        private class NodeException : NodeBase {
            public override bool CanDelete {
                get { return true; }
            }

            public override enumViewMeetingsAction DeleteAction {
                get { return enumViewMeetingsAction.DeleteException; }
            }
        }
    }
}
