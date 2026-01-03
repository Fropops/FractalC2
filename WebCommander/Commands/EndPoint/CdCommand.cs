using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.EndPoint
{
    public class CdCommand : EndPointCommand
    {
        public override string Name => "cd";
        public override string Description => "Change the current working directory.";
        public override CommandId Id => CommandId.Cd;
        public override string[] Aliases => new[] { "chdir" };
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) 
            { 
                Arity = ArgumentArity.ExactlyOne,
                Description = "Path of the directory to change to"
            });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            var path = parseResult.GetValue<string>(ParameterId.Path.ToString());
            if (!string.IsNullOrEmpty(path))
                parms.AddParameter(ParameterId.Path, path);
        }
    }
}
