using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.CommanderCommand;
using Commander.Commands;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.CommandLine.Core;
using Common.CommandLine.Execution;
using Spectre.Console;
using System.Reflection;


namespace Commander.CommanderCommand
{
    [Command("help", "Give info on available commands", Category = "Commander")]
    public class HelpCommand : ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
        {
            var mode = context.Executor.Mode;

            List<CommandDefinition> cmds = context.Executor.GetAllCommands();

            context.Terminal.WriteLine("Available commands :");

            var categories = new List<string> { CommandCategory.Commander };


            categories.AddRange(cmds.Select(c => c.CommandType.GetCustomAttribute<CommandAttribute>().Category).Distinct().Where(c => c != CommandCategory.Commander).OrderBy(cat => cat));

            foreach (var cat in categories)
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                // Add some columns
                table.AddColumn(new TableColumn("Name").LeftAligned());
                table.AddColumn(new TableColumn("Description").LeftAligned());

                var tmpCmds = cmds.Where(c => c.CommandType.GetCustomAttribute<CommandAttribute>().Category == cat);

                //if (mode == ExecutorMode.AgentInteraction && context.Executor.CurrentAgent != null && context.Executor.CurrentAgent.Metadata != null)
                //{
                //    tmpCmds = tmpCmds.Where(c => c.SupportedOs == null || !c.SupportedOs.Any() || c.SupportedOs.Contains(context.Executor.CurrentAgent.Metadata.OsType));
                //}

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
