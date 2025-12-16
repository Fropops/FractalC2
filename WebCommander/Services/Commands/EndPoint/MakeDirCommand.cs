using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class MakeDirCommand : EndPointCommand
    {
        public override string Name => "mkdir";
        public override string[] Aliases => new[] { "mkdir", "md" };
        public override OsType[] SupportedOs => new[] { OsType.Windows, OsType.Linux };
        public override string Description => "Create a folder on the agent.";
        public override CommandId Id => CommandId.MkDir;
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ParameterId.Path.ToString()) 
            { 
                Arity = ArgumentArity.ExactlyOne,
                Description = "Path of the folder to create"
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
