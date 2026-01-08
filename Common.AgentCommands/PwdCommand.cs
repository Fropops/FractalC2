using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("pwd", "Display the current working directory.", Category = AgentCommandCategories.System)]
    public class PwdCommand : AgentCommand<CommandOption>
    {
        public override CommandId CommandId => CommandId.Pwd;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };
    }
}
