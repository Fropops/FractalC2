using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Models;
using Shared;

namespace TeamServer.Models
{
    public class Agent
    {
        public string Id { get; protected set; }
        public string RelayId { get; set; }

        public Dictionary<string, LinkInfo> Links { get; protected set; } = new Dictionary<string, LinkInfo>();
        public Shared.AgentMetadata Metadata { get; set; }

        public DateTime LastSeen { get; set; }

        public DateTime FirstSeen { get; set; }

        public string ListenerId { get; set; }

        public bool CheckInrequested { get; set; } = false;

        public Agent(string id)
        {
            this.Metadata = null;
            this.Id = id;
            this.FirstSeen = DateTime.UtcNow;
            this.CheckInrequested = false;
        }
    }
}
