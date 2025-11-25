using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public class NetFrame
    {
        public NetFrame()
        {

        }

        public NetFrame(string source, string destination, NetFrameType typ, byte[] data)
        {
            this.Source = source;
            this.Destination = destination;
            this.FrameType = typ;
            this.Data = data;
        }

        public NetFrame(string source, string destination, NetFrameType typ) : this(source, destination, typ, null)
        {
        }

        public NetFrame(string source, NetFrameType typ) : this(source, null, typ, null)
        {
        }

        public NetFrame(string source, NetFrameType typ, byte[] data) : this(source, String.Empty, typ, data)
        {
        }

        [FieldOrder(0)]
        public NetFrameType FrameType { get; set; }
        [FieldOrder(1)]
        public string Source { get; set; } = String.Empty;
        [FieldOrder(2)]
        public string Destination { get; set; } = String.Empty;
        [FieldOrder(3)]
        public byte[] Data { get; set; }
    }
}
