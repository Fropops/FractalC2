using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    [Command("capture", "Capture current screen(s).", Category = AgentCommandCategories.Media)]
    public class CaptureCommand : AgentCommand<CommandOption>
    {
        public override CommandId CommandId => CommandId.Capture;
    }
}
