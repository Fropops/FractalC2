using Common.CommandLine.Core;
using System.IO;
using System.Threading.Tasks;

namespace Commander.Commands
{
    [Command("lpwd", "Print Commander Working Directory", Category = "Commander")]
    public class LocalPrintWorkingDirectory : ICommand<CommanderCommandContext, CommandOption>
    {
        public async Task<bool> Execute(CommanderCommandContext context, CommandOption options)
        {
            context.Terminal.WriteLine($"Current working directory = " + Directory.GetCurrentDirectory());
            return true;
        }
    }
}
