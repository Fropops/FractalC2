using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public enum AgentResultStatus : byte
    {
        Queued = 0x00,
        Running = 0x01,
        Completed = 0x02,
        Error = 0x03
    }

    public class AgentTaskResult
    {
        [FieldOrder(0)]
        public string Id { get; set; }
        [FieldOrder(1)]
        public string Output { get; set; }
        [FieldOrder(2)]
        public byte[] Objects { get; set; }
        [FieldOrder(3)]
        public string Error { get; set; }
        [FieldOrder(4)]
        public string Info { get; set; }
        [FieldOrder(5)]
        public AgentResultStatus Status { get; set; }
    }
}
