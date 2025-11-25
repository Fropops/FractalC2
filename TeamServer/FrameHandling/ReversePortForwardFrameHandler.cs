using System;
using System.Threading.Tasks;
using BinarySerializer;
using Common.APIModels;
using Shared;

namespace TeamServer.FrameHandling;

public class ReversePortForwardFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.ReversePortForward; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var packet = await this.ExtractFrameData<ReversePortForwardPacket>(frame);

        if (packet is null)
            return;

        switch (packet.Type)
        {
            case ReversePortForwardPacket.PacketType.CONNECT:
                {
                    var destination = await packet.Data.BinaryDeserializeAsync<ReversePortForwardDestination>();
                    await Server.ReversePortForwardService.StartClient(packet.Id, frame.Source, destination);
                    break;
                }

            case ReversePortForwardPacket.PacketType.DATA:
                {
                    var client = Server.ReversePortForwardService.GetClientById(packet.Id);
                    if (client == null)
                        return;

                    client.QueueData(packet.Data);
                    break;
                }

            case ReversePortForwardPacket.PacketType.DISCONNECT:
                {
                    await Server.ReversePortForwardService.StopClient(packet.Id);
                }break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}