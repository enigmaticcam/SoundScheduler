using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundScheduler_Logic.Abstract;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_Win {
    public partial class frmJobConsiderationTester : Form {
        private Random _random = new Random();
        private int[] _slots;

        public frmJobConsiderationTester() {
            InitializeComponent();
        }

        private void cmdGo_Click(object sender, EventArgs e) {
            BeginTesting(GetConsideration());
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
            //_slots = new int[users.Count * ]
            return new JobConsiderationVariety.Builder()
                .SetJobs(jobs)
                .SetTemplates(templates)
                .SetUserExceptions(new UserExceptionDictionary())
                .SetUsers(users)
                .Build();
        }

        private void BeginTesting(JobConsideration consideration) {
            consideration.Begin();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int count = 1; count <= 1000000; count++) {
                //consideration.IsValid();
            }
        }

        //private int[]
    }
}
