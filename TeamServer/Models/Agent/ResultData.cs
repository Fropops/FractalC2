using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class ResultData
    {
        public AgentMetadata Metadata { get; set; }
        public List<AgentTaskResult> Results { get; set; } = new List<AgentTaskResult>();
    }
}
