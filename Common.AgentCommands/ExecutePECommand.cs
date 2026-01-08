using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;
using System.IO;

namespace Common.AgentCommands
{
    public class ExecutePECommandOptions : CommandOption
    {
        [Argument("ExePath", "Path to the executable", 0, IsRequired = true)]
        public string ExePath { get; set; }

        [Argument("Arguments", "Arguments for the executable", 1, IsRemainder = true)]
        public string Arguments { get; set; }
    }

    [Command("execute-pe", "Execute a PE assembly with Fork And Run mechanism", Category = AgentCommandCategories.Execution)]
    public class ExecutePECommand : AgentCommand<ExecutePECommandOptions>
    {
        public override CommandId CommandId => CommandId.ForkAndRun;

        protected override void SpecifyParameters(AgentCommandContext context, ExecutePECommandOptions options)
        {
            if (string.IsNullOrEmpty(options.Arguments))
                context.WriteInfo($"Generating payload without params...");
            else
                context.WriteInfo($"Generating payload with params {options.Arguments}...");

            context.AddParameter(ParameterId.Name, Path.GetFileName(options.ExePath));
            
            if (!string.IsNullOrEmpty(options.Arguments))
                context.AddParameter(ParameterId.Parameters, options.Arguments);
                
            context.AddParameter(ParameterId.Output, true);
        }
    }
}
