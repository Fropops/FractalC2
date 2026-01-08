using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Commander.CommanderCommand;
using Commander.Executor;
using Commander.Models;
using Common.CommandLine.Core;
using Shared;

namespace Commander.CommanderCommand
{
    public class InteractAgentCommandOptions : CommandOption
    {
        [Argument("id", "index or id of the agent", 0, IsRequired = true)]
        public string id { get; set; }

    }

    [Command("int", "Select an agent to interact with", Category = "Commander", Aliases = new string[] { "interact" })]
    public class InteractAgentCommand : ICommand<CommanderCommandContext, InteractAgentCommandOptions>
    {
        public async Task<bool> Execute(CommanderCommandContext context, InteractAgentCommandOptions options)
        {
            Commander.Models.Agent agent = null;
            int index = 0;
            if (int.TryParse(options.id, out index))
                agent = context.CommModule.GetAgent(index);
            else
                agent = context.CommModule.GetAgents().FirstOrDefault(a => a.Metadata.Id.ToLower().Equals(options.id.ToLower()));

            if (agent == null)
            {
                context.Terminal.WriteError($"No agent with id or index {options.id} found.");
                return false;
            }

            context.Executor.CurrentAgent = agent;
            context.Executor.Mode = ExecutorMode.AgentInteraction;

            return true;
        }

    }
}
