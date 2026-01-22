using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class DestroyCommandOptions : CommandOption
    {
        [Option("f", "force", "Force the agent to exit (stop the process).")]
        public bool Force { get; set; } = false;
    }

    [Command("destroy", "Ask an agent to exit.", Category = AgentCommandCategories.System)]
    public class DestroyCommand : AgentCommand<DestroyCommandOptions>
    {
        public override CommandId CommandId => CommandId.Exit;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };

        protected override void SpecifyParameters(AgentCommandContext context, DestroyCommandOptions options)
        {
            if (options.Force)
                context.AddParameter(ParameterId.Command, options.Force);
        }
    }
}
