using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Core
{
    public class QuitCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Close the Commander";
        public override string Name => "exit";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(CommandContext context)
        {
            if (context.CommModule.ConnectionStatus == ConnectionStatus.Connected)
                context.CommModule.CloseSession().Wait();
            context.Executor.Stop();
        }
    }
}
