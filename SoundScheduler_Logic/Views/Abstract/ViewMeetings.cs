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
        public string Name { get; set; }
        public int OrderById { get; set; }

        private List<NodeBase> _children;
        public IEnumerable<NodeBase> Children {
            get { return _children.OrderBy(x => x.OrderById); }
        }

        public void AddChild(NodeBase node) {
            _children.Add(node);
        }

        public NodeBase() {
            _children = new List<NodeBase>();
        }
    }
}
