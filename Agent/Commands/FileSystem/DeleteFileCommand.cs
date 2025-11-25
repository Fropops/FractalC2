using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DeleteFileCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Del;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Path);
            var path = task.GetParameter<string>(ParameterId.Path);

            File.Delete(path);
            context.AppendResult($"File {path} removed");
        }
    }
}
