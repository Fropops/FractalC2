using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;

namespace Commander.Commands.Agent.EndPoint
{
    public class ExitCommand : EndPointCommand
    {
        public override string Description => "Ask an agent to exit.";
        public override string Name => "destroy";
        public override CommandId CommandId => CommandId.Exit;
    }
}
