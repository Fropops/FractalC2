using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    // Reusing AgentShellCommandOption as it captures everything.
    [Command("startas", "Start an executable, without capturing output, as another user", Category = AgentCommandCategories.Execution)]
    public class StartAsCommand : AgentCommand<AgentShellCommandOption>
    {
        public override CommandId CommandId => CommandId.StartAs;

        protected override bool CheckParams(AgentCommandContext context, AgentShellCommandOption options)
        {
             // Expected format: domain\user password executable [args]
             // options.RawArgs contains the full string.
             if (string.IsNullOrEmpty(options.RawArgs))
             {
                 context.WriteError($"Usage : startas domain\\user password executable [args]");
                 return false;
             }
             
             // Simple naive split - mimic original logic "GetArgs()"
             // This split might be too simple regarding quotes etc, but matches original intent if GetArgs() was simple.
             // Original used `context.CommandParameters.GetArgs()`. System.CommandLine parsing might have already tokenized it?
             // No, AgentShellCommandOption uses IsRemainder=true, so it gets the rest of the line as one string.
             
             var args = options.RawArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
             
             if (args.Length < 3 || !args[0].Contains('\\'))
            {
                context.WriteError($"Usage : startas domain\\user password executable [args]");
                return false;
            }
            return base.CheckParams(context, options);
        }

        protected override void SpecifyParameters(AgentCommandContext context, AgentShellCommandOption options)
        {
             var args = options.RawArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            var splitUser = args[0].Split('\\');
            var domain = splitUser[0];
            var username = splitUser[1];
            var password = args[1];
            
            context.AddParameter(ParameterId.User, username);
            context.AddParameter(ParameterId.Domain, domain);
            context.AddParameter(ParameterId.Password, password);
             
            // Extract command: everything after password.
            // Reconstruct cmd string.
            // Original: `context.CommandParameters.ExtractAfterParam(1);` which implies param index logic.
            // Here we skip first 2 tokens.
            var cmd = string.Join(" ", args.Skip(2));
            context.AddParameter(ParameterId.Command, cmd);
        }
    }
}
