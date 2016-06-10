using System;
using System.IO;
using System.ServiceProcess;
using System.Text;

namespace Aims.BizTalk.Autohealer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            switch (args.Length > 0 ? args[0] : null)
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

                case "/config":
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aims-autohealer.conf");
                    string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Join(":", args[2], args[3])));
                    using (StreamWriter file = File.CreateText(path))
                    {
                        file.WriteLine("ENVIRONMENT=" + args[1]);
                        file.WriteLine("TOKEN=" + token);
                        file.WriteLine("TRACKED_NODE_IDS=ALL");
                    }
                    return;

                default:
                    ServiceBase.Run(new HealerService());
                    return;
            }
        }
    }
}