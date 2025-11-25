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
    public class HomeCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Navigation;
        public override string Description => "Return to home mode";
        public override string Name => "home";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(CommandContext context)
        {
            context.Executor.Mode = ExecutorMode.None;
            context.Terminal.Prompt = Terminal.Terminal.DefaultPrompt;
        }
    }
}
