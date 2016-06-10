using System.Linq;
using System.Management;

namespace Aims.BizTalk.Autohealer
{
    public class BizTalkHelper
    {
        public static void StartHostInstance(Node node)
        {
            var query = $"SELECT * FROM MSBTS_HostInstance WHERE HostName='{node.Name}' AND HostType=1";
            using (var searcher = new ManagementObjectSearcher(@"root\MicrosoftBizTalkServer", query))
            {
                var hi = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (hi != null && hi["ServiceState"].ToString() == "1")
                {
                    hi.InvokeMethod("Start", null);
                }
            }
        }
    }
}