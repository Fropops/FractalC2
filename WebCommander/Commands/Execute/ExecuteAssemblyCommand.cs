using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.Execute
{
    public class ExecuteAssemblyCommand : ExecuteCommand
    {
        public override string Name => "execute-assembly";
        public override string Description => "Execute a .Net assembly with Fork And Run mechanism";
        public override CommandId Id => CommandId.ForkAndRun;
        public override string Category => CommandCategory.Execution;
    }
}
