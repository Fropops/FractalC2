using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class ListAgentsCommandOptions
    {
        public string listenerName { get; set; }
        public bool verbose { get; set; }
    }

    public class ListAgentsCommand : EnhancedCommand<ListAgentsCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "List all agents";
        public override string Name => "list";

        public override ExecutorMode AvaliableIn => ExecutorMode.Agent;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("listenerName",() => "" ,"Name of the listener"),
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ListAgentsCommandOptions> context)
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
                //var listenerName = listeners.FirstOrDefault(l => l.Id == agent.ListenerId)?.Name ?? string.Empty;
                //if (string.IsNullOrEmpty(context.Options.listenerName) || listenerName.ToLower().Equals(context.Options.listenerName.ToLower()))
                //{
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
                        //Version = agent.Metadata.Version,
                        //Listener = listenerName,
                    );
                //}
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

        public class ListAgentsEverywhareCommand : ListAgentsCommand
        {
            public override string Name => "agents";

            public override ExecutorMode AvaliableIn => ExecutorMode.All;
        }

    }

}
