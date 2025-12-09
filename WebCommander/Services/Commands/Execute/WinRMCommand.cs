using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class WinRMCommand : EndPointCommand
    {
        public override string Name => "winrm";
        public override string Description => "Send a command to be executed with winrm to the target";
        public override CommandId Id => CommandId.Winrm;
        public override string Category => CommandCategory.LateralMovement;

        private const string ARG_TARGET = "target";
        private const string ARG_CMD = "cmd";
        private const string OPT_USER = "--user";
        private const string OPT_USER_ALIAS = "-u";
        private const string OPT_PASS = "--password";
        private const string OPT_PASS_ALIAS = "-p";
        private const string OPT_PORT = "--port";

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(ARG_TARGET) { Arity = ArgumentArity.ExactlyOne, Description = "Target computer." });
            command.Arguments.Add(new Argument<string>(ARG_CMD) {Arity = ArgumentArity.ExactlyOne,Description = "Command to execute"});
            command.Options.Add(new Option<string>(OPT_USER, OPT_USER_ALIAS ) {Arity = ArgumentArity.ZeroOrOne,Description = "Username format DOMAIN\\User"});
            command.Options.Add(new Option<string>(OPT_PASS, OPT_PASS_ALIAS ) {Arity = ArgumentArity.ZeroOrOne,Description = "Password"});
            command.Options.Add(new Option<int>(OPT_PORT) {Arity = ArgumentArity.ZeroOrOne, Description = "Port number"});
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            var target = parseResult.GetValue<string>(ARG_TARGET);
            var cmd = parseResult.GetValue<string>(ARG_CMD);
            var user = parseResult.GetValue<string>(OPT_USER);
            var password = parseResult.GetValue<string>(OPT_PASS);

            parms.AddParameter(ParameterId.Target, target);
            parms.AddParameter(ParameterId.Command, cmd);

            if (!string.IsNullOrEmpty(user))
            {
                if (user.Contains("\\"))
                {
                    var split = user.Split('\\');
                    var domain = split[0];
                    var username = split[1];
                    parms.AddParameter(ParameterId.User, username);
                    parms.AddParameter(ParameterId.Domain, domain);
                }
                else
                {
                     parms.AddParameter(ParameterId.User, user);
                }
            }

            if (!string.IsNullOrEmpty(password))
            {
                parms.AddParameter(ParameterId.Password, password);
            }

            var port = parseResult.GetValue<int>(OPT_PORT);
            if (port != 0)
            {
                 parms.AddParameter(ParameterId.Port, port);
         
            }
        }
    }
}
