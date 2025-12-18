using Commander.Executor;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Common.Payload;
using System.Linq;
using System;
using System.IO;
using Commander.Models;
using System.Collections.Generic;
using Shared;
using Spectre.Console;
using Common;

namespace Commander.Commands.Core
{
    public class ImplantCommandOptions
    {
        public string action { get; set; } // show, generate, download, delete
        public string id { get; set; } // for delete
        
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

        public override RootCommand Command
        {
            get
            {
                var root = new RootCommand(Description);
                root.AddArgument(new Argument<string>("action", () => "show", "Action to perform: show, generate, download, delete").FromAmong("show", "generate", "download", "delete"));
                
                // Generated options
                root.AddOption(new Option<string>(new[] { "--listener", "-l" }, "Listener to connect to (for generate)"));
                root.AddOption(new Option<string>(new[] { "--endpoint", "-b" }, "Endpoint URL (for generate, e.g. http://10.10.10.10:80)"));
                root.AddOption(new Option<bool>(new[] { "--download", "-d" }, "Download implant after generation"));
                root.AddOption(new Option<string>(new[] { "--type", "-t" }, () => "exe", "exe | dll | rfl | svc | ps | bin | elf").FromAmong("exe", "dll", "rfl" , "svc", "ps", "bin", "elf"));
                root.AddOption(new Option<string>(new[] { "--arch", "-a" }, () => "x64", "x64 | x86").FromAmong("x64", "x86"));

                // Delete options
                root.AddOption(new Option<string>(new[] { "--id", "-i" }, "Implant ID (for delete)"));

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
            table.AddColumn(new TableColumn("ID"));
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

               table.AddRow(implant.Id, name, type, arch, endpoint, listener);
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

            if (Enum.TryParse<ImplantType>(typeStr, true, out var typeEnum))
                config.Type = typeEnum;
            else
                config.Type = ImplantType.Executable;

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
                    context.Options.id = result.Id;
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
            var id = context.Options.id;
            if (string.IsNullOrEmpty(id))
            {
                context.Terminal.WriteError("Implant ID is required for download.");
                return false;
            }

            try
            {
                context.Terminal.WriteInfo($"Downloading implant {id}...");
                var implant = await context.CommModule.GetImplantBinary(id);
                
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
            var id = context.Options.id;
            if (string.IsNullOrEmpty(id))
            {
                context.Terminal.WriteError("Implant ID is required for deletion.");
                return false;
            }

            await context.CommModule.DeleteImplant(id);
            context.Terminal.WriteSuccess($"Implant {id} deleted.");
            return true;
        }
    }
}
