using Commander.CommanderCommand.Abstract;
using Common.APIModels;
using Common.CommandLine.Core;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.CommanderCommand
{
    public class ManageToolsCommandOptions : VerbCommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = "show", AllowedValues = new object[] { "show", "add" }, IsRequired = true)]
        public override string verb { get; set; }

        [Option("type", "t", "Tool Type (list)")]
        public string type { get; set; }

        [Option("name", "n", "Tool Name (list)")]
        public string name { get; set; }

        [Option("path", "p", "File Path (add)")]
        public string path { get; set; }
    }

    [Command("tool", "Manage Tools", Category = "Commander", Aliases = new string[] { "tools" })]
    public class ManageToolsCommand : VerbCommand<CommanderCommandContext, ManageToolsCommandOptions>
    {
        protected override void RegisterVerbs()
        {
            Register("show", List);
            Register("add", Add);
        }

        private async Task<bool> List(CommanderCommandContext context, ManageToolsCommandOptions options)
        {
            // Parse type option
            ToolType? type = null;
            if (!string.IsNullOrEmpty(options.type))
            {
                if (int.TryParse(options.type, out int typeInt))
                    type = (ToolType)typeInt;
                else if (Enum.TryParse<ToolType>(options.type, true, out var typeEnum))
                    type = typeEnum;
                else
                {
                    context.Terminal.WriteError($"Invalid tool type: {options.type}");
                    return false;
                }
            }

            try
            {
                var tools = await context.CommModule.GetTools(type, options.name);
                if (tools == null || !tools.Any())
                {
                    context.Terminal.WriteInfo("No tools found.");
                    return true;
                }

                var table = new Table();
                table.AddColumn("Name");
                table.AddColumn("Type");

                foreach (var tool in tools)
                {
                    table.AddRow(tool.Name ?? "", tool.Type.ToString());
                }

                context.Terminal.Write(table);
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Error listing tools: {ex.Message}");
            }

            return true;
        }

        private async Task<bool> Add(CommanderCommandContext context, ManageToolsCommandOptions options)
        {
            var path = options.path;
            if (string.IsNullOrEmpty(path))
            {
                context.Terminal.WriteError("File path is required for 'add'.");
                return false;
            }

            try
            {
                await context.CommModule.AddTool(path);
                context.Terminal.WriteSuccess($"Tool {Path.GetFileName(path)} added successfully.");
            }
            catch (Exception ex)
            {
                context.Terminal.WriteError($"Error adding tool: {ex.Message}");
            }

            return true;
        }
    }
}
