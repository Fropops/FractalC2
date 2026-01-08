using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class MakeDirCommandOptions : CommandOption
    {
        [Argument("path", "Path of the folder to create", 0, IsRequired = true)]
        public string Path { get; set; }
    }

    [Command("mkdir", "Create a folder on the agent.", Category = AgentCommandCategories.System)]
    public class MakeDirCommand : AgentCommand<MakeDirCommandOptions>
    {
        public override CommandId CommandId => CommandId.MkDir;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };

        protected override void SpecifyParameters(AgentCommandContext context, MakeDirCommandOptions options)
        {
            context.AddParameter(ParameterId.Path, options.Path);
        }
    }
}
