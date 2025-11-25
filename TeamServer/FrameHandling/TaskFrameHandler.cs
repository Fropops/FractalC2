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

        var task = this.Server.TaskService.Get(taskOutput.Id);
        if(task != null && task.CommandId == CommandId.Download && taskOutput.Objects != null)
        {
            var file = await taskOutput.Objects.BinaryDeserializeAsync<DownloadFile>();
            taskOutput.Objects = null;
            this.Server.DownloadFileService.Add(file);
        }

        this.Server.TaskResultService.AddTaskResult(taskOutput);
        this.Server.ChangeTrackingService.TrackChange(ChangingElement.Result, taskOutput.Id);


    }
}