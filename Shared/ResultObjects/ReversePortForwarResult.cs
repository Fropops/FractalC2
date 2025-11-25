using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared.ResultObjects
{
    public class ReversePortForwarResult
    {
        [FieldOrder(0)]
        public int Port { get; set; }
        [FieldOrder(1)]
        public string DestHost { get; set; }
        [FieldOrder(2)]
        public int DestPort { get; set; }
    }
}
