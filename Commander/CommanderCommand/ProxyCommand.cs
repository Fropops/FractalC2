using Commander.CommanderCommand.Abstract;
using Commander.Helper;
using Common.CommandLine.Core;
using Shared;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.CommanderCommand
{
    public class ProxyCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = "show", AllowedValues = new object[] { "start", "stop", "show" }, IsRequired = true)]
        public override string verb { get; set; }

        [Option("port", "p", "port to use on the server")]
        public int? port { get; set; } = 1080;
    }

    [Command("proxy", "Start a Socks4 Proxy on the agent", Category = "Network")]
    public class ProxyCommand : VerbCommand<CommanderCommandContext, ProxyCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            Register(CommandVerbs.Start.Command(), Start);
            Register(CommandVerbs.Stop.Command(), Stop);
            Register(CommandVerbs.Show.Command(), Show);
        }

        private async Task<bool> Start(CommanderCommandContext context, ProxyCommandOptions options)
        {
            var agent = context.Executor.CurrentAgent;
            if (agent == null)
            {
                context.Terminal.WriteError("No active agent interaction.");
                return false;
            }

            if (!options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to start the proxy!");
                return false;
            }
            var res = await context.CommModule.StartProxy(agent.Metadata.Id, options.port.Value);
            if (!res)
            {
                context.Terminal.WriteError("[X] Cannot start proxy on the server!");
                return false;
            }

            context.Terminal.WriteSuccess("[*] Proxy server started !");
            return true;
        }

        private async Task<bool> Stop(CommanderCommandContext context, ProxyCommandOptions options)
        {
            if (!options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to stop the proxy!");
                return false;
            }

            var res = await context.CommModule.StopProxy(options.port.Value);
            if (!res)
            {
                context.Terminal.WriteError("[X] Cannot stop proxy on the server!");
                return false;
            }
            context.Terminal.WriteSuccess("[*] Proxy server stopped !");
            return true;
        }

        private async Task<bool> Show(CommanderCommandContext context, ProxyCommandOptions options)
        {
            var res = await context.CommModule.ShowProxy();
            if (!res.Any())
            {
                context.Terminal.WriteLine("[>] No proxy running!");
                return true;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            // Add some columns
            table.AddColumn(new TableColumn("Agent").LeftAligned());
            table.AddColumn(new TableColumn("Port").LeftAligned());
            foreach (var item in res)
            {
                table.AddRow(item.AgentId, item.Port.ToString());
            }

            context.Terminal.Write(table);

            return true;
        }
    }
}
