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
    public class CheckinCommand : EndPointCommand
    {
        public override string Description => "Force agent to update its metadata.";
        public override string Name => "checkin";
        public override CommandId CommandId => CommandId.CheckIn;
    }
}
