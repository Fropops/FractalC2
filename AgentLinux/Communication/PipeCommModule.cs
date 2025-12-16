using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Agent.Communication
{
    internal class PipeCommModule : P2PCommunicator
    {
        public PipeCommModule(ConnexionUrl conn) : base(conn)
        {
                throw new NotImplementedException();
        }

        public override event Func<NetFrame, Task> FrameReceived;
        public override event Action OnException;

        public override Task Run()
        {
            throw new NotImplementedException();
        }

        public override Task SendFrame(NetFrame frame)
        {
            throw new NotImplementedException();
        }

        public override Task Start()
        {
            throw new NotImplementedException();
        }
    }
}
