using System.CommandLine;
using System.CommandLine.Parsing;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class RmDirCommand : EndPointCommand
    {
        public override string Name => "rmdir";
        public override string Description => "Delete a folder on the agent";
        public override CommandId Id => CommandId.RmDir;
        public override string Category => CommandCategory.System;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Add(new Argument<string>("path") { Description = "Path of the folder to delete" });
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            var path = result.GetValue<string>("path");
            parms.AddParameter(ParameterId.Path, path);
            return Task.CompletedTask;
        }
    }
}
