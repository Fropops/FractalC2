using System;
using System.Threading.Tasks;
using BinarySerializer;
using Common.APIModels;
using Shared;

namespace TeamServer.FrameHandling;

public class SocksFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.Socks; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var packet = await this.ExtractFrameData<Socks4Packet>(frame);

        if (packet is null)
            return;

        var socks = Server.SocksService.GetClientById(frame.Source, packet.Id);

        if (socks is null)
            return;

        switch (packet.Type)
        {
            case Socks4Packet.PacketType.CONNECT:
                {
                    var connected = packet.Data.BinaryDeserializeAsync<bool>().Result;
                    socks.Unblock(connected);
                    break;
                }

            case Socks4Packet.PacketType.DATA:
                {
                    socks.QueueData(packet.Data);
                    break;
                }

            case Socks4Packet.PacketType.DISCONNECT:
                {
                    socks.Disconnect();
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}