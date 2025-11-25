using Commander.Executor;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class InteractAgentCommandOptions
    {
        public string id { get; set; }

    }


    public class InteractAgentCommand : EnhancedCommand<InteractAgentCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Select an agent to interact with";
        public override string Name => "interact";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override string[] Alternate { get => new string[1] { "int" }; }

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("id", "index or id of the agent"),
            };

        protected override async Task<bool> HandleCommand(CommandContext<InteractAgentCommandOptions> context)
        {
            Commander.Models.Agent agent = null;
            int index = 0;
            if (int.TryParse(context.Options.id, out index))
                agent = context.CommModule.GetAgent(index);
            else
                agent = context.CommModule.GetAgents().FirstOrDefault(a => a.Metadata.Id.ToLower().Equals(context.Options.id.ToLower()));
            
            if(agent == null)
            {
                context.Terminal.WriteError($"No agent with id or index {context.Options.id} found.");
                return false;
            }

            context.Executor.CurrentAgent = agent;
            context.Executor.Mode = ExecutorMode.AgentInteraction;

            return true;
        }
    }



}
