using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Text;

namespace Aims.BizTalk.Autohealer
{
    public static class InstallerHelper
    {
        public static void AddCommandLineArgs(Installer installer, params string[] args)
        {
            var path = new StringBuilder(installer.Context.Parameters["assemblypath"]);
            if (path[0] != '"')
            {
                path.Insert(0, '"');
                path.Append('"');
            }
            foreach (string arg in args)
            {
                path.AppendFormat(" {0}", FormatCommandLineArgument(arg));
            }
            installer.Context.Parameters["assemblypath"] = path.ToString();
        }

        public static void ConfigureEventLogInstaller(InstallerCollection installers, string log, string source)
        {
            var eventLogInstaller = FindInstaller<EventLogInstaller>(installers);
            if (eventLogInstaller == null)
            {
                eventLogInstaller = new EventLogInstaller();
                installers.Add(eventLogInstaller);
            }

            eventLogInstaller.Source = source;
            eventLogInstaller.Log = log;
        }

        public static T FindInstaller<T>(InstallerCollection installers)
            where T : Installer
        {
            foreach (Installer inst in installers)
            {
                var eventLogInstaller = inst as T;
                if (eventLogInstaller != null)
                    return eventLogInstaller;

                eventLogInstaller = FindInstaller<T>(inst.Installers);
                if (eventLogInstaller != null)
                    return eventLogInstaller;
            }

            return null;
        }

        public static T[] FindInstallers<T>(InstallerCollection installers)
            where T : Installer
        {
            var result = new List<T>();
            foreach (Installer inst in installers)
            {
                var eventLogInstaller = inst as T;
                if (eventLogInstaller != null)
                {
                    result.Add(eventLogInstaller);
                }

                result.AddRange(FindInstallers<T>(inst.Installers));
            }

            return result.ToArray();
        }

        public static void Install(Installer installer)
        {
            SetContext(installer);
            installer.Install(new Hashtable());
        }

        public static void Uninstall(Installer installer)
        {
            SetContext(installer);
            installer.Uninstall(null);
        }

        private static string FormatCommandLineArgument(string s)
        {
            string arg = s.Trim().Trim('"');
            return arg.IndexOf(' ') == -1 ? arg : "\"" + arg + "\"";
        }

        private static void SetContext(Installer installer)
        {
            installer.Context = new InstallContext("",
                new[] { "/assemblypath=" + installer.GetType().Assembly.Location });
        }
    }
}