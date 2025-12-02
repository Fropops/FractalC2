using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class DownloadCommand : EndPointCommand
    {
        public override string Name => "download";
        public override string Description => "Download a file";
        public override CommandId Id => CommandId.Download;
        public override string[] Aliases => new[] { "dl" };
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) { Arity = ArgumentArity.ExactlyOne });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            parms.AddParameter(ParameterId.Path, parseResult.GetValue<string>(ParameterId.Path.ToString()));
        }
    }
}
