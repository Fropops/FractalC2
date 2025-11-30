using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer;
using Common.APIModels;
using Shared;

namespace TeamServer.FrameHandling;

public class TaskFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.TaskResult; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var taskOutput = await this.ExtractFrameData<AgentTaskResult>(frame);
        var agent = this.Server.TaskResultService.GetAgentTaskResult(taskOutput.Id);

        var task = this.Server.TaskService.Get(taskOutput.Id);
        if(task != null && task.CommandId == CommandId.Download && taskOutput.Objects != null)
        {
            var file = await taskOutput.Objects.BinaryDeserializeAsync<DownloadFile>();
            taskOutput.Objects = null;
            await this.Server.LootService.AddFileAsync(agent.Id, file.FileName, file.Data);
        }

        if (task != null && task.CommandId == CommandId.Capture && taskOutput.Objects != null)
        {
            var files = await taskOutput.Objects.BinaryDeserializeAsync<List<DownloadFile>>();
            taskOutput.Objects = null;
            foreach(var file in files)
            {
                await this.Server.LootService.AddFileAsync(agent.Id, file.FileName, file.Data);
            }
        }

        this.Server.TaskResultService.AddTaskResult(taskOutput);
        this.Server.ChangeTrackingService.TrackChange(ChangingElement.Result, taskOutput.Id);


    }
}