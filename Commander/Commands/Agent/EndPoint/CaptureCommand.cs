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
    public class CaptureCommand : EndPointCommand
    {
        public override string Description => "Capture current screen(s).";
        public override string Name => "capture";
        public override CommandId CommandId => CommandId.Capture;
        public override string Category => CommandCategory.Media;
    }
}
