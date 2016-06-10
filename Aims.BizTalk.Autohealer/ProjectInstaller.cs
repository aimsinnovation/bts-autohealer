using System.ComponentModel;
using System.Configuration.Install;

namespace Aims.BizTalk.Autohealer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}