using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("start", "Start an executable, without capturing output", Category = AgentCommandCategories.System)]
    public class StartCommand : AgentCommand<AgentShellCommandOption>
    {
        public override CommandId CommandId => CommandId.Start;

        protected override void SpecifyParameters(AgentCommandContext context, AgentShellCommandOption options)
        {
            if (!string.IsNullOrEmpty(options.RawArgs))
                context.AddParameter(ParameterId.Command, options.RawArgs);
        }
    }
}
