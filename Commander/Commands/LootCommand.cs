using Commander.Executor;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Common.APIModels;
using System.Linq;
using System;
using System.IO;
using Commander.Models;
using System.Collections.Generic;
using Spectre.Console;

namespace Commander.Commands
{
    public class LootCommandOptions
    {
        public string action { get; set; } // show, download, upload
        public string agentId { get; set; }
        public string target { get; set; } // fileName or filePath
    }

    public class LootCommand : EnhancedCommand<LootCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Manage Loot";
        public override string Name => "loot";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command
        {
            get
            {
                var root = new RootCommand(Description);
                root.AddArgument(new Argument<string>("action", "show, download, upload").FromAmong("show", "download", "upload"));
                root.AddArgument(new Argument<string>("agentId", "Agent ID"));
                root.AddArgument(new Argument<string>("target", () => null, "File Name (download) or File Path (upload)")); // Optional for show

                return root;
            }
        }

        protected override async Task<bool> HandleCommand(CommandContext<LootCommandOptions> context)
        {
            switch (context.Options.action.ToLower())
            {
                case "show":
                    return await this.ShowLoot(context);
                case "download":
                    return await this.DownloadLoot(context);
                case "upload":
                    return await this.UploadLoot(context);
                default:
                    context.Terminal.WriteError($"Unknown action: {context.Options.action}");
                    return false;
            }
        }

        private async Task<bool> ShowLoot(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Options.agentId;
            // target ignored
            
            if (string.IsNullOrEmpty(agentId))
            {
                context.Terminal.WriteError("Agent ID is required.");
                return false;
            }

            try
            {
                var lootList = await context.CommModule.GetLoot(agentId);
                if (lootList == null || !lootList.Any())
                {
                    context.Terminal.WriteInfo("No loot found.");
                    return true;
                }

                var table = new Spectre.Console.Table();
                table.AddColumn(new TableColumn("File Name"));
                table.AddColumn(new TableColumn("Is Image"));
                table.AddColumn(new TableColumn("Has Data"));

                foreach (var item in lootList)
                {
                    table.AddRow(item.FileName ?? "", item.IsImage.ToString(), string.IsNullOrEmpty(item.Data) ? "No" : "Yes");
                }

                context.Terminal.Write(table);
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Error fetching loot: {ex.Message}");
            }
            return true;
        }

        private async Task<bool> DownloadLoot(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Options.agentId;
            var fileName = context.Options.target;

            if (string.IsNullOrEmpty(agentId) || string.IsNullOrEmpty(fileName))
            {
                context.Terminal.WriteError("Agent ID and File Name are required.");
                return false;
            }

            try
            {
                var loot = await context.CommModule.GetLootFile(agentId, fileName);
                if (loot == null || string.IsNullOrEmpty(loot.Data))
                {
                     context.Terminal.WriteError("Loot data not found.");
                     return false;
                }
                
                var data = Convert.FromBase64String(loot.Data);
                var outPath = Path.Combine(Environment.CurrentDirectory, "Loot", agentId, fileName);
                var dir = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                await File.WriteAllBytesAsync(outPath, data);
                context.Terminal.WriteSuccess($"Loot saved to {outPath}");
            }
            catch (Exception ex)
            {
                 context.Terminal.WriteError($"Error downloading loot: {ex.Message}");
            }

            return true;
        }

         private async Task<bool> UploadLoot(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Options.agentId;
            var filePath = context.Options.target;
             if (string.IsNullOrEmpty(agentId) || string.IsNullOrEmpty(filePath))
            {
                context.Terminal.WriteError("Agent ID and File Path are required.");
                return false;
            }
            context.Terminal.WriteInfo("Upload feature pending API verification.");
            return true;
        }

    }
}
