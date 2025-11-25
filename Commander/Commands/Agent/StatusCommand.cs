using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class StatusCommandOptions
    {
    }

    public class StatusCommand : EnhancedCommand<StatusCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Show current agent status";
        public override string Name => "status";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description);

       

        protected async override Task<bool> HandleCommand(CommandContext<StatusCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            var activ = context.IsAgentAlive(agent);
            if (activ == true)
                context.Terminal.WriteSuccess($"Agent {agent.Id} is up and running !");
            if(activ == false)
                context.Terminal.WriteError($"Agent {agent.Id} seems to be not responding!");
            if(activ == null)
                context.Terminal.WriteInfo($"Agent {agent.Id} response time is unknown!");

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Item").LeftAligned());
            table.AddColumn(new TableColumn("Value").LeftAligned());
            table.HideHeaders();

            table.AddRow("Id", agent.Id ?? string.Empty);
            table.AddRow("Implant", agent.Metadata?.ImplantId ?? string.Empty);
            table.AddRow("Hostname", agent.Metadata?.Hostname ?? string.Empty);
            table.AddRow("User Name", agent.Metadata?.UserName ?? string.Empty);
            table.AddRow("IP", StringHelper.IpAsString(agent.Metadata?.Address));
            table.AddRow("Process Id", agent.Metadata?.ProcessId.ToString() ?? string.Empty);
            table.AddRow("Process Name", agent.Metadata?.ProcessName ?? string.Empty);
            table.AddRow("Architecture", agent.Metadata?.Architecture ?? string.Empty);
            table.AddRow("Integrity", agent.Metadata?.Integrity.ToString() ?? string.Empty);
            table.AddRow("EndPoint", agent.Metadata?.EndPoint ?? string.Empty);
            table.AddRow("Version", agent.Metadata?.Version ?? string.Empty);
            if (string.IsNullOrEmpty(agent.RelayId))
                table.AddRow("Sleep", agent.Metadata?.Sleep ?? string.Empty);
            table.AddRow("First Seen", agent.FirstSeen.ToLocalTime().ToString());
            table.AddRow("Last Seen", StringHelper.FormatElapsedTime(Math.Round(agent.LastSeenDelta.TotalSeconds, 2)) ?? string.Empty);


            context.Terminal.Write(table);
            return true;
        }



    }
}
