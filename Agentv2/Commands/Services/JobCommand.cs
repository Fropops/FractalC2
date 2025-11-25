using Agent.Commands.Services;
using Agent.Communication;
using Agent.Models;
using Agent.Service;
using BinarySerializer;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;

namespace Agent.Commands
{
    internal class JobCommand : ServiceCommand<IJobService>
    {
        public override CommandId Command => CommandId.Job;

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Kill, this.Kill);
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            var jobs = this.Service.GetJobs();
            if (!jobs.Any())
            {
                context.AppendResult("No jobs running!");
                return;
            }

            context.Objects(jobs);
            return;
        }

        protected async Task Kill(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Id);

            int id = task.GetParameter<int>(ParameterId.Id);
            var job = this.Service.GetJob(id);
            if(job == null)
            {
                context.Error($"Job {id} not found!");
                return;
            }

            if (job.ProcessId.HasValue)
            {

                var cmd = $@"c:\windows\system32\cmd.exe /c taskkill /F /T /PID {job.ProcessId}";

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

                if (creationParms.RedirectOutput)
                    APIWrapper.ReadPipeToEnd(procResult.OutPipeHandle, output => context.AppendResult(output, false));
            }

            if(job.CancellationToken != null)
                job.CancellationToken.Cancel();

            return;
        }

        
    }
}
