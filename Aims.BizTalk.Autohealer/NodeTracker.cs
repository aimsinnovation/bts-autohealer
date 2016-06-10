using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Aims.BizTalk.Autohealer
{
    public class NodeTracker
    {
        private long[] _trackedNodeIds;
        private string _email;
        private string _password;
        private string _environment;

        private Dictionary<string, string> _nodeStatuses = null;
        private Dictionary<string, string> _nodeTypes = null;

        public NodeTracker()
        {
            if(!File.Exists("aims-autohealer.conf"))
                throw new Exception("Config file 'aims-autohealer.conf' is not found.");

            Dictionary<string, string> pp = new Dictionary<string, string>();
            using (var configFile = File.OpenText("aims-autohealer.conf"))
            {
                string s;
                while (!String.IsNullOrEmpty(s = configFile.ReadLine()))
                {
                    string[] parts = s.Split(new[] {'='}, 2);
                    pp[parts[0].ToUpperInvariant()] = parts[1];
                }
            }

            var setter = new Dictionary<string, Action<string>>
            {
                {"EMAIL", v => _email = v},
                {"PASSWORD", v => _password = v},
                {"ENVIRONMENT", v => _environment = v},
                {
                    "TRACKED_NODE_IDS", v => _trackedNodeIds = v.ToUpperInvariant() == "ALL"
                        ? null
                        : v.Split(',').Select(Int64.Parse).ToArray()
                },
            };

            foreach (var kvp in setter)
            {
                if (!pp.ContainsKey(kvp.Key))
                    throw new Exception($"{kvp.Key} is not presented in config file.");

                kvp.Value(pp[kvp.Key]);
            }
            _environment += _environment.Last() == '/' ? String.Empty : "/";
        }

        private T Get<T>(string path)
        {
            using (WebClient client = GetWebClient())
            {
                string response = client.DownloadString(_environment + path);
                return JsonConvert.DeserializeObject<T>(response);
            }
        }

        public Node[] CheckNodes()
        {
            if (_nodeStatuses == null)
            {
                _nodeStatuses = Get<Entity[]>("nodeStatuses")
                    .ToDictionary(s => s.Id, s => s.Name);
            }

            if (_nodeTypes == null)
            {
                _nodeTypes = Get<Entity[]>("nodeTypes")
                    .ToDictionary(s => s.Id, s => s.Name);
            }

            var nodes = Get<Node[]>("nodes")
                .Where(n => _trackedNodeIds == null || _trackedNodeIds.Contains(n.Id))
                .ToArray();

            foreach (var node in nodes)
            {
                node.Status = _nodeStatuses.ContainsKey(node.Status) ? _nodeStatuses[node.Status] : "unknown";
                node.Type = _nodeTypes.ContainsKey(node.Type) ? _nodeTypes[node.Type] : "unknown";
            }

            return nodes
                .Where(n => n.Type == "aims.bts.host-instance" && n.Status != "aims.core.started")
                .ToArray();
        }

        private WebClient GetWebClient()
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_email}:{_password}"));

            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Basic {token}");
            return webClient;
        }

        private class Entity
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}