using Agent.Commands;
using Agent.Helpers;
using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Commands
{
    public class RevertToSelfCommand : AgentCommand
    {
        public override CommandId Command => CommandId.RevertSelf;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            if (context.Agent.ImpersonationToken == IntPtr.Zero)
            {
                context.Error($"No impersonation to revert");
                return;
            }

            context.Agent.ImpersonationToken = IntPtr.Zero;
            context.AppendResult($"Reverted to self");
        }
    }
}
