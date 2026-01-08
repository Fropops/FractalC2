using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class PSCommandOptions : CommandOption
    {
        [Argument("process", "process name to search for", 0)]
        public string Process { get; set; } = string.Empty;
    }

    [Command("ps", "List running processes", Category = AgentCommandCategories.System)]
    public class PsCommand : AgentCommand<PSCommandOptions>
    {
        public override CommandId CommandId => CommandId.ListProcess;

        protected override void SpecifyParameters(AgentCommandContext context, PSCommandOptions options)
        {
            if (!string.IsNullOrEmpty(options.Process))
                context.AddParameter(ParameterId.Path, options.Process);
        }
    }
}
