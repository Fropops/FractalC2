using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.APIModels;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Core;

namespace Commander.Commands.Core
{
    public class ToolsCommandOptions : VerbAwareCommandOptions
    {
        public string type { get; set; }
        public string name { get; set; }
        public string path { get; set; }
    }

    public class ToolsCommand : VerbAwareCommand<ToolsCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Manage Tools";
        public override string Name => "tool";
        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override string[] Alternate => new string[] { "tools" };

        public override RootCommand Command
        {
            get
            {
                var root = new RootCommand(Description);
                root.AddArgument(new Argument<string>("verb", () => "show", "show, add").FromAmong("show", "add"));
                root.AddArgument(new Argument<string>("path", () => null, "File Path (add)")); 
                root.AddOption(new Option<string>(new[] { "--type", "-t" }, "Tool Type (list)"));
                root.AddOption(new Option<string>(new[] { "--name", "-n" }, "Tool Name (list)"));
                return root;
            }
        }

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register("show", this.List);
            this.Register("add", this.Add);
        }

        private async Task<bool> List(CommandContext<ToolsCommandOptions> context)
        {
            // Parse type option
            ToolType? type = null;
            if (!string.IsNullOrEmpty(context.Options.type))
            {
                if (int.TryParse(context.Options.type, out int typeInt))
                    type = (ToolType)typeInt;
                else if (Enum.TryParse<ToolType>(context.Options.type, true, out var typeEnum))
                    type = typeEnum;
                else
                {
                    context.Terminal.WriteError($"Invalid tool type: {context.Options.type}");
                    return false;
                }
            }

            try
            {
                var tools = await context.CommModule.GetTools(type, context.Options.name);
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

        private async Task<bool> Add(CommandContext<ToolsCommandOptions> context)
        {
            var path = context.Options.path;
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
