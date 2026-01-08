using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class MakeTokenCommandOptions : CommandOption
    {
        [Argument("username", "Full username (DOMAIN\\User)",0)]
        public string Username { get; set; }

        [Argument("password", "Password of the account", 1)]
        public string Password { get; set; }
    }

    [Command("make-token", "Make token for a specified user", Category = AgentCommandCategories.Token)]
    public class MakeTokenCommand : AgentCommand<MakeTokenCommandOptions>
    {
        public override CommandId CommandId => CommandId.MakeToken;

        protected override bool CheckParams(AgentCommandContext context, MakeTokenCommandOptions options)
        {
             if (string.IsNullOrEmpty(options.Username) || !options.Username.Contains('\\'))
            {
                context.WriteError($"Username is not in a correct format.");
                return false;
            }
            return base.CheckParams(context, options);
        }

        protected override void SpecifyParameters(AgentCommandContext context, MakeTokenCommandOptions options)
        {
            var split = options.Username.Split('\\');
            var domain = split[0];
            var username = split[1];
            context.AddParameter(ParameterId.User, username);
            context.AddParameter(ParameterId.Domain, domain);
            context.AddParameter(ParameterId.Password, options.Password);
        }
    }
}
