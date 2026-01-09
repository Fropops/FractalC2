using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Common.CommandLine.Execution;
using Spectre.Console;


namespace Commander.Commands
{
    [Command("help", "Give info on available commands", Category = "Commander")]
    public class HelpCommand : ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
        {
            List<CommandDefinition> cmds = context.Executor.GetAllCommands();

            context.Terminal.WriteLine("Available commands :");

            var categories = new List<string> { "Commander" };

            var availableCommands = new List<CommandDefinition>();
            foreach (var cmdDef in cmds)
            {
                if (typeof(Common.AgentCommands.AgentCommandBase).IsAssignableFrom(cmdDef.CommandType))
                {
                    var cmd = (AgentCommandBase)Activator.CreateInstance(cmdDef.CommandType);
                    if (context.Executor.CurrentAgent != null && cmd.SupportedOs.Contains(context.Executor.CurrentAgent.Metadata.OsType))
                    {
                        availableCommands.Add(cmdDef);
                    }
                }
                else if (typeof(Commander.Commands.Agent.ICommanderAgentCommand).IsAssignableFrom(cmdDef.CommandType))
                {
                    if (context.Executor.CurrentAgent != null)
                    {
                        availableCommands.Add(cmdDef);
                    }
                }
                else
                    availableCommands.Add(cmdDef);
            }

            categories.AddRange(availableCommands.Select(c => c.CommandType.GetCustomAttribute<CommandAttribute>().Category).Distinct().Where(c => c != "Commander").OrderBy(cat => cat));


            foreach (var cat in categories)
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                // Add some columns
                table.AddColumn(new TableColumn("Name").LeftAligned());
                table.AddColumn(new TableColumn("Description").LeftAligned());

                var tmpCmds = availableCommands.Where(c => c.CommandType.GetCustomAttribute<CommandAttribute>().Category == cat);

                if (!tmpCmds.Any())
                    continue;

                context.Terminal.Write(new Rule(cat));


                foreach (var cmd in tmpCmds.OrderBy(c => c.CommandType.GetCustomAttribute<CommandAttribute>().Name))
                {
                    table.AddRow(cmd.CommandType.GetCustomAttribute<CommandAttribute>().Name, cmd.CommandType.GetCustomAttribute<CommandAttribute>().Description);
                }

                table.Expand();
                context.Terminal.Write(table);
            }
            return true;
        }

    }
}
