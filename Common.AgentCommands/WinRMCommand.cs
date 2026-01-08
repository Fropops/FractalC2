using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class WinRMCommandOptions : CommandOption
    {
        [Argument("target", "Target computer.", 0, IsRequired = true)]
        public string Target { get; set; }

        [Argument("cmd", "Command to execute", 1, IsRequired = true)]
        public string Cmd { get; set; }

        [Option("-u", "user", "username (format : Domain\\user)")]
        public string User { get; set; }

        [Option("-p", "password", "password")]
        public string Password { get; set; }
    }

    [Command("winrm", "Send a command to be executed with winrm to the target", Category = AgentCommandCategories.LateralMovement)]
    public class WinRMCommand : AgentCommand<WinRMCommandOptions>
    {
        public override CommandId CommandId => CommandId.Winrm;

        protected override void SpecifyParameters(AgentCommandContext context, WinRMCommandOptions options)
        {
            context.AddParameter(ParameterId.Target, options.Target);
            context.AddParameter(ParameterId.Command, options.Cmd);
            if (!string.IsNullOrEmpty(options.User))
            {
                var split = options.User.Split('\\');
                if (split.Length > 1) 
                {
                    context.AddParameter(ParameterId.Domain, split[0]);
                    context.AddParameter(ParameterId.User, split[1]);
                }
                else
                {
                     // Fallback if no domain? Original code assumed it has split[1].
                     context.AddParameter(ParameterId.User, options.User);
                }
            }

             if (!string.IsNullOrEmpty(options.Password))
            {
                context.AddParameter(ParameterId.Password, options.Password);
            }
        }
    }
}
