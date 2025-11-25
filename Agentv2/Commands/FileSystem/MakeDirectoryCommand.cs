using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class MakeDirectoryCommand : AgentCommand
    {
        public override CommandId Command => CommandId.MkDir;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Path);
            var path = task.GetParameter<string>(ParameterId.Path);


            var dirInfo = Directory.CreateDirectory(path);
            context.AppendResult($"Folder {dirInfo.FullName} created");
        }
    }

}
