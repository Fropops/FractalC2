using System.CommandLine;
using WebCommander.Models;
using System.Text;
using Shared;
using WebCommander.Commands;
using WebCommander.Services;

namespace WebCommander.Commands.VerbCommand
{
    public class RPortFwdCommand : ParsedCommand
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Start a Reverse Port Forward on the agent";
        public override string Name => "rportfwd";
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override CommandId Id => CommandId.RportFwd;

        private const string VerbArg = "verb";
        private const string PortOpt = "port";
        private const string DestHostOpt = "destHost";
        private const string DestPortOpt = "destPort";

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(VerbArg) { Description = "The action to perform (start, stop)" });
            
            command.Options.Add(new Option<int>(PortOpt, new[] { "--port", "-p" }) { Description = "Port to use on the agent" });
            command.Options.Add(new Option<string>(DestHostOpt, new[] { "--destHost", "-h" }) { Description = "Host to use as destination" });
            command.Options.Add(new Option<int>(DestPortOpt, new[] { "--destPort", "-d" }) { Description = "Port to use as destination" });
        }

        public override async Task<CommandResult> ExecuteAsync(ParseResult parseResult, TeamServerClient client, Agent agent)
        {
            var verb = parseResult.GetValue<string>(VerbArg);
            var port = parseResult.GetValue<int>(PortOpt);
            var destHost = parseResult.GetValue<string>(DestHostOpt);
            var destPort = parseResult.GetValue<int>(DestPortOpt);

            var cmdResult = new CommandResult();

            if (string.IsNullOrEmpty(verb))
            {
                return cmdResult.Failed("Verb is required (start, stop).");
            }

            try
            {
                var parameters = new ParameterDictionary();

                switch (verb.ToLower())
                {
                    case "start":
                        if (port == 0)
                            return cmdResult.Failed("[X] Port is required to start the port forward!");
                        if (string.IsNullOrEmpty(destHost))
                            return cmdResult.Failed("[X] Destination Host is required to start the port forward!");
                        if (destPort == 0)
                            return cmdResult.Failed("[X] Destination Port is required to start the port forward!");

                        parameters.AddParameter(ParameterId.Verb, (byte)CommandVerbs.Start);
                        parameters.AddParameter(ParameterId.Port, port);
                        parameters.AddParameter(ParameterId.Parameters, new ReversePortForwardDestination() { Hostname = destHost, Port = destPort });

                        await client.TaskAgent("rportfwd", agent.Metadata.Id, CommandId.RportFwd, parameters);

                        return cmdResult.Succeed($"[*] RPortFwd tasked to start on port {port} -> {destHost}:{destPort} !");

                    case "stop":
                        if (port == 0)
                            return cmdResult.Failed("[X] Port is required to stop the port forward!");

                        parameters.AddParameter(ParameterId.Verb, (byte)CommandVerbs.Stop);
                        parameters.AddParameter(ParameterId.Port, port);

                        await client.TaskAgent("rportfwd", agent.Metadata.Id, CommandId.RportFwd, parameters);

                        return cmdResult.Succeed($"[*] RPortFwd tasked to stop on port {port} !");

                    case "show":
                        parameters.AddParameter(ParameterId.Verb, (byte)CommandVerbs.Show);

                        await client.TaskAgent("rportfwd", agent.Metadata.Id, CommandId.RportFwd, parameters);

                        // Usually showing results is asynchronous via TaskResult, so we just confirm tasking.
                        return cmdResult.Succeed($"[*] RPortFwd tasked to show !");

                    default:
                        return cmdResult.Failed($"Unknown verb: {verb}. Supported verbs: start, stop, show.");
                }
            }
            catch (Exception ex)
            {
                return cmdResult.Failed($"[Error] {ex.Message}");
            }
        }
    }
}

