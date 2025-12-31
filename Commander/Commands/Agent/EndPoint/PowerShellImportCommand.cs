using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using Commander.Executor;
using Shared;
using Spectre.Console;

namespace Commander.Commands.Agent.EndPoint
{
    public class PowershellImportCommandOptions
    {
        public string path { get; set; }
    }
    public class PowershellImportCommand : EndPointCommand<PowershellImportCommandOptions>
    {
        public override string Description => "Import a script to be executed whil using powershell commands";
        public override string Category => CommandCategory.Execution;
        public override string Name => "powershell-import";
        public override Shared.OsType[] SupportedOs => new[] { Shared.OsType.Windows };
        public override CommandId CommandId => CommandId.PowershellImport;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", () => null, "Path of the file script to import"),
            };

        protected override void SpecifyParameters(CommandContext<PowershellImportCommandOptions> context)
        {
            context.AddParameter(ParameterId.Name, Path.GetFileName(context.Options.path));
        }
    }
}
