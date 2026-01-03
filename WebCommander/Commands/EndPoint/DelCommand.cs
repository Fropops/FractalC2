using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.EndPoint
{
    public class DelCommand : EndPointCommand
    {
        public override string Name => "del";
        public override string Description => "Delete a file on the agent.";
        public override CommandId Id => CommandId.Del;
        public override string[] Aliases => new[] { "rm", "del" };
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) 
            { 
                Arity = ArgumentArity.ExactlyOne,
                Description = "Path of the file to delete"
            });
        }

        public override Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            var path = parseResult.GetValue<string>(ParameterId.Path.ToString());
            if (!string.IsNullOrEmpty(path))
                parms.AddParameter(ParameterId.Path, path);
            
            return Task.CompletedTask;
        }
    }
}
