using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Shared;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class CatCommand : EndPointCommand
    {
        public override string Name => "cat";
        public override string[] Aliases => new[] { "cat", "type" };
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Description => "Display the content of a file.";
        public override CommandId Id => CommandId.Cat;
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) 
            { 
                Arity = ArgumentArity.ExactlyOne,
                Description = "Path of the file to display"
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
