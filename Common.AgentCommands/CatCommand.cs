using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class CatCommandOptions : CommandOption
    {
        [Argument("path", "Path of the file to display", 0, IsRequired = true)]
        public string Path { get; set; }
    }

    [Command("cat", "Display the content of a file.", Category = AgentCommandCategories.System)]
    public class CatCommand : AgentCommand<CatCommandOptions>
    {
        public override CommandId CommandId => CommandId.Cat;

        protected override void SpecifyParameters(AgentCommandContext context, CatCommandOptions options)
        {
            context.AddParameter(ParameterId.Path, options.Path);
        }
    }
}
