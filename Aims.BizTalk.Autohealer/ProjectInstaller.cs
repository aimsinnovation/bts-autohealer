using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Aims.BizTalk.Autohealer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            const string eventSource = Constants.ServiceName;

            if (EventLog.SourceExists(eventSource))
            {
                EventLog.DeleteEventSource(eventSource);
            }

            InstallerHelper.ConfigureEventLogInstaller(Installers, "Application", eventSource);
        }
    }
}