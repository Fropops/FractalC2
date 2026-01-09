using Commander.CommanderCommand.Abstract;
using Commander.Helper;
using Common.APIModels;
using Common.CommandLine.Core;
using Common.Payload;
using Shared;
using System.Threading.Tasks;

namespace Commander.CommanderCommand
{
    public class RPortFwdCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = "show", AllowedValues = new object[] { "start", "stop" }, IsRequired = true)]
        public override string verb { get; set; }

        [Option("port", "p", "port to use on the agent")]
        public int? port { get; set; }

        [Option("destHost", "h", "host to use as destination")]
        public string destHost { get; set; }

        [Option("destPort", "d", "port to use as destination")]
        public int? destPort { get; set; }
    }

    [Command("rportfwd", "Start a Reverse Port Forward on the agent", Category = "Network")]
    public class RPortFwdCommand : VerbCommand<CommanderCommandContext, RPortFwdCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            Register(CommandVerbs.Start.Command(), Start);
            Register(CommandVerbs.Stop.Command(), Stop);
        }

        private async Task<bool> Start(CommanderCommandContext context, RPortFwdCommandOptions options)
        {
            var agent = context.Executor.CurrentAgent;
            if (agent == null)
            {
                context.Terminal.WriteError("No active agent interaction.");
                return false;
            }

            if (!options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to start the port forward!");
                return false;
            }
            if (string.IsNullOrEmpty(options.destHost))
            {
                context.Terminal.WriteError("[X] Destination Host is required to start the port forward!");
                return false;
            }
            if (!options.destPort.HasValue)
            {
                context.Terminal.WriteError("[X] Destination Port is required to start the port forward!");
                return false;
            }

            ReversePortForwardDestination dest = new ReversePortForwardDestination()
            {
                Hostname = options.destHost,
                Port = options.destPort.Value
            };

            var parameters = new ParameterDictionary();
            parameters.AddParameter(ParameterId.Port, options.port.Value);
            parameters.AddParameter(ParameterId.Parameters, dest);
            parameters.AddParameter(ParameterId.Verb, CommandVerbs.Start);

            await context.CommModule.TaskAgent("Start rportfwd", agent.Metadata.Id, CommandId.RportFwd, parameters);

            return true;
        }

        private async Task<bool> Stop(CommanderCommandContext context, RPortFwdCommandOptions options)
        {
            var agent = context.Executor.CurrentAgent;
            if (agent == null)
            {
                context.Terminal.WriteError("No active agent interaction.");
                return false;
            }

            if (!options.port.HasValue)
            {
                context.Terminal.WriteError("[X] Port is required to stop the port forward!");
                return false;
            }

            var parameters = new ParameterDictionary();
            parameters.AddParameter(ParameterId.Port, options.port.Value);
            parameters.AddParameter(ParameterId.Verb, CommandVerbs.Stop);

            await context.CommModule.TaskAgent("Stop rportfwd", agent.Metadata.Id, CommandId.RportFwd, parameters);

            return true;
        }
    }
}
