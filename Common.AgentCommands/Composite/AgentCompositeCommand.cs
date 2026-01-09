using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Command;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands.Composite
{
    public abstract class AgentCompositeCommand<TOption> : AgentCommand<TOption> where TOption : CommandOption, new()
    {
        public override CommandId CommandId => CommandId.Composite;

        protected abstract Task<bool> Run(AgentCommandContext context, TOption option );

        public override async Task<bool> Execute(AgentCommandContext context, TOption options)
        {
            var result = await this.Run(context, options);

            if (!result)
                return false;

            context.TaskAgent(options.CommandLine, this.CommandId, context.GetParameters());
            return true;
        }

    }
}
