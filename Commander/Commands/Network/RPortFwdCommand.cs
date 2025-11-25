using Commander.Communication;
using Commander.Executor;
using Commander.Helper;
using Commander.Models;
using Commander.Terminal;
using Shared;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Network
{
    public class RPortFwdCommandOptions : VerbAwareCommandOptions
    {
        public int? port { get; set; }
        public string destHost { get; set; }
        public int? destPort { get; set; }
    }

    public class RPortFwdCommand : VerbAwareCommand<RPortFwdCommandOptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Start a Reverse Port Forward on the agent";
        public override string Name => "rportfwd";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.RportFwd;

        public override RootCommand Command => new RootCommand(Description)
            {
                new Argument<string>("verb", () => CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Start.Command(), CommandVerbs.Stop.Command(), CommandVerbs.Show.Command()),
                new Option<int?>(new[] { "--port", "-p" }, () => null, "port to use on the agent"),
                new Option<string>(new[] { "--destHost", "-h" }, () => null, "host to use as destination"),
                new Option<int?>(new[] { "--destPort", "-d" }, () => null, "port to use as destination"),
            };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            Register(CommandVerbs.Start, Start);
            Register(CommandVerbs.Stop, Stop);
        }

        protected async Task<bool> Start(CommandContext<RPortFwdCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (!context.Options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to start the port forward!");
                return false;
            }
            if (string.IsNullOrEmpty(context.Options.destHost))
            {
                context.Terminal.WriteError("[X] Destination Host is required to start the port forward!");
                return false;
            }
            if (!context.Options.destPort.HasValue)
            {
                context.Terminal.WriteError("[X] Destination Port is required to start the port forward!");
                return false;
            }

            ReversePortForwardDestination dest = new ReversePortForwardDestination()
            {
                Hostname = context.Options.destHost,
                Port = context.Options.destPort.Value
            };

            context.AddParameter(ParameterId.Port, context.Options.port.Value);
            context.AddParameter(ParameterId.Parameters, dest);

            return true;
        }

        protected async Task<bool> Stop(CommandContext<RPortFwdCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (!context.Options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to stop the port forward!");
                return false;
            }

            context.AddParameter(ParameterId.Port, context.Options.port.Value);

            return true;
        }
    }
}
