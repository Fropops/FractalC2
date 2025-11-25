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
    public class PwdCommand : EndPointCommand
    {
        public override string Description => "Display the current working directory.";
        public override string Name => "pwd";
        public override CommandId CommandId => CommandId.Pwd;
    }
}
