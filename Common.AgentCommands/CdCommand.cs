using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class AgentCdCommandOptions : CommandOption
    {
        [Argument("path", "Path of the directory to list", 0, IsRequired = true, IsRemainder = true)]
        public string Path { get; set; } = string.Empty;
    }

    [Command("cd", "List the content of the directory.", Category = AgentCommandCategories.System)]
    public class CdCommand : AgentCommand<AgentCdCommandOptions>
    {
        public override CommandId CommandId => CommandId.Cd;

        protected override void SpecifyParameters(AgentCommandContext context, AgentCdCommandOptions options)
        {
            context.AddParameter(ParameterId.Path, options.Path);
        }
    }
}
