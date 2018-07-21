using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace SoundScheduler_Win {
    public partial class frmJobConsiderationTester : Form {
        private Random _random = new Random();
        private int[] _slots;

        public frmJobConsiderationTester() {
            InitializeComponent();
        }

        private void cmdGo_Click(object sender, EventArgs e) {
            BeginTesting(GetConsideration(), Convert.ToInt32(txtCount.Text));
        }

        private JobConsideration GetConsideration() {
            List<User> users = new List<User>();
            List<Job> jobs = new List<Job>();
            List<Template> templates = new List<Template>();
            for (int count = 1; count <= 10; count++) {
                users.Add(new User());
                jobs.Add(new Job());
                templates.Add(new Template());
            }
            _slots = new int[users.Count * templates.Count];
            templates.ForEach(x => x.Jobs = jobs);
            return new JobConsiderationVariety.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUserExceptions(new UserExceptionDictionary())
                .SetUsers(users)
                .Build();
        }

        private void BeginTesting(JobConsideration consideration, int countMax) {
            consideration.Begin();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int count = 1; count <= countMax; count++) {
                consideration.IsValid(_slots);
            }
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            lblResults.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }
    }
}
