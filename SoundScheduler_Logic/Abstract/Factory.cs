﻿using System;
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
    }
}