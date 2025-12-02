using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class ExecutePECommand : ExecuteCommand
    {
        public override string Name => "execute-pe";
        public override string Description => "Execute a PE assembly with Fork And Run mechanism";
        public override CommandId Id => CommandId.ForkAndRun;
        public override string Category => CommandCategory.Execution;
    }
}
