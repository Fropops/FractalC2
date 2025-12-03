using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class PowerShellCommand : NonParsedCommand
    {
        public override string Name => "powershell";
        public override string Description => "Send a command to be executed by the agent powershell";
        public override CommandId Id => CommandId.Powershell;
        public override string[] Aliases => new[] { "powerpick" };
        public override string Category => CommandCategory.Execution;
    }
}
