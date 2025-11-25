using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class AgentFileChunck
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public string Data { get; set; }
    }
}
