using Commander.Executor;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace Commander.Commands.Core
{

    public class PrintWorkingDirectoryCommand : EnhancedCommand
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Print Commander Working Directory";
        public override string Name => "lpwd";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(Description);

        protected override async Task<bool> HandleCommand(CommandContext<EmptyCommandOptions> context)
        {
            context.Terminal.WriteLine($"Current working directory = " + Directory.GetCurrentDirectory());
            return true;
        }
    }
}
