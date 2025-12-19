using Commander.Commands.Agent;
using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Terminal;
using Common.Models;
using Newtonsoft.Json;
using Shared;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Commands.Core
{
    public class ListenerCommandOptions : VerbAwareCommandOptions
    {
        public string name { get; set; }
        public int? port { get; set; }
        public string address { get; set; }
        public bool secured { get; set; }
    }

    public class ListenerCommand : VerbAwareCommand<ListenerCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Manage listeners";
        public override string Name => "listener";

        public override string[] Alternate => new string[] { "listeners" };

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(Description)
            {
                new Argument<string>("verb", () => CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Start.Command(), CommandVerbs.Stop.Command(), CommandVerbs.Show.Command()),
                new Option<string>(new[] { "--name", "-n" }, "name of the listener"),
                new Option<string>(new[] { "--address", "-a" }, () => "127.0.0.1", "The listening address."),
                new Option<int?>(new[] { "--port", "-p" }, () => null, "The listening port."),
                new Option<bool>(new[] { "--secured", "-s" }, () => true, "HTTPS if secured else HTTP"),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            Register("start", Start);
            Register("stop", Stop);
            Register("show", Show);
        }

        protected async Task<bool> Start(CommandContext<ListenerCommandOptions> context)
        {
            if (string.IsNullOrEmpty(context.Options.name))
            {
                context.Terminal.WriteError("[X] Name is required to start a listener!");
                return false;
            }

            if (!context.Options.port.HasValue)
            {
                if (context.Options.secured)
                    context.Options.port = 443;
                else
                    context.Options.port = 80;
            }

            if (context.CommModule.GetListeners().Any(l => l.Name.ToLower().Equals(context.Options.name.ToLower())))
            {
                context.Terminal.WriteError($"A listener with the name {context.Options.name} already exists !");
                return false;
            }

            var result = await context.CommModule.CreateListener(context.Options.name, context.Options.port.Value, context.Options.address, context.Options.secured);
            if (!result.IsSuccessStatusCode)
            {
                context.Terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var listener = JsonConvert.DeserializeObject<TeamServerListener>(json);

            context.Terminal.WriteSuccess($"Listener {listener.Name} started on port {listener.BindPort}.");
            return true;
        }

        protected async Task<bool> Stop(CommandContext<ListenerCommandOptions> context)
        {
            if (string.IsNullOrEmpty(context.Options.name))
            {
                context.Terminal.WriteError("[X] Name is required to stop a listener!");
                return false;
            }

            var listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower().Equals(context.Options.name.ToLower()));
            if (listener == null)
            {
                context.Terminal.WriteError($"Cannot find listener with the name {context.Options.name} !");
                return false;
            }

            var result = await context.CommModule.StopListener(listener.Id);
            if (!result.IsSuccessStatusCode)
            {
                context.Terminal.WriteError("An error occured : " + result.StatusCode);
                return false;
            }

            context.Terminal.WriteSuccess($"Listener {listener.Name} stopped.");
            return true;
        }

        protected async Task<bool> Show(CommandContext<ListenerCommandOptions> context)
        {
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
                    listener.Ip ?? "127.0.0.1",
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
}
