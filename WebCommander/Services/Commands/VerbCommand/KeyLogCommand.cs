using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class KeyLogCommand : VerbAwareCommand
    {
        public override string Name => "keylog";
        public override string Description => "Log keys on the agent";
        public override CommandId Id => CommandId.KeyLog;
        override protected List<CommandVerbs> AllowedVerbs => new List<CommandVerbs> { CommandVerbs.Show, CommandVerbs.Start, CommandVerbs.Stop };

        protected override void AddCommandParameters(RootCommand command)
        {
            base.AddCommandParameters(command);
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            await base.FillParametersAsync(parseResult, parms);
        }
    }
}
