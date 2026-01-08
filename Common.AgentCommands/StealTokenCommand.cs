using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class StealTokenCommandOptions : CommandOption
    {
        [Argument("pid", "Id of the process", 0)]
        public int Pid { get; set; }
    }

    [Command("steal-token", "Steal the token from a process", Category = AgentCommandCategories.Token)]
    public class StealTokenCommand : AgentCommand<StealTokenCommandOptions>
    {
        public override CommandId CommandId => CommandId.StealToken;

        protected override void SpecifyParameters(AgentCommandContext context, StealTokenCommandOptions options)
        {
            context.AddParameter(ParameterId.Id, options.Pid);
        }
    }
}
