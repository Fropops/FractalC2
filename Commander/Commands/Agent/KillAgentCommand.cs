using ApiModels.Response;
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
    public class KillAgentCommandOptions
    {
        public string id { get; set; }

        public bool all { get; set; }
    }


    public class KillAgentCommand : EnhancedCommand<DeleteAgentCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Kill an agent from the list";
        public override string Name => "kill";
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
            if (context.Options.all)
            {
                agents.AddRange(context.CommModule.GetAgents());
            }
            else
            {
                if(string.IsNullOrEmpty(context.Options.id))
                {
                    context.Terminal.WriteError($"Please specify an agent");
                    return false;
                }

                var agt = context.CommModule.GetAgent(context.Options.id);
                if (agt != null)
                {
                    agents.Add(agt);
                }
                else
                {
                    context.Terminal.WriteError($"Unable to find agent {context.Options.id}");
                    return false;
                }
            }

            foreach (var agent in agents)
            {
                await context.CommModule.TaskAgent(context.CommandLabel, Guid.NewGuid().ToString(), agent.Metadata.Id, EndPointCommand.TERMINATE);
                context.Terminal.WriteSuccess($"Command {EndPointCommand.TERMINATE} tasked to agent {agent.Metadata.Id}.");
            }

            return cmdRes;
        }
    }



}
