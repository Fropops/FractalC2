using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Payload;
using Shared;

namespace Commander.Commands.Scripted
{
    public class ScriptingCommander<T>
    {
        private CommandContext<T> context;

        public ScriptingCommander(CommandContext<T> ctxt)
        {
            context = ctxt;
        }

        public void WriteError(string message)
        {
            this.context.Terminal.WriteError(message);
        }
        public void WriteSuccess(string message)
        {
            this.context.Terminal.WriteSuccess(message);
        }
        public void WriteLine(string message)
        {
            this.context.Terminal.WriteLine(message);
        }
        public void WriteInfo(string message)
        {
            this.context.Terminal.WriteInfo(message);
        }

        public byte[] GeneratePayload(PayloadGenerationOptions options, bool isVerbose)
        {
            return context.GeneratePayloadAndDisplay(options, isVerbose);
        }
    }
}
