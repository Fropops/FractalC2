using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;
using BinarySerializer;
using System.CommandLine;
using System.IO;

namespace Commander.Commands.Agent.EndPoint
{
    public class PowershellImportCommandOptions
    {
        public string path { get; set; }
    }
    public class PowershellImportCommand : EndPointCommand<PowershellImportCommandOptions>
    {
        public override string Description => "Import a script to be executed whil using powershell commands";
        public override string Name => "powershell-import";

        public override CommandId CommandId => CommandId.PowershellImport;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", () => null, "Path of the file script to import"),
            };

        protected override void SpecifyParameters(CommandContext<PowershellImportCommandOptions> context)
        {
            string script = string.Empty;
            if (!string.IsNullOrEmpty(context.Options.path))
                script = File.ReadAllText(context.Options.path);

            context.AddParameter(ParameterId.File, script);
        }

        protected override async Task<bool> CheckParams(CommandContext<PowershellImportCommandOptions> context)
        {
            if (!File.Exists(context.Options.path))
            {
                context.Terminal.WriteError($"File {context.Options.path} not found");
                return false;
            }
            return await base.CheckParams(context);
        }
    }
}
