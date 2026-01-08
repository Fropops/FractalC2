using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("run", "Run an executable, capturing output", Category = AgentCommandCategories.Execution)]
    public class RunCommand : AgentCommand<AgentShellCommandOption>
    {
        public override CommandId CommandId => CommandId.Run;

        protected override void SpecifyParameters(AgentCommandContext context, AgentShellCommandOption options)
        {
            if (!string.IsNullOrEmpty(options.RawArgs))
                context.AddParameter(ParameterId.Command, options.RawArgs);
        }
    }
}
