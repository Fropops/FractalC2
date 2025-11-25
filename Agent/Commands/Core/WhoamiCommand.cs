using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class WhoamiCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Whoami;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var identity = WindowsIdentity.GetCurrent();
            context.AppendResult(identity.Name);
        }
    }
}
