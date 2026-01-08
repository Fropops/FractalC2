using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.CommandLine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Commander.CommanderCommand
{
    [Command("exit", "Close the Commander", Category = "Commander")]
    public class QuitCommand : ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
        {
            if (context.CommModule.ConnectionStatus == ConnectionStatus.Connected)
                context.CommModule.CloseSession().Wait();
            context.Executor.Stop();

            return true;
        }
    }
}
