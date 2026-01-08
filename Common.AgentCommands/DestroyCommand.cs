using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("destroy", "Ask an agent to exit.", Category = AgentCommandCategories.System)]
    public class DestroyCommand : AgentCommand<CommandOption>
    {
        public override CommandId CommandId => CommandId.Exit;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };
    }
}
