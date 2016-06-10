using System;
using System.ServiceProcess;
using System.Threading;

namespace Aims.BizTalk.Autohealer
{
    public partial class HealerService : ServiceBase
    {
        private static bool _isRunning = true;

        public HealerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ThreadPool.QueueUserWorkItem(Iteration);
        }

        protected override void OnStop()
        {
            _isRunning = false;
        }

        private static void Iteration(object state)
        {
            var tracker = new NodeTracker();

            while (_isRunning)
            {
                try
                {
                    foreach (var node in tracker.CheckNodes())
                    {
                        BizTalkHelper.StartHostInstance(node);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
