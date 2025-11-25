using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Shared;
using BinarySerializer;

namespace Commander.Commands.Agent
{
    public abstract class EndPointCommand<T> : EnhancedCommand<T>
    {
        public override string Category => CommandCategory.Core;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override RootCommand Command => new RootCommand(this.Description);

        public abstract CommandId CommandId { get; }

        protected virtual async Task CallEndPointCommand(CommandContext<T> context)
        {
            await context.CommModule.TaskAgent(context.CommandLabel, context.Executor.CurrentAgent.Id, this.CommandId, context.Parameters);
            context.WriteTaskSendToAgent(this);
        }

        protected virtual async Task<bool> CheckParams(CommandContext<T> context)
        {
            return true;
        }

      
        protected virtual void SpecifyParameters(CommandContext<T> context)
        {
        }

        protected virtual async Task<bool> Body(CommandContext<T> context)
        {
            return true;
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            if (!await this.CheckParams(context))
                return false;
            if (!await this.Body(context))
                return false;
            this.SpecifyParameters(context);
            await this.CallEndPointCommand(context);
            return true;
        }
    }

    public abstract class EndPointCommand : EndPointCommand<EmptyCommandOptions>
    {
    }
   
}
