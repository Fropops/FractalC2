using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class InlineAssemblyCommandOptions : CommandOption
    {
        [Argument("AssemblyPath", "Path to the assembly", 0, IsRequired = true)]
        public string AssemblyPath { get; set; }

        [Argument("Arguments", "Arguments for the assembly", 1, IsRemainder = true)]
        public string Arguments { get; set; }
    }

    [Command("inline-assembly", "Execute a dot net assembly in memory", Category = AgentCommandCategories.Execution)]
    public class InlineAssemblyCommand : AgentCommand<InlineAssemblyCommandOptions>
    {
        public override CommandId CommandId => CommandId.Assembly;

        protected override void SpecifyParameters(AgentCommandContext context, InlineAssemblyCommandOptions options)
        {
            context.AddParameter(ParameterId.Name, options.AssemblyPath);
            if (!string.IsNullOrEmpty(options.Arguments))
                context.AddParameter(ParameterId.Parameters, options.Arguments);
            context.AddParameter(ParameterId.Output, true);
        }
    }
}
