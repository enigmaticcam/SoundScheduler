using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundScheduler_Logic.Views.Abstract;
using SoundScheduler_Logic.Views.Concrete;

namespace SoundScheduler_Logic.Abstract {    
    public class Factory {
        public Repository CreateRepository() {
            Repository repository = new Repository();
            return repository;
        }

        public IViewJobs CreateViewJobs() {
            ViewJobs view = new ViewJobs(this);
            return view;
        }

        public IViewUsers CreateViewUsers() {
            ViewUsers view = new ViewUsers(this);
            return view;
        }

        public IViewTemplates CreateViewTemplates() {
            ViewTemplates view = new ViewTemplates(this);
            return view;
        }

        public IViewMeetings CreateViewMeetings() {
            ViewMeetings view = new ViewMeetings(this);
            return view;
        }
    }
}
