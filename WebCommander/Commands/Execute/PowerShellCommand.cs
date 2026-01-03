using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.Execute
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
