using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Views.Abstract {
    public interface IViewMeetings {
        IEnumerable<Meeting> Meetings { get; }
        IEnumerable<Template> Templates { get; }
        IEnumerable<NodeBase> Nodes { get; }
        IEnumerable<User> Users { get; }
        DateTime MeetingDateForNode(int nodeId);
        bool CanDeleteNode(int nodeId);
        bool CanAddMeeting(DateTime date);
        void AddMeeting(Template template, DateTime date);
        void AddMeetingException(DateTime date, User user, bool isSoftException);
        void LoadFromSource();
        void SaveToSource();
    }

    public abstract class ViewMeetingsDecorator {
        private IViewMeetings _view;

        public IEnumerable<Meeting> Meetings {
            get { return _view.Meetings; }
        }

        public IEnumerable<Template> Tempaltes {
            get { return _view.Templates; }
        }

        public IEnumerable<User> Users {
            get { return _view.Users; }
        }

        public DateTime MeetingDateForNode(int nodeId) {
            return _view.MeetingDateForNode(nodeId);
        }

        public bool CanDeleteNode(int nodeId) {
            return _view.CanDeleteNode(nodeId);
        }

        public bool CanAddMeeting(DateTime date) {
            return _view.CanAddMeeting(date);
        }

        public void AddMeeting(Template template, DateTime date) {
            _view.AddMeeting(template, date);
        }

        public void AddMeetingException(DateTime date, User user, bool isSoftException) {
            _view.AddMeetingException(date, user, isSoftException);
        }

        public void LoadFromSource() {
            _view.LoadFromSource();
        }

        public void SaveToSource() {
            _view.SaveToSource();
        }

        public ViewMeetingsDecorator(IViewMeetings view) {
            _view = view;
        }
    }

    public abstract class NodeBase {
        public abstract bool CanDelete { get; }
        public abstract enumViewMeetingsAction DeleteAction { get; }

        private Dictionary<int, NodeBase> _nodesById = new Dictionary<int, NodeBase>();

        public string Name { get; set; }
        public int OrderById { get; set; }
        public int NodeId { get; set; }
        public NodeBase Parent { get; set; }

        private List<NodeBase> _children = new List<NodeBase>();
        public IEnumerable<NodeBase> Children {
            get { return _children.OrderBy(x => x.OrderById); }
        }

        public void AddChild(NodeBase node) {
            node.Parent = this;
            _children.Add(node);
            _nodesById.Add(node.NodeId, node);
        }

        public NodeBase NodeById(int nodeId) {
            if (this.NodeId == nodeId) {
                return this;
            } else if (_nodesById.ContainsKey(nodeId)) {
                return _nodesById[nodeId];
            } else {
                foreach (NodeBase node in _children) {
                    NodeBase possibleNode = node.NodeById(nodeId);
                    if (possibleNode != null) {
                        return possibleNode;
                    }
                }
            }
            return null;
        }
    }

    public enum enumViewMeetingsAction {
        DeleteNothing = 0,
        DeleteJob,
        DeleteMeeting,
        DeleteException
    }
}
