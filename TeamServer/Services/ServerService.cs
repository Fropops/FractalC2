using System;
using Shared;
using System.Collections.Generic;
using Mono.Cecil;
using BinarySerializer;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading.Tasks;
using System.Reflection;
using TeamServer.FrameHandling;
using System.Linq;
using TeamServer.Service;

namespace TeamServer.Services;

public interface IServerService
{
    IAgentService AgentService { get; }
    IFrameService FrameService { get; }
    IChangeTrackingService ChangeTrackingService { get; }
    ITaskResultService TaskResultService { get; }
    ITaskService TaskService { get; }

    IReversePortForwardService ReversePortForwardService { get; }
    ISocksService SocksService { get; }
    IDownloadFileService DownloadFileService { get; }
    Task HandleInboundFrames(IEnumerable<NetFrame> frames, string relay);
}

public class ServerService : IServerService
{
    public IAgentService AgentService { get; }
    public IFrameService FrameService { get; }
    public IChangeTrackingService ChangeTrackingService { get; }
    public ITaskResultService TaskResultService { get; }
    public ITaskService TaskService { get; }
    public IDownloadFileService DownloadFileService { get; }
    public ISocksService SocksService { get; }

    public IReversePortForwardService ReversePortForwardService { get; }

    public ServerService(IAgentService agentService,
        IFrameService frameService,
        ITaskResultService taskResultService,
        IChangeTrackingService changeTrackingService,
        ISocksService socksService,
        IReversePortForwardService reversePortForwardService,
        ITaskService taskService,
        IDownloadFileService downloadFileService)
    {
        this.AgentService = agentService;
        this.FrameService=frameService;
        this.TaskResultService= taskResultService;
        this.ChangeTrackingService = changeTrackingService;
        this.SocksService=socksService;
        this.ReversePortForwardService = reversePortForwardService;
        this.TaskService=taskService;
        this.DownloadFileService=downloadFileService;

        this.LoadModules();
    }

    public List<FrameHandler> _handlers = new List<FrameHandler>();

    public async Task HandleInboundFrames(IEnumerable<NetFrame> frames, string relay)
    {
        foreach (var frame in frames)
            await this.HandleInboundFrame(frame, relay);
    }

    public async Task HandleInboundFrame(NetFrame frame, string relay)
    {
        var handler = _handlers.First(m => m.FrameType == frame.FrameType);
        try
        {
            await handler.ProcessFrame(frame, relay);
        }
        catch(Exception ex)
        {
            Logger.Log($"Error While Handling Frame : {ex}");
        }
        
    }

    private void LoadModules()
    {
        var self = Assembly.GetExecutingAssembly();

        foreach (var type in self.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(FrameHandler)))
                continue;

            var handler = (FrameHandler)Activator.CreateInstance(type);

            if (handler is null)
                continue;

            handler.Init(this);
            _handlers.Add(handler);
        }
    }
}
