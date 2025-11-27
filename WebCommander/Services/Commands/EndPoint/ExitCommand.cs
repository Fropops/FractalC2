using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class ExitCommand : EndPointCommand
    {
        public override string Name => "destroy";
        public override string Description => "Ask an agent to exit.";
        public override CommandId Id => CommandId.Exit;
        public override string[] Aliases => new[] { "exit", "quit" };

        protected override void AddCommandParameters(RootCommand command)
        {
            // Exit command has no parameters
        }

        public override Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            // No parameters to fill
            return Task.CompletedTask;
        }
    }
}
