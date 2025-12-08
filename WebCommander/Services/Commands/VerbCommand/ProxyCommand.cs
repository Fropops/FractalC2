using System.CommandLine;
using System.Text;
using WebCommander.Models;

namespace WebCommander.Services.Commands.VerbCommand
{
    public class ProxyCommand : ParsedCommand
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Start a Socks4 Proxy on the agent";
        public override string Name => "proxy";
        public override CommandId Id => CommandId.Proxy;

        private const string VerbArg = "verb";
        private const string PortOpt = "port";

        protected override void AddCommandParameters(RootCommand command)
        {

            command.Arguments.Add(new Argument<string>(VerbArg) { Description = "The action to perform (start, stop, show)" });
            
            var portOption = new Option<int>(PortOpt, "--port", "-p") { Description = "Port to use (required for start)" };
            command.Options.Add(portOption);
        }

        public override async Task<CommandResult> ExecuteAsync(ParseResult parseResult, TeamServerClient client, Agent agent)
        {
            var verb = parseResult.GetValue<string>(VerbArg);
            var port = parseResult.GetValue<int>(PortOpt);
            
            var cmdResult = new CommandResult();

            if (string.IsNullOrEmpty(verb))
            {
                return cmdResult.Failed("Verb is required (start, stop, show).");
            }

            try
            {
                switch (verb.ToLower())
                {
                    case "start":
                        if (port == 0) // Assuming 0 means not provided or default. SOCKS usually doesn't run on port 0.
                        {
                            // Check if user actually provided 0 or just didn't provide it.
                            // In this older version, checking for presence might be harder without token inspection.
                            // But 0 is invalid port anyway.
                            return cmdResult.Failed("[X] Port is required to start the proxy (and must be > 0)!");
                        }
                        await client.StartProxyAsync(agent.Metadata.Id, port);
                        return cmdResult.Succeed("[*] Proxy server started !");

                    case "stop":
                        await client.StopProxyAsync(agent.Metadata.Id);
                        return cmdResult.Succeed("[*] Proxy server stopped !");

                    case "show":
                        var proxies = await client.GetProxiesAsync();
                        if (!proxies.Any())
                        {
                            return cmdResult.Succeed("[>] No proxy running!");
                        }

                        var sb = new StringBuilder();
                        sb.AppendLine("Proxies:");
                        sb.AppendLine("========");
                        sb.AppendLine(string.Format("{0,-40} {1,-10}", "Agent", "Port"));
                        sb.AppendLine(new string('-', 51));

                        foreach (var proxy in proxies)
                        {
                            sb.AppendLine(string.Format("{0,-40} {1,-10}", proxy.AgentId, proxy.Port));
                        }

                        return cmdResult.Succeed(sb.ToString());

                    default:
                        return cmdResult.Failed($"Unknown verb: {verb}. Supported verbs: start, stop, show.");
                }
            }
            catch (Exception ex)
            {
                return cmdResult.Failed($"[Error] {ex.Message}");
            }
        }

        // Removed FillParametersAsync as it is not in the base class
    }
}
