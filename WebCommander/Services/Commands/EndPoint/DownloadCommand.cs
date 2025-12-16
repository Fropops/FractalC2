using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class DownloadCommand : EndPointCommand
    {
        public override string Name => "download";
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Description => "Download a file";
        public override CommandId Id => CommandId.Download;
        public override string[] Aliases => new[] { "dl" };
        public override string Category => CommandCategory.Network;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) { Arity = ArgumentArity.ExactlyOne, Description = "Path of the file to download" });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            parms.AddParameter(ParameterId.Path, parseResult.GetValue<string>(ParameterId.Path.ToString()));
        }
    }
}
