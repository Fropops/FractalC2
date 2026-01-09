using Common.CommandLine.Core;
using System.IO;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class LocalChangeWorkingDirectoryOptions : CommandOption
    {
        [Argument("path", "path on the local machine to go into.",0)]
        public string Path { get; set; }
    }

    [Command("lcd", "Change Commander Working Directory", Category = "Commander")]
    public class LocalChangeWorkingDirectory : ICommand<CommanderCommandContext, LocalChangeWorkingDirectoryOptions>
    {
        public async Task<bool> Execute(CommanderCommandContext context, LocalChangeWorkingDirectoryOptions options)
        {
            if (!string.IsNullOrEmpty(options.Path))
            {
                if(Directory.Exists(options.Path))
                    Directory.SetCurrentDirectory(options.Path);
                else
                    context.Terminal.WriteError($"Directory {options.Path} not found.");
            }

            context.Terminal.WriteLine($"Current working directory = " + Directory.GetCurrentDirectory());
            return true;
        }
    }
}
