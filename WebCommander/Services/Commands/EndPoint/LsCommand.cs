using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class LsCommand : EndPointCommand
    {
        public override string Name => "ls";
        public override string Description => "List directory contents";
        public override CommandId Id => CommandId.Ls;
        public override string[] Aliases => new[] { "dir" };

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) { Arity = ArgumentArity.ZeroOrOne });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            parms.AddParameter(ParameterId.Path, parseResult.GetValue<string>(ParameterId.Path.ToString()));
        }
    }
}
