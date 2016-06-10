using System;
using System.Diagnostics;
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
            try
            {
                var tracker = new NodeTracker();

                while (_isRunning)
                {
                    try
                    {
                        foreach (Node node in tracker.CheckNodes())
                        {
                            BizTalkHelper.StartHostInstance(node);
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(Constants.ServiceName, ex.ToString(), EventLogEntryType.Error);
                    }

                    Thread.Sleep(30 * 1000);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(Constants.ServiceName, ex.ToString(), EventLogEntryType.Error);
            }
        }
    }
}