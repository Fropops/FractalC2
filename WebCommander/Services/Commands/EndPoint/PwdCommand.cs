using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class PwdCommand : EndPointCommand
    {
        public override string Name => "pwd";
        public override string[] Aliases => new[] { "pwd" };
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Description => "Print the current working directory.";
        public override CommandId Id => CommandId.Pwd;
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            // Pwd command has no parameters
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            // No parameters to fill
            await Task.CompletedTask;
        }
    }
}
