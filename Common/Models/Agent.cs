using System;
using System.Collections.Generic;
using System.Net;
using Shared;

namespace Common.Models
{
    public class Agent
    {
        public string Id { get; set; }
        public string RelayId { get; set; }
        public AgentMetadata Metadata { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstSeen { get; set; }
        public List<string> Links { get; set; } = new List<string>();
        public List<long> Pings { get; set; } = new List<long>();

        public TimeSpan LastSeenDelta => DateTime.UtcNow - LastSeen;
        
        public string Hostname => Metadata?.Hostname ?? "";
        public string Ip => Metadata?.Address != null ? new System.Net.IPAddress(Metadata.Address).ToString() : "";
        public string UserName => Metadata?.UserName ?? "";
        public string ProcessName => Metadata?.ProcessName ?? "";
        public int Pid => Metadata?.ProcessId ?? 0;
        public string Architecture => Metadata?.Architecture ?? "";
        public string Integrity => Metadata?.Integrity.ToString() ?? "";
    }
}
