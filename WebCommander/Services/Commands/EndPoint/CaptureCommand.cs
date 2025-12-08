using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class CaptureCommand : EndPointCommand
    {
        public override string Name => "capture";
        public override string Description => "Capture current screen(s).";
        public override CommandId Id => CommandId.Capture;
        public override string Category => CommandCategory.Media;

        protected override void AddCommandParameters(RootCommand command)
        {
            // Capture command has no parameters
        }

        public override Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            // No parameters to fill
            return Task.CompletedTask;
        }
    }
}
