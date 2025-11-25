using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Listener
{
    public class ListListenersCommandOptions
    {
        public bool verbose { get; set; }
    }

    public class ListListenersCommand : EnhancedCommand<ListListenersCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "List all listeners";
        public override string Name => "list";

        public override ExecutorMode AvaliableIn => ExecutorMode.Listener;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ListListenersCommandOptions> context)
        {
            
            if (context.Options.verbose)
                context.Terminal.WriteLine("Hello from verbose output!");

            var result = context.CommModule.GetListeners();
            if (result.Count() == 0)
            {
                context.Terminal.WriteInfo("No Listeners running.");
                return true;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Index").Centered());
            table.AddColumn(new TableColumn("Name").LeftAligned());
            table.AddColumn(new TableColumn("Port").LeftAligned());
            table.AddColumn(new TableColumn("Host").LeftAligned());
            table.AddColumn(new TableColumn("Id").LeftAligned());
            table.AddColumn(new TableColumn("Secure").LeftAligned());

            var index = 0;
            foreach (var listener in result)
            {
                table.AddRow(
                    index.ToString(),
                    listener.Name,
                    listener.BindPort.ToString(),
                    listener.Ip ?? "*",
                    listener.Id,
                    listener.Secured ? "Yes" : "No"
                );

                index++;
            }

            table.Expand();
            context.Terminal.Write(table);
            return true;
        }

    }

    public class ListListenersEverywhereCommand : ListListenersCommand
    {
        public override ExecutorMode AvaliableIn => ExecutorMode.All;
        public override string Name => "listeners";
    }
}
