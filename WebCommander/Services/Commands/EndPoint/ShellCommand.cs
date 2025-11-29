using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class ShellCommand : NonParsedCommand
    {
        public override string Name => "shell";
        public override string Description => "Send a command to be executed by the agent shell";
        public override CommandId Id => CommandId.Shell;
    }
}
