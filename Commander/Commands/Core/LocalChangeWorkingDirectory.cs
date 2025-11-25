using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Core
{
    public class ChangeWorkingDirectoryCommandOptions
    {
        public string path { get; set; }
    }

    public class ChangeWorkingDirectoryCommand : EnhancedCommand<ChangeWorkingDirectoryCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Change Commander Working Directory";
        public override string Name => "lcd";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(Description)
            {
                new Argument<string>("path" ,() => string.Empty, "path on the local machine to go into."),
            };


        protected override async Task<bool> HandleCommand(CommandContext<ChangeWorkingDirectoryCommandOptions> context)
        {
            if (!string.IsNullOrEmpty(context.Options.path))
            {
                Directory.SetCurrentDirectory(context.Options.path);
            }

            context.Terminal.WriteLine($"Current working directory = " + Directory.GetCurrentDirectory());
            return true;
        }
    }
}
