using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class CheckinCommand : EndPointCommand
    {
        public override string Name => "checkin";
        public override string Description => "Force agent to update its metadata.";
        public override CommandId Id => CommandId.CheckIn;

        protected override void AddCommandParameters(RootCommand command)
        {
            // Checkin command has no parameters
        }

        public override Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            // No parameters to fill
            return Task.CompletedTask;
        }
    }
}
