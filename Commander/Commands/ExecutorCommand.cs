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

        public virtual void Execute(string parms)
        {
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();
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
