using System.Text;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class HelpCommand : NonParsedCommand
    {
        public override string Name => "help";
        public override string Description => "List all available commands";
        public override CommandId Id => CommandId.None;
        public override string Category => CommandCategory.UI;

        public override async Task<CommandResult> ExecuteAsync(string cmdLine)
        {
            var args = WebCommander.Helpers.CommandsHelper.GetArgs(cmdLine);
            if (args.Length > 1)
            {
                var cmdName = args[1];
                var cmd = _commandService.GetCommands().FirstOrDefault(c => c.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase) || c.Aliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase));
                
                if (cmd != null)
                {
                    var sbCmd = new StringBuilder();
                    sbCmd.AppendLine($"Command: {cmd.Name}");
                    sbCmd.AppendLine($"Description: {cmd.Description}");
                    sbCmd.AppendLine(cmd.GetUsage());
                    if (cmd.Aliases.Any())
                    {
                        sbCmd.AppendLine($"Aliases: {string.Join(", ", cmd.Aliases)}");
                    }
                    return new CommandResult().Succeed(sbCmd.ToString());
                }
                else
                {
                    return new CommandResult().Failed($"Command '{cmdName}' not found.");
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("Available Commands:");
            sb.AppendLine("===================");

            var commands = _commandService.GetCommands().Where(c => c.Name != "help" && (_agent == null || c.SupportedOs.Contains(_agent.Metadata.OsType)));
            var groupedCommands = commands.GroupBy(c => c.Category).OrderBy(g => g.Key);

            foreach (var group in groupedCommands)
            {
                sb.AppendLine();
                sb.AppendLine($"[{group.Key}]");
                foreach (var cmd in group.OrderBy(c => c.Name))
                {
                    sb.AppendLine($"  {cmd.Name,-20} {cmd.Description}");
                }
            }

            return new CommandResult().Succeed(sb.ToString());
        }
    }
}
