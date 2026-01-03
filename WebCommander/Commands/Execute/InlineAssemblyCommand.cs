using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.Execute
{
    public class InlineAssemblyCommand : ExecuteCommand
    {
        public override string Name => "inline-assembly";
        public override string Description => "Execute a dot net assembly in memory";
        public override CommandId Id => CommandId.Assembly;
        public override string Category => CommandCategory.Execution;
    }
}
