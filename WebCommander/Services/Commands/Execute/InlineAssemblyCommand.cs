using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class InlineAssemblyCommand : ExecuteCommand
    {
        public override string Name => "inline-assembly";
        public override string Description => "Execute a dot net assembly in memory";
        public override CommandId Id => CommandId.Assembly;
        public override string Category => CommandCategory.Execution;
    }
}
