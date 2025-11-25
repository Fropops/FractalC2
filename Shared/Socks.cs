using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public sealed class Socks4ConnectRequest
    {
        [FieldOrder(0)]
        public string Id { get; set; }

        public int Version { get; set; }
        public CommandCode Command { get; set; }

        [FieldOrder(1)]
        public int DestinationPort { get; set; }

        [FieldOrder(2)]
        public byte[] DestinationAddress { get; set; }

        [FieldOrder(3)]
        public string DestinationDomain { get; set; }

        public Socks4ConnectRequest(byte[] data)
        {
            Version = Convert.ToInt32(data[0]);
            Command = (CommandCode)data[1];
            DestinationPort = data[3] | data[2] << 8;

            byte[] ip = new byte[4];
            Array.Copy(data, 4, ip, 0, 4);
            var address = new IPAddress(ip);
            DestinationAddress = address.GetAddressBytes();

            // if this is SOCKS4a
            if (address.ToString().StartsWith("0.0.0."))
            {
                byte[] dest = new byte[4];
                Array.Copy(data, 9, ip, 0, data.Length - 9);
                DestinationDomain = Encoding.UTF8.GetString(dest);
            }
        }

        public Socks4ConnectRequest()
        {
        }

        public enum CommandCode : byte
        {
            StreamConnection = 0x01,
            PortBinding = 0x02
        }
    }

    public sealed class Socks4Packet
    {
        [FieldOrder(1)]
        public string Id { get; set; }

        [FieldOrder(2)]
        public PacketType Type { get; set; }

        [FieldOrder(3)]
        public byte[] Data { get; set; }

        public Socks4Packet(string id, PacketType type, byte[] data = null)
        {
            Id = id;
            Type = type;
            Data = data;
        }

        public Socks4Packet()
        {

        }

        public enum PacketType
        {
            CONNECT,
            DATA,
            DISCONNECT
        }
    }

    public class Socks4Data
    {
        [FieldOrder(1)]
        public string Id { get; set; }

        [FieldOrder(2)]
        public byte[] Data { get; set; }
    }
}
