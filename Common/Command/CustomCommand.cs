using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Common.Command
{
    public abstract class CustomCommand<T>
    {
        public virtual string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public virtual string[] Alternate { get; protected set; }
        public virtual Shared.OsType[] SupportedOs { get; protected set; } = null;

        public async Task<bool> Execute(CommandExecutionContext<T> context)
        {
            var agent = context.Agent;
            var commander = context.Commander;
            var result = await this.Run(context);

            if (!result)
                return false;

            agent.AddParameter(ParameterId.Parameters, agent.GetTasks());
            commander.CallEndPointCommand(this.Name, CommandId.Script);
            return result;
        }

        protected abstract Task<bool> Run(CommandExecutionContext<T> context);
    }
}
