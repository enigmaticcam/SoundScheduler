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
        IEnumerable<MeetingNode> Nodes { get; }
        string NodeDescriptionMeeting(Meeting meeting);
        string NodeDescriptionJob(Meeting meeting, Job job);
        void AddMeeting(Template template, DateTime date);
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

        private string NodeDescriptionMeeting(Meeting meeting) {
            return _view.NodeDescriptionMeeting(meeting);
        }

        private string NodeDescriptionJob(Meeting meeting, Job job) {
            return _view.NodeDescriptionJob(meeting, job);
        }

        public void AddMeeting(Template template, DateTime date) {
            _view.AddMeeting(template, date);
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

    public class MeetingNode {
        public string Name { get; set; }
        public int OrderById { get; set; }
        public IEnumerable<MeetingNode> Children { get; set; }
    }
}
