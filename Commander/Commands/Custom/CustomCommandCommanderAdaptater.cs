using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.Models;
using Common.Payload;
using Shared;

namespace Commander.Commands.Custom
{
    public class CustomCommandCommanderAdaptater<T> : ICommandCommander
    {
        private CommandContext<T> context;

        public CustomCommandCommanderAdaptater(CommandContext<T> ctxt)
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

        public Implant GeneratePayload(ImplantConfig options)
        {
            return context.GeneratePayloadAndDisplay(options);
        }



        public void CallEndPointCommand(string commandName, CommandId commandId)
        {
            context.CommModule.TaskAgent(context.CommandLabel, context.Executor.CurrentAgent.Id, commandId, context.Parameters).Wait();
            context.Terminal.WriteSuccess($"Command {commandName} tasked to agent {context.Executor.CurrentAgent?.Metadata?.Name}.");
        }
    }
}
