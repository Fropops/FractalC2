using System.CommandLine;
using System.CommandLine.Parsing;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.EndPoint
{
    public class PsCommand : EndPointCommand
    {
        public override string Name => "ps";
        public override string Description => "List processes";
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override CommandId Id => CommandId.ListProcess;
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Add(new Argument<string>("process") { Description = "process name to search for", Arity = ArgumentArity.ZeroOrOne });
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            var process = result.GetValue<string>("process");
            if (!string.IsNullOrEmpty(process))
                parms.AddParameter(ParameterId.Path, process);
            
            return Task.CompletedTask;
        }
    }
}
