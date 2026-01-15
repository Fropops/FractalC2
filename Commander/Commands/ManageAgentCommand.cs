using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Commander.Communication;
using Commander.Executor;
using Commander.Helper;

using Commander.Terminal;
using Common.CommandLine.Core;
using Common.Models;
using Newtonsoft.Json;
using Shared;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Commander.Commands
{

    public class ManageAgentCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Show, "Delete" }, IsRequired = true)]
        public override string verb { get; set; }
        [Option("id", "index", "Index of the agent (or name) - For Delete Action")]
        public string index { get; set; }
        [Option("a", "all", "Apply to all agents - For Delete Action")]
        public bool all { get; set; }
    }

    [Command("agent", "Manage agents", Category = "Commander", Aliases = new string[] { "agents" })]
    public class ManageAgentCommand : VerbCommand<CommanderCommandContext, ManageAgentCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            Register("show", Show);
            Register("delete", Delete);
        }

        protected async Task<bool> Show(CommanderCommandContext context, ManageAgentCommandOptions options)
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
            //table.AddColumn(new TableColumn("Id").LeftAligned());
            table.AddColumn(new TableColumn("Name").LeftAligned());
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

                string arch = agent.Metadata?.Architecture ?? "Unknown Arch";
                arch += " - " + agent.Metadata?.OsType ?? "Unknown Os";

                table.AddRow(
                        SurroundIfDeadOrSelf(agent, context, index.ToString()),
                        //SurroundIfDeadOrSelf(agent, context, agent.Id),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.Name),
                        SurroundIfDeadOrSelf(agent, context, activStr),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.UserName),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.Hostname),
                        SurroundIfDeadOrSelf(agent, context, StringHelper.IpAsString(agent.Metadata?.Address)),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.Integrity.ToString()),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.ProcessName + " (" + agent.Metadata?.ProcessId + ")"),
                        SurroundIfDeadOrSelf(agent, context, arch),
                        SurroundIfDeadOrSelf(agent, context, agent.Metadata?.EndPoint),
                        SurroundIfDeadOrSelf(agent, context, StringHelper.FormatElapsedTime(Math.Round(agent.LastSeenDelta.TotalSeconds, 2)))
                    );
                index++;
            }

            table.Expand();
            context.Terminal.Write(table);

            return true;
        }

        private IRenderable SurroundIfDeadOrSelf(Common.Models.Agent agent, CommanderCommandContext ctxt, string value)
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

        protected async Task<bool> Delete(CommanderCommandContext context, ManageAgentCommandOptions options)
        {
            bool cmdRes = true;

            var agents = new List<Common.Models.Agent>();



            if (options.all)
            {
                agents.AddRange(context.CommModule.GetAgents());
            }
            else
            {
                if(string.IsNullOrEmpty(options.index))
                {
                    context.Terminal.WriteError("Index is mandatory");
                    return false;
                }
                int index = 0;
                Common.Models.Agent agt = null;
                if (int.TryParse(options.index, out index))
                    agt = context.CommModule.GetAgent(index);
                else
                    agt = context.CommModule.GetAgents().FirstOrDefault(a => a.Metadata.Name.ToLower().Equals(options.index.ToLower()));
                if (agt != null)
                    agents.Add(agt);
            }

            foreach (var agent in agents)
            {
                if(context.IsAgentAlive(agent) == true)
                {
                    context.Terminal.WriteInfo($"Agent {agent.Id} is still active. It will not be deleted.");
                    continue;
                }

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
