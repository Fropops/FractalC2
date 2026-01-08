using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("revert-self", "Remove Token Impersonation", Category = AgentCommandCategories.Token)]
    public class Rev2SelfCommand : AgentCommand<CommandOption>
    {
        public override CommandId CommandId => CommandId.RevertSelf;
    }
}
