using Common.CommandLine.Core;
using Spectre.Console;
using System.IO;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class LocalListDirectoryCommandOptions : CommandOption
    {
        [Argument("path", "directory to list", 0)]
        public string Path { get; set; }
    }

    [Command("lls", "List Directory", Category = "Commander")]
    public class LocalListDirectoryCommand : ICommand<CommanderCommandContext, LocalListDirectoryCommandOptions>
    {
        public async Task<bool> Execute(CommanderCommandContext context, LocalListDirectoryCommandOptions options)
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("Name").LeftAligned());
            table.AddColumn(new TableColumn("Length").LeftAligned());
            table.AddColumn(new TableColumn("IsFile").LeftAligned());

            string path = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(options.Path))
            {
                if (Directory.Exists(options.Path))
                    path = options.Path;
                else
                {
                    context.Terminal.WriteError($"Directory {options.Path} not found.");
                    return false;
                }
            }

            try
            {
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    table.AddRow(
                        dirInfo.Name,
                        "0",
                        "No"
                    );
                }

                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    table.AddRow(
                        Path.GetFileName(fileInfo.FullName),
                        fileInfo.Length.ToString(),
                        "Yes"
                    );
                }

                context.Terminal.Write(table);
            }
            catch (System.Exception ex)
            {
                context.Terminal.WriteError($"Error listing directory: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
