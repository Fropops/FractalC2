using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.EndPoint
{
    public class LsCommand : EndPointCommand
    {
        public override string Name => "ls";
        public override string Description => "List directory contents";
        public override CommandId Id => CommandId.Ls;
        public override string[] Aliases => new[] { "ls", "dir" };
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) { Arity = ArgumentArity.ZeroOrOne, Description = "Path of the directory to list" });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            parms.AddParameter(ParameterId.Path, parseResult.GetValue<string>(ParameterId.Path.ToString()));
        }
    }
}
