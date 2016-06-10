using System;
using System.Linq;
using System.Management;

namespace Aims.BizTalk.Autohealer
{
    public class BizTalkHelper
    {
        public static void StartHostInstance(Node node)
        {
            string name = node.Name.ToLowerInvariant()
                .Replace(String.Format(" ({0})", Environment.MachineName.ToLowerInvariant()), String.Empty);
            string query = String.Format("SELECT * FROM MSBTS_HostInstance WHERE HostName='{0}' AND HostType=1", name);
            using (var searcher = new ManagementObjectSearcher(@"root\MicrosoftBizTalkServer", query))
            {
                ManagementObject hi = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (hi != null && hi["ServiceState"].ToString() == "1")
                {
                    hi.InvokeMethod("Start", null);
                }
            }
        }
    }
}