using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Abstract;

namespace SoundScheduler_Logic.Views.Abstract {
    public interface IViewJobs {
        List<Job> Jobs { get; }
        void LoadFromSource();
        void SaveToSource();
    }

    public class ViewJobDecorator : IViewJobs {
        private IViewJobs _view;

        public virtual List<Job> Jobs {
            get { return _view.Jobs; }
        }

        public virtual void LoadFromSource() {
            _view.LoadFromSource();
        }

        public virtual void SaveToSource() {
            _view.SaveToSource();
        }

        public ViewJobDecorator(IViewJobs view) {
            _view = view;
        }
    }
}
