using System.Threading.Tasks;
using Common.APIModels;
using Shared;
using TeamServer.Models;

namespace TeamServer.FrameHandling;

public class CheckinFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.CheckIn; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var metaData = await this.ExtractFrameData<AgentMetadata>(frame);
        var ag = this.Server.AgentService.GetOrCreateAgent(frame.Source);
        if (ag.Id != relay)
        {
            ag.RelayId = relay;
            this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, ag.Id);
        }

        this.Server.AgentService.Checkin(ag, metaData);
        this.Server.ChangeTrackingService.TrackChange(ChangingElement.Metadata, metaData.Id);
    }
}