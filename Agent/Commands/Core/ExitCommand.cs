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
    public class ExitCommand: AgentCommand
    {
        public override CommandId Command => CommandId.Exit;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            context.AppendResult("Bye !");
            context.Agent.AskToStop();
        }
    }
}
