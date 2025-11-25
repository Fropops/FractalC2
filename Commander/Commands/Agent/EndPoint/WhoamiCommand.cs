using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;

namespace Commander.Commands.Agent.EndPoint
{
    public class WhoamiCommand : EndPointCommand
    {
        public override string Description => "Get User and Hostname where agent is running on";
        public override string Name => "whoami";
        public override CommandId CommandId => CommandId.Whoami;
    }
}
