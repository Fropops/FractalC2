using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class DelCommandOptions : CommandOption
    {
        [Argument("path", "Path of the file to delete", 0, IsRequired = true)]
        public string Path { get; set; }
    }

    [Command("del", "Delete a file on the agent.", Category = AgentCommandCategories.System)]
    public class DelCommand : AgentCommand<DelCommandOptions>
    {
        public override CommandId CommandId => CommandId.Del;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };

        protected override void SpecifyParameters(AgentCommandContext context, DelCommandOptions options)
        {
            context.AddParameter(ParameterId.Path, options.Path);
        }
    }
}
