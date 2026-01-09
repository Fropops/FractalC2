using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public static class AgentCommandCategories
    {
        public const string System = "Agent - System";
        public const string Token = "Agent - Token";
        public const string Media = "Agent - Media";
        public const string LateralMovement = "Agent - LateralMovement";
        public const string Execution = "Agent - Execution";
        public const string Composite = "Agent - Composite";
        public const string Network = "Agent - Network";
    }

    public abstract class AgentCommandBase
    {
        public abstract CommandId CommandId { get; }

        public virtual Shared.OsType[] SupportedOs { get; protected set; } = new Shared.OsType[] { OsType.Windows };
    }

    public abstract class AgentCommand<TOption> : AgentCommandBase, ICommand<AgentCommandContext, TOption>
        where TOption : CommandOption
    {
        

        public virtual async Task<bool> Execute(AgentCommandContext context, TOption options)
        {
            this.CallEndPointCommand(context, options);
            return true;
        }

        protected void CallEndPointCommand(AgentCommandContext context, TOption options)
        {
            if (!this.CheckParams(context, options))
                return;
            this.SpecifyParameters(context, options);
            context.TaskAgent(options.CommandLine, this.CommandId);
        }

        protected virtual void SpecifyParameters(AgentCommandContext context, TOption options)
        {
        }

        protected virtual bool CheckParams(AgentCommandContext context, TOption options)
        {
            return true;
        }
    }
}
