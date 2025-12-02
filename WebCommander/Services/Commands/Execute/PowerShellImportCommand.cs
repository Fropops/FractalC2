using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class PowerShellImportCommand : EndPointCommand
    {
        public override string Name => "powershell-import";
        public override string Description => "Import a script to be executed whil using powershell commands";
        public override CommandId Id => CommandId.PowershellImport;
        public override string Category => CommandCategory.Execution;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Name.ToString()) { Arity = ArgumentArity.ZeroOrOne });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            parms.AddParameter(ParameterId.Name, parseResult.GetValue<string>(ParameterId.Name.ToString()));
        }
    }
}
