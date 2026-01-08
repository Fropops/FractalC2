using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class RemoveDirCommandOptions : CommandOption
    {
        [Argument("path", "Path of the folder to delete", 0, IsRequired = true)]
        public string Path { get; set; }
    }

    [Command("rmdir", "Delete a folder on the agent.", Category = AgentCommandCategories.System)]
    public class RemoveDirCommand : AgentCommand<RemoveDirCommandOptions>
    {
        public override CommandId CommandId => CommandId.RmDir;

        protected override void SpecifyParameters(AgentCommandContext context, RemoveDirCommandOptions options)
        {
            context.AddParameter(ParameterId.Path, options.Path);
        }
    }
}
