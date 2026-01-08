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
    [Command("whoami", "Get User and Hostname where agent is running on", Category = AgentCommandCategories.System, Aliases = new string[] { "who" })]
    public class WhoamiCommand : AgentCommand<CommandOption>
    {
        public override CommandId CommandId => CommandId.Whoami;
    }
}
