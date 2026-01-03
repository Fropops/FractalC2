using System.CommandLine;
using System.CommandLine.Parsing;
using Shared;
using WebCommander.Commands;
using WebCommander.Models;

namespace WebCommander.Commands.EndPoint
{
    public class MakeTokenCommand : EndPointCommand
    {
        public override string Name => "make-token";
        public override string Description => "Make token for a specified user";
        public override CommandId Id => CommandId.MakeToken;
        public override string Category => CommandCategory.Token;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Add(new Option<string>("--username", "-u") { Description = "Full username (DOMAIN\\User)" });
            command.Add(new Argument<string>("password") { Description = "Password of the account" });
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            var username = result.GetValue<string>("--username");
            var password = result.GetValue<string>("password");
            
            if (string.IsNullOrEmpty(username) || !username.Contains('\\'))
            {
                throw new ArgumentException("Username is not in a correct format (DOMAIN\\User).");
            }

            var split = username.Split('\\');
            var domain = split[0];
            var user = split[1];

            parms.AddParameter(ParameterId.User, user);
            parms.AddParameter(ParameterId.Domain, domain);
            parms.AddParameter(ParameterId.Password, password);
            
            return Task.CompletedTask;
        }
    }
}
