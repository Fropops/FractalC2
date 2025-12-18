using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class AgentCommandOptions : VerbAwareCommandOptions
    {
        public string id { get; set; }
        public bool all { get; set; }
    }

    public class AgentCommand : VerbAwareCommand<AgentCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Manage agents";
        public override string Name => "agent";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override string[] Alternate => new string[] { "agents" };

        public override RootCommand Command => new RootCommand(Description)
        {
            new Argument<string>("verb", () => "show").FromAmong("show", "delete"),
            new Option<string>(new[] { "--id", "-i" }, "Id of the agent"),
            new Option<bool>(new[] { "--all", "-a" }, "Apply to all agents"),
        };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            Register("show", Show);
            Register("delete", Delete);
        }

        protected async Task<bool> Show(CommandContext<AgentCommandOptions> context)
        {
             var result = context.CommModule.GetAgents();
            if (result.Count() == 0)
            {
                context.Terminal.WriteLine("No Agents running.");
                return true;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Index").Centered());
            table.AddColumn(new TableColumn("Id").LeftAligned());
            table.AddColumn(new TableColumn("Implant").LeftAligned());
            table.AddColumn(new TableColumn("Active").LeftAligned());
            table.AddColumn(new TableColumn("User").LeftAligned());
            table.AddColumn(new TableColumn("Host").LeftAligned());
            table.AddColumn(new TableColumn("Address").LeftAligned());
            table.AddColumn(new TableColumn("Integrity").LeftAligned());
            table.AddColumn(new TableColumn("Process").LeftAligned());
            table.AddColumn(new TableColumn("Arch.").LeftAligned());
            table.AddColumn(new TableColumn("End Point").LeftAligned());
            table.AddColumn(new TableColumn("Last Seen").LeftAligned());

            var listeners = context.CommModule.GetListeners();
            var index = 0;
            foreach (var agent in result.OrderBy(a => a.FirstSeen))
            {
                var activ = context.IsAgentAlive(agent);
                var activStr = "Unknown";
                if (activ == true)
                    activStr = "Yes";
                if(activ == false)
                    activStr = "No";

                table.AddRow(
                        SurroundIfDeadOrSelf(agent, context, index.ToString()),
                        SurroundIfDeadOrSelf(agent, context, agent.Id),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.ImplantId),
                        SurroundIfDeadOrSelf(agent, context, activStr),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.UserName),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.Hostname),
                        SurroundIfDeadOrSelf(agent, context, StringHelper.IpAsString(agent.Metadata?.Address)),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.Integrity.ToString()),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.ProcessName + " (" + agent.Metadata?.ProcessId + ")"),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.Architecture),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.EndPoint),
                        SurroundIfDeadOrSelf(agent, context, StringHelper.FormatElapsedTime(Math.Round(agent.LastSeenDelta.TotalSeconds, 2)))
                    );
                index++;
            }

            table.Expand();
            context.Terminal.Write(table);

            return true;
        }

        private IRenderable SurroundIfDeadOrSelf(Models.Agent agent, CommandContext ctxt, string value)
        {
            if (string.IsNullOrEmpty(value))
                return new Markup(string.Empty);

            if(ctxt.Executor.CurrentAgent != null && ctxt.Executor.CurrentAgent.Id == agent.Id)
                return new Markup($"[cyan]{value}[/]");

            if (ctxt.IsAgentAlive(agent) != true)
                return new Markup($"[grey]{value}[/]");
            else
                return new Markup(value);
        }

        protected async Task<bool> Delete(CommandContext<AgentCommandOptions> context)
        {
            bool cmdRes = true;
            var agents = new List<Models.Agent>();
            if (context.Options.all)
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
