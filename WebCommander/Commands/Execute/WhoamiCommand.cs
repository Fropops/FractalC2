using System.CommandLine;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.Execute
{
    public class WhoamiCommand : EndPointCommand
    {
        public override string Name => "whoami";
        public override string Description => "Get the current user name.";
        public override CommandId Id => CommandId.Whoami;
        public override string Category => CommandCategory.System;
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };

        protected override void AddCommandParameters(RootCommand command)
        {
            // No parameters
        }

        public override Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            return Task.CompletedTask;
        }
    }
}
