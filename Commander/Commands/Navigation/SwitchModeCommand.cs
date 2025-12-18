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
    public abstract class SwitchModeCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Navigation;
        public override string Description => $"Switch to {TargetMode} mode";
        public override ExecutorMode AvaliableIn => ExecutorMode.None;

        public abstract ExecutorMode TargetMode { get; }
        protected override void InnerExecute(CommandContext context)
        {
            context.Executor.CurrentAgent = null;
            context.Executor.Mode = TargetMode;
            if (TargetMode == ExecutorMode.None)
                context.Terminal.Prompt = Terminal.Terminal.DefaultPrompt;
            else
                context.Terminal.Prompt = $"${TargetMode}> ";
        }
    }


}
