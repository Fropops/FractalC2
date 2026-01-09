using Commander.Executor;
using Common.CommandLine.Core;
using System.Threading.Tasks;

namespace Commander.CommanderCommand.Agent
{
    [Command("back", "Return to Commander mode", Category = "Commander", Aliases = new[] { "home" })]
    public class BackCommand : ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
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

            return true;
        }
    }
}
