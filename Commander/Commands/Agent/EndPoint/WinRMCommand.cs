using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;
using BinarySerializer;
using System.CommandLine;

namespace Commander.Commands.Agent.EndPoint
{
    public class WinRMCommandOptions
    {
        public string cmd { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string target { get; set; }
    }
    public class WinRMCommand : EndPointCommand<WinRMCommandOptions>
    {
        public override string Description => "Send a command to be executed with winrm to the target";
        public override string Name => "winrm";

        public override CommandId CommandId => CommandId.Winrm;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("target", "Target computer."),
                 new Argument<string>("cmd", "Command to execute"),
                  new Option<string>(new[] { "--user", "-u" }, "username (format : Domain\\user)"),
                  new Option<string>(new[] { "--password", "-p" }, "password"),
            };

        protected override void SpecifyParameters(CommandContext<WinRMCommandOptions> context)
        {
            context.AddParameter(ParameterId.Target, context.Options.target);
            context.AddParameter(ParameterId.Command, context.Options.cmd);
            if (!string.IsNullOrEmpty(context.Options.user))
            {
                var split = context.Options.user.Split('\\');
                var domain = split[0];
                var username = split[1];
                context.AddParameter(ParameterId.User, username);
                context.AddParameter(ParameterId.Domain, domain);
            }

            if (string.IsNullOrEmpty(context.Options.password))
            {
                context.AddParameter(ParameterId.Password, context.Options.password);
            }
        }
    }
}
