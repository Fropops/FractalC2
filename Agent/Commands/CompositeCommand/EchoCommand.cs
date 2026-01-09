using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class EchoCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Echo;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Parameters);

            string message = task.GetParameter<string>(ParameterId.Parameters);

            context.AppendResult(message);
        }
    }
}
