using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;
using System.IO;

namespace Common.AgentCommands
{
    public class PowershellImportCommandOptions : CommandOption
    {
        [Argument("ToolScriptName", "Path of the tool script to import", 0, IsRequired = true)]
        public string ToolScriptName { get; set; }
    }

    [Command("powershell-import", "Import a script to be executed while using powershell commands", Category = AgentCommandCategories.Execution)]
    public class PowerShellImportCommand : AgentCommand<PowershellImportCommandOptions>
    {
        public override CommandId CommandId => CommandId.PowershellImport;

        protected override void SpecifyParameters(AgentCommandContext context, PowershellImportCommandOptions options)
        {
            context.AddParameter(ParameterId.Name, Path.GetFileName(options.ToolScriptName));
        }
    }
}
