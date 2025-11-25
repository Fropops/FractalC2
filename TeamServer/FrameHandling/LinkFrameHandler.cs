using System.Collections.Generic;
using System.Threading.Tasks;
using Common.APIModels;
using Shared;

namespace TeamServer.FrameHandling;

public class LinkFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.Link; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var link = await this.ExtractFrameData<LinkInfo>(frame);
        var parent = this.Server.AgentService.GetOrCreateAgent(link.ParentId);
        var child = this.Server.AgentService.GetOrCreateAgent(link.ChildId);
        if (!parent.Links.ContainsKey(child.Id))
        {
            parent.Links.Add(child.Id, link);
            this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, relay);
        }
    }
}

public class UnlinkFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.Unlink; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var link = await this.ExtractFrameData<Shared.LinkInfo>(frame);
        var parent = this.Server.AgentService.GetOrCreateAgent(link.ParentId);
        var child = this.Server.AgentService.GetOrCreateAgent(link.ChildId);
        if (parent.Links.ContainsKey(child.Id))
        {
            parent.Links.Remove(child.Id);
            this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, relay);
        }
    }
}

public class LinkRelayFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.LinkRelay; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var relayIds = await this.ExtractFrameData<List<string>>(frame);

        foreach (var relayedAgent in this.Server.AgentService.GetAgentToRelay(relay))
        {
            if (relayedAgent.Id == relay)
                continue;

            relayedAgent.RelayId = null;
        }

        foreach (var relayId in relayIds)
        {
            var relayedAgent = this.Server.AgentService.GetOrCreateAgent(relayId);
            relayedAgent.RelayId = relay;
        }
    }
}