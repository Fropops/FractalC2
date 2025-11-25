using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public sealed class ReversePortForwardPacket
    {
        [FieldOrder(0)]
        public string Id { get; set; }

        [FieldOrder(1)]
        public PacketType Type { get; set; }

        [FieldOrder(2)]
        public byte[] Data { get; set; }

        [FieldOrder(3)]
        public int Port { get; set; }

        public enum PacketType
        {
            CONNECT,
            DATA,
            DISCONNECT
        }

        public ReversePortForwardPacket()
        {
            
        }

        public ReversePortForwardPacket(string id, PacketType type, byte[] data = null)
        {
            this.Id = id;
            this.Type = type;
            this.Data = data;
        }

  
    }

    public sealed class ReversePortForwardDestination
    {
        [FieldOrder(0)]
        public string Hostname { get; set; }

        [FieldOrder(1)]
        public int Port { get; set; }
    }
}
