using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class AgentShellCommandOption : CommandOption
    {
        [Argument("Args", "Passthrough Arguments", 0, IsRemainder = true, IsRequired = true)]
        public string RawArgs { get; set; }
    }

    [Command("shell", "Send a command to be executed by the agent", Category = AgentCommandCategories.Execution)]
    public class ShellCommand : AgentCommand<AgentShellCommandOption>
    {
        public override CommandId CommandId => CommandId.Shell;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };

        protected override void SpecifyParameters(AgentCommandContext context, AgentShellCommandOption options)
        {
            if(!string.IsNullOrEmpty(options.RawArgs))
            {
                context.AddParameter(ParameterId.Command, options.RawArgs);
            }
        }
    }
}
