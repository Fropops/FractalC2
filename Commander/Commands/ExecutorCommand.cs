using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public abstract class ExecutorCommand
    {
        public virtual string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public virtual string Category { get; protected set; } = "Others";

        public virtual string[] Alternate { get; protected set; }

        public abstract ExecutorMode AvaliableIn { get; }

        public virtual Shared.OsType[] SupportedOs { get; protected set; } = null;

        public virtual void Execute(string parms)
        {
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();

            if (this.AvaliableIn == ExecutorMode.AgentInteraction && this.SupportedOs != null && this.SupportedOs.Any())
            {
                if (executor.CurrentAgent != null && executor.CurrentAgent.Metadata != null)
                {
                    if (!this.SupportedOs.Contains(executor.CurrentAgent.Metadata.OsType)) 
                    {
                        terminal.WriteError($"Command {this.Name} is not supported on {executor.CurrentAgent.Metadata.OsType}");
                        executor.InputHandled(this, true);
                        return;
                    }
                }
            }

            var label = this.Name;
            if (!string.IsNullOrEmpty(parms))
                label += " " + parms;
            var context = new CommandContext()
            {
                CommandLabel = label,
                CommandParameters = parms,
                CommModule = comm,
                Executor = executor,
                Terminal = terminal,
                Config = comm.Config
            };

            try
            {
                InnerExecute(context);
            }
            catch(Exception ex)
            {
                context.Terminal.WriteError($"An Error occurred : {ex}");
            }
            executor.InputHandled(this, true);
        }

        protected abstract void InnerExecute(CommandContext context);
    }
}
