using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Commander.Models
{
    public class Agent
    {
        public string Id { get; set; }

        public string RelayId { get; set; }

        public AgentMetadata Metadata { get; set;  }

        public DateTime LastSeen { get; set; }

        public DateTime FirstSeen { get; set; }

        public List<string> Links { get; set; } = new List<string>();

        

        public TimeSpan LastSeenDelta
        {
            get
            {
                if (this.Metadata == null)
                    return new TimeSpan();

                TimeSpan delta = DateTime.UtcNow - this.LastSeen;
                return delta;
            }
        }

    }
}
