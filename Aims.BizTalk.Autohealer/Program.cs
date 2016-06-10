using System.ServiceProcess;

namespace Aims.BizTalk.Autohealer
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                switch (args[0])
                {
                    case "/install":
                        using (var installer = new ProjectInstaller())
                        {
                            InstallerHelper.Install(installer);
                        }
                        return;

                    case "/uninstall":
                        using (var installer = new ProjectInstaller())
                        {
                            InstallerHelper.Uninstall(installer);
                        }
                        return;
                }
            }

            ServiceBase.Run(new ServiceBase[]
            {
                new HealerService()
            });
        }
    }
}
