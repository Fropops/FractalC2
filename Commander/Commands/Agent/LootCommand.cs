using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commander.Commands.Core;
using Commander.Executor;
using Commander.Models;
using Common.APIModels;
using Shared;
using Spectre.Console;
using static System.Net.Mime.MediaTypeNames;

namespace Commander.Commands.Agent
{
    public class LootCommandOptions : VerbAwareCommandOptions
    {
        public string target { get; set; } // fileName or filePath
    }

    public class LootCommand : VerbAwareCommand<LootCommandOptions>
    {
        public override string Category => CommandCategory.Agent;
        public override string Description => "Manage Loot";
        public override string Name => "loot";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command
        {
            get
            {
                var root = new RootCommand(Description);
                root.AddArgument(new Argument<string>("verb", () => "show", "show, download, upload, delete").FromAmong("show", "download", "upload", "delete"));
                root.AddArgument(new Argument<string>("target", () => null, "File Name (download) or File Path (upload)")); // Optional for show

                return root;
            }
        }

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register("show", this.Show);
            this.Register("uplaod", this.Upload);
            this.Register("download", this.Download);
            this.Register("delete", this.Delete);
        }

      

        private async Task<bool> Show(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Executor.CurrentAgent.Id;
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

                foreach (var item in lootList)
                {
                    table.AddRow(item.FileName ?? "", item.IsImage.ToString());
                }

                context.Terminal.Write(table);
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Error fetching loot: {ex.Message}");
            }
            return true;
        }

        private async Task<bool> Download(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Executor.CurrentAgent.Id;
            var fileName = context.Options.target;

            if (string.IsNullOrEmpty(fileName))
            {
                context.Terminal.WriteError("File Name is required.");
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

        private async Task<bool> Upload(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Executor.CurrentAgent.Id;
            var filePath = context.Options.target;
            if (string.IsNullOrEmpty(filePath))
            {
                context.Terminal.WriteError("File Path is required.");
                return false;
            }

            if (!File.Exists(filePath))
            {
                context.Terminal.WriteError("File not found.");
                return false;
            }

            var fileName = Path.GetFileName(filePath);
            try
            {
                var data = File.ReadAllBytes(filePath);
                var loot = new Loot()
                {
                    AgentId = agentId,
                    Data = Convert.ToBase64String(data),
                    FileName = fileName,
                    IsImage = IsImageFile(fileName)
                };
                await context.CommModule.CreateLootAsync(agentId, loot);
                context.Terminal.WriteSuccess($"Loot {fileName} uploaded !");
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Error uploading loot: {ex.Message}");
            }

            return true;

        }

        private async Task<bool> Delete(CommandContext<LootCommandOptions> context)
        {
            var agentId = context.Executor.CurrentAgent.Id;
            var fileName = context.Options.target;
            if (string.IsNullOrEmpty(fileName))
            {
                context.Terminal.WriteError("File Name is required.");
                return false;
            }

            if (!File.Exists(fileName))
            {
                context.Terminal.WriteError("File not found.");
                return false;
            }

            try
            {
                await context.CommModule.DeleteLoot(agentId, fileName);
                context.Terminal.WriteSuccess($"Loot {fileName} deleted !");
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Error deleting loot: {ex.Message}");
            }

            return true;

        }

        private bool IsImageFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" }.Contains(ext);
        }

    }
}
