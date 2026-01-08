using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class AgentLsCommandOptions : CommandOption
    {
        [Argument("path", "Path of the directory to list", 0, IsRequired = true)]
        public string Path { get; set; } = string.Empty;
    }

    [Command("ls", "List the content of the directory.", Category = AgentCommandCategories.System, Aliases = new string[] { "dir" })]
    public class LsCommand : AgentCommand<AgentLsCommandOptions>
    {
        public override CommandId CommandId => CommandId.Ls;

        protected override void SpecifyParameters(AgentCommandContext context, AgentLsCommandOptions options)
        {
            if (!string.IsNullOrEmpty(options.Path))
                context.AddParameter(ParameterId.Path, options.Path);
        }
    }
}
