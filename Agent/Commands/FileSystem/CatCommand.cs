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
    public class CatCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Cat;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Path);
 
            string path = task.GetParameter<string>(ParameterId.Path);

            if (!File.Exists(path))
            {
                context.Error($"{path} not found");
                return;
            }

            string text = System.IO.File.ReadAllText(path);
            context.AppendResult(text);
        }


    }
}
