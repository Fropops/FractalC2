using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class DeleteAgentCommandOptions
    {
        public string id { get; set; }
        public bool all { get; set; }
    }


    public class DeleteAgentCommand : EnhancedCommand<DeleteAgentCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Delete an agent from the list";
        public override string Name => "delete";
        public override ExecutorMode AvaliableIn => ExecutorMode.Agent;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("id", () => "", "Id of the agent"),
                new Option(new[] { "--all", "-a" }, "Delete all the agents."),
            };

        protected override async Task<bool> HandleCommand(CommandContext<DeleteAgentCommandOptions> context)
        {
            bool cmdRes = true;
            var agents = new List<Models.Agent>();
            if (context.Options.all || string.IsNullOrEmpty(context.Options.id))
            {
                agents.AddRange(context.CommModule.GetAgents());
            }
            else
            {
                var agt = context.CommModule.GetAgent(context.Options.id);
                if (agt != null)
                    agents.Add(agt);
            }

            foreach (var agent in agents)
            {
                var result = await context.CommModule.StopAgent(agent.Id);

                if (!result.IsSuccessStatusCode)
                {
                    context.Terminal.WriteError($"An error occured : {result.StatusCode}");
                    cmdRes = false;
                }
                else
                    context.Terminal.WriteSuccess($"{agent.Id} was deleted.");
            }

            return cmdRes;
        }
    }
}
