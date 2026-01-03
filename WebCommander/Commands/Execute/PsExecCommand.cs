using System.CommandLine;
using System.CommandLine.Parsing;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.Execute
{
    public class PsExecCommand : EndPointCommand
    {
        public override string Name => "psexec";
        public override string Description => "Send a path to be run as remote service";
        public override CommandId Id => CommandId.PsExec;
        public override string Category => CommandCategory.LateralMovement;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Add(new Argument<string>("target") { Description = "Target computer", Arity = ArgumentArity.ExactlyOne });
            command.Add(new Argument<string>("path") { Description = "Path of the service to start", Arity = ArgumentArity.ExactlyOne });
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            var target = result.GetValue<string>("target");
            var path = result.GetValue<string>("path");
            
            parms.AddParameter(ParameterId.Target, target);
            parms.AddParameter(ParameterId.Path, path);
            
            return Task.CompletedTask;
        }
    }
}
