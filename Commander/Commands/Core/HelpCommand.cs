using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Core
{
    public class HelpCommand : ExecutorCommand
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Give info on available commands";
        public override string Name => "help";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        protected override void InnerExecute(CommandContext context)
        {
            var mode = context.Executor.Mode;

            List<ExecutorCommand> cmds = new List<ExecutorCommand>();
            cmds.AddRange(context.Executor.GetCommandsInMode(mode));
            cmds.AddRange(context.Executor.GetCommandsInMode(ExecutorMode.All));

            context.Terminal.WriteLine("Available commands :");
            bool first = true;
            foreach (var cat in CommandCategory.All)
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                // Add some columns
                table.AddColumn(new TableColumn("Name").LeftAligned());
                table.AddColumn(new TableColumn("Description").LeftAligned());

                var tmpCmds = cmds.Where(c => c.Category == cat);
                if (!tmpCmds.Any())
                    continue;
                /*if (first)
                    first = false;
                else
                    context.Terminal.WriteLine();*/
                context.Terminal.Write(new Rule(cat));


                foreach (var cmd in tmpCmds.OrderBy(c => c.Name))
                {
                    table.AddRow(cmd.Name, cmd.Description);
                }

                table.Expand();
                context.Terminal.Write(table);
            }

        }

    }
}
