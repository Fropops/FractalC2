using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.APIModels;
using Shared;

namespace TeamServer.FrameHandling;


public class LinksFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.Links; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var links = await this.ExtractFrameData<List<LinkInfo>>(frame);
        foreach (var link in links)
        {
            var parent = await this.Server.AgentService.GetOrCreateAgentAsync(link.ParentId);
            var child = await this.Server.AgentService.GetOrCreateAgentAsync(link.ChildId);
            if (!parent.Links.ContainsKey(child.Id))
            {
                parent.Links.Add(child.Id, link);
                this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, relay);
            }
        }
    }
}

public class LinkFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.Link; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var link = await this.ExtractFrameData<LinkInfo>(frame);
        var parent = await this.Server.AgentService.GetOrCreateAgentAsync(link.ParentId);
        var child = await this.Server.AgentService.GetOrCreateAgentAsync(link.ChildId);
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
        var parent = await this.Server.AgentService.GetOrCreateAgentAsync(link.ParentId);
        var child = await this.Server.AgentService.GetOrCreateAgentAsync(link.ChildId);
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

            if (!relayIds.Contains(relayedAgent.Id))
            {
                relayedAgent.RelayId = null;
                this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, relayedAgent.Id);
            }
        }

        foreach (var relayId in relayIds)
        {
            var relayedAgent = await this.Server.AgentService.GetOrCreateAgentAsync(relayId);
            if (relayedAgent.RelayId != relay && relayedAgent.Id != relay)
            {
                relayedAgent.RelayId = relay;
                this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, relayedAgent.Id);
            }
            this.Server.ChangeTrackingService.TrackChange(ChangingElement.Agent, relay);
        }
    }
}

