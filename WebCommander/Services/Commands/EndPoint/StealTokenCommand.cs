using System.CommandLine;
using System.CommandLine.Parsing;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class StealTokenCommand : EndPointCommand
    {
        public override string Name => "steal-token";
        public override string Description => "Steal the token from a process";
        public override CommandId Id => CommandId.StealToken;
        public override string Category => CommandCategory.Token;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Add(new Argument<int>("pid") { Description = "Id of the process", Arity = ArgumentArity.ExactlyOne });
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            var pid = result.GetValue<int>("pid");
            parms.AddParameter(ParameterId.Id, pid);
            return Task.CompletedTask;
        }
    }
}
