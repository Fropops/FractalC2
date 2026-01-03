using System.CommandLine;
using System.CommandLine.Parsing;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.EndPoint
{
    public class RevertSelfCommand : EndPointCommand
    {
        public override string Name => "revert-self";
        public override string Description => "Remove Token Impersonation";
        public override CommandId Id => CommandId.RevertSelf;
        public override string Category => CommandCategory.Token;

        protected override void AddCommandParameters(RootCommand command)
        {
            // No parameters
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            return Task.CompletedTask;
        }
    }
}
