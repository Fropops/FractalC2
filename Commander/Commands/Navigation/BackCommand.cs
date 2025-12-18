using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Navigation
{
    public class BackCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Navigation;
        public override string Description => "Return to Commander mode";
        public override string Name => "back";

        public override string[] Alternate { get => new string[] {"home" }; }
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        protected override void InnerExecute(CommandContext context)
        {
            switch (context.Executor.Mode)
            {
                case ExecutorMode.AgentInteraction:
                    {
                        context.Executor.CurrentAgent = null;
                        context.Executor.Mode = ExecutorMode.None;
                    }
                    break;
                default:
                    {
                        context.Executor.Mode = ExecutorMode.None;
                    }
                    break;
            }

            if (context.Executor.Mode == ExecutorMode.None)
                context.Terminal.Prompt = Terminal.Terminal.DefaultPrompt;
            else
                context.Terminal.Prompt = $"${context.Executor.Mode}> ";

        }
    }
}
