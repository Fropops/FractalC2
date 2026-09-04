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
        var ag = await this.Server.AgentService.GetOrCreateAgentAsync(frame.Source);
        if (ag.Id != relay)
        {
            ag.RelayId = relay;
            this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, ag.Id);
        }

        await this.Server.AgentService.CheckinAsync(ag, metaData);
        this.Server.ChangeTrackingService.TrackChange(ChangingElement.Metadata, metaData.Id);
    }
}