using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class AgentTask
    {
        public string Label { get; set; }
        public string Id { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public DateTime RequestDate { get; set; }
        public string FileId { get; set; }  
        public string FileName { get; set; }
    }
}
