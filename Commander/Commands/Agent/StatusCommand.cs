using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands;
using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Common.CommandLine.Core;
using Spectre.Console;

namespace Commander.Commands.Agent
{
    [Command("Status", "Show current agent status", Category = "Commander")]
    public class StatusCommand : ICommanderAgentCommand, ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
        {
            if(context.Executor.CurrentAgent == null)
            {
                context.Terminal.WriteError("No agent selected. Use 'interact' command to select an agent.");
                return false;
            }
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

            string arch = agent.Metadata?.Architecture ?? "Unknown Arch";
            arch += " - " + agent.Metadata?.OsType ?? "Unknown Os";

            table.AddRow("Id", agent.Id ?? string.Empty);
            table.AddRow("Name", agent.Metadata?.Name ?? string.Empty);
            table.AddRow("Hostname", agent.Metadata?.Hostname ?? string.Empty);
            table.AddRow("User Name", agent.Metadata?.UserName ?? string.Empty);
            table.AddRow("IP", StringHelper.IpAsString(agent.Metadata?.Address));
            table.AddRow("Process Id", agent.Metadata?.ProcessId.ToString() ?? string.Empty);
            table.AddRow("Process Name", agent.Metadata?.ProcessName ?? string.Empty);
            table.AddRow("Architecture", arch);
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
