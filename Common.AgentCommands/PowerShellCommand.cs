using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("powershell", "Send a command to be executed by the agent powershell", Category = AgentCommandCategories.Execution, Aliases = new string[] { "powerpick" })]
    public class PowerShellCommand : AgentCommand<AgentShellCommandOption>
    {
        public override CommandId CommandId => CommandId.Powershell;

        protected override void SpecifyParameters(AgentCommandContext context, AgentShellCommandOption options)
        {
            if (!string.IsNullOrEmpty(options.RawArgs))
            {
                context.AddParameter(ParameterId.Command, options.RawArgs);
            }
        }
    }
}
