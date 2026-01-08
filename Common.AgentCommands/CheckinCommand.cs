using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("checkin", "Force agent to update its metadata.", Category = AgentCommandCategories.System)]
    public class CheckinCommand : AgentCommand<CommandOption>
    {
        public override CommandId CommandId => CommandId.CheckIn;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };
    }
}
