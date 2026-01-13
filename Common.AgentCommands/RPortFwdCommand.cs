using Common.AgentCommands;
using Common.APIModels;
using Common.CommandLine.Core;
using Common.Payload;
using Shared;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class RPortFwdCommandOptions : CommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = "show", AllowedValues = new object[] { "show", "start", "stop",  }, IsRequired = true)]
        public string verb { get; set; }

        [Option("p", "port", "port to use on the agent")]
        public int? port { get; set; }

        [Option("dh", "destHost", "host to use as destination")]
        public string destHost { get; set; }

        [Option("dp", "destPort", "port to use as destination")]
        public int? destPort { get; set; }
    }

    [Command("rportfwd", "Start a Reverse Port Forward on the agent", Category = AgentCommandCategories.Network)]
    public class RPortFwdCommand : AgentCommand<RPortFwdCommandOptions>
    {
        public override CommandId CommandId => CommandId.RportFwd;

        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };

        public override async Task<bool> Execute(AgentCommandContext context, RPortFwdCommandOptions options)
        {
            switch(options.verb.ToLower())
            {
                case "show":
                    return await Show(context, options);
                case "start":
                    return await Start(context, options);
                case "stop":
                    return await Stop(context, options);
                default:
                    context.WriteError($"[X] Unknown verb: {options.verb}");
                    return false;
            }
        }
        private async Task<bool> Start(AgentCommandContext context, RPortFwdCommandOptions options)
        {

            if (!options.port.HasValue)
            {
                context.WriteError("[X] Port is required to start the port forward!");
                return false;
            }
            if (string.IsNullOrEmpty(options.destHost))
            {
                context.WriteError("[X] Destination Host is required to start the port forward!");
                return false;
            }
            if (!options.destPort.HasValue)
            {
                context.WriteError("[X] Destination Port is required to start the port forward!");
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

            context.TaskAgent(options.CommandLine, this.CommandId, parameters);

            return true;
        }

        private async Task<bool> Stop(AgentCommandContext context, RPortFwdCommandOptions options)
        {
            if (!options.port.HasValue)
            {
                context.WriteError("[X] Port is required to stop the port forward!");
                return false;
            }

            var parameters = new ParameterDictionary();
            parameters.AddParameter(ParameterId.Port, options.port.Value);
            parameters.AddParameter(ParameterId.Verb, CommandVerbs.Stop);

            context.TaskAgent(options.CommandLine, this.CommandId, parameters);

            return true;
        }

        private async Task<bool> Show(AgentCommandContext context, RPortFwdCommandOptions options)
        {
           
            var parameters = new ParameterDictionary();
            parameters.AddParameter(ParameterId.Verb, CommandVerbs.Show);
            context.TaskAgent(options.CommandLine, this.CommandId, parameters);
            return true;
        }
    }
}
