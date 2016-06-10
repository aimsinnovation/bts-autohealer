using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Aims.BizTalk.Autohealer
{
    public class NodeTracker
    {
        private string _environment;
        private string _token;
        private long[] _trackedNodeIds;

        public NodeTracker()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aims-autohealer.conf");
            if (!File.Exists(path))
                throw new Exception(String.Format("Config file '{0}' is not found.", path));

            var pp = new Dictionary<string, string>();
            using (StreamReader configFile = File.OpenText(path))
            {
                string s;
                while (!String.IsNullOrEmpty(s = configFile.ReadLine()))
                {
                    string[] parts = s.Split(new[] { '=' }, 2);
                    pp[parts[0].ToUpperInvariant()] = parts[1];
                }
            }

            var setter = new Dictionary<string, Action<string>>
            {
                { "TOKEN", v => _token = v },
                { "ENVIRONMENT", v => _environment = v },
                {
                    "TRACKED_NODE_IDS", v => _trackedNodeIds = v.ToUpperInvariant() == "ALL"
                        ? null
                        : v.Split(',').Select(Int64.Parse).ToArray()
                },
            };

            foreach (var kvp in setter)
            {
                if (!pp.ContainsKey(kvp.Key))
                    throw new Exception(String.Format("{0} is not present in the config file.", kvp.Key));

                kvp.Value(pp[kvp.Key]);
            }
            _environment += _environment.Last() == '/' ? String.Empty : "/";
        }

        public Node[] CheckNodes()
        {
            return Get<Node[]>("nodes")
                .Where(n => _trackedNodeIds == null || _trackedNodeIds.Contains(n.Id))
                .Where(n => n.Type == Constants.NodeType.HostInstance
                    && n.Status != Constants.NodeStatus.Running && n.Status != Constants.NodeStatus.Enabled)
                .ToArray();
        }

        private T Get<T>(string path)
        {
            using (WebClient client = GetWebClient())
            {
                string response = client.DownloadString(_environment + path);
                return JsonConvert.DeserializeObject<T>(response);
            }
        }

        private WebClient GetWebClient()
        {
            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.Authorization, String.Format("Basic {0}", _token));
            return webClient;
        }
    }
}