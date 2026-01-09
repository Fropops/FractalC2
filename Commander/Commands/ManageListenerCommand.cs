using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Common.Models;
using Newtonsoft.Json;
using Shared;
using Spectre.Console;

namespace Commander.Commands
{
    public class ManageListenerCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Start, CommandVerbs.Stop, CommandVerbs.Show }, IsRequired = true)]
        public override string verb { get; set; }
        [Option("n", "name", "Name of the listener", DefaultValue = "Local")]
        public string name { get; set; }
        [Option("p", "port", "The listening address", DefaultValue = null)]
        public int? port { get; set; }
        [Option("a", "address", "The listening address", DefaultValue = "127.0.0.1")]
        public string address { get; set; }
        [Option("s", "secured", "HTTPS if secured else HTTP", DefaultValue = true)]
        public bool secured { get; set; }
    }

    [Command("listener", "Manager TeamServer Listeners", Category = "Commander", Aliases = new string[] { "implants" })]
    public class ManageListenerCommand : VerbCommand<CommanderCommandContext, ManageListenerCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            this.Register(CommandVerbs.Start.ToString(), Start);
            this.Register(CommandVerbs.Stop.ToString(), Stop);
            this.Register(CommandVerbs.Show.ToString(), Show);
        }

        protected async Task<bool> Start(CommanderCommandContext context, ManageListenerCommandOptions options)
        {
            if (string.IsNullOrEmpty(options.name))
            {
                context.Terminal.WriteError("[X] Name is required to start a listener!");
                return false;
            }

            if (!options.port.HasValue)
            {
                if (options.secured)
                    options.port = 443;
                else
                    options.port = 80;
            }

            if (context.CommModule.GetListeners().Any(l => l.Name.ToLower().Equals(options.name.ToLower())))
            {
                context.Terminal.WriteError($"A listener with the name {options.name} already exists !");
                return false;
            }

            var result = await context.CommModule.CreateListener(options.name, options.port.Value, options.address, options.secured);
            if (!result.IsSuccessStatusCode)
            {
                var body = await result.Content.ReadAsStringAsync();
                context.Terminal.WriteError("An error occured : " + result.StatusCode + Environment.NewLine + body);
                return false;
            }

            var json = await result.Content.ReadAsStringAsync();
            var listener = JsonConvert.DeserializeObject<TeamServerListener>(json);

            context.Terminal.WriteSuccess($"Listener {listener.Name} started on port {listener.BindPort}.");
            return true;
        }

        protected async Task<bool> Stop(CommanderCommandContext context, ManageListenerCommandOptions options)
        {
            if (string.IsNullOrEmpty(options.name))
            {
                context.Terminal.WriteError("[X] Name is required to stop a listener!");
                return false;
            }

            var listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower().Equals(options.name.ToLower()));
            if (listener == null)
            {
                context.Terminal.WriteError($"Cannot find listener with the name {options.name} !");
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

        protected async Task<bool> Show(CommanderCommandContext context, ManageListenerCommandOptions options)
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
