using Commander.Commands;
using Commander.Executor;
using Common.CommandLine.Core;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    [Command("back", "Return to Commander mode", Category = "Commander", Aliases = new[] { "home" })]
    public class BackCommand : ICommanderAgentCommand, ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
        {
            context.Executor.CurrentAgent = null;
            context.Terminal.Prompt = Terminal.Terminal.DefaultPrompt;
            return true;
        }
    }
}
