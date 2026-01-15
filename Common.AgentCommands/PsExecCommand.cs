using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class PsExecCommandOptions : CommandOption
    {
        [Argument("target", "Target computer.", 0, IsRequired = true)]
        public string Target { get; set; }

        [Argument("path", "path of the service to start", 1, IsRequired = true)]
        public string Path { get; set; }

        [Option("s", "service", "Name of service.", DefaultValue = null)]
        public string Service { get; set; }
        [Option("n", "name", "Display Name of service.", DefaultValue = null)]
        public string Name { get; set; }
    }

    [Command("psexec", "Send a path to be run as remote service", Category = AgentCommandCategories.LateralMovement)]
    public class PsExecCommand : AgentCommand<PsExecCommandOptions>
    {
        public override CommandId CommandId => CommandId.PsExec;

        protected override void SpecifyParameters(AgentCommandContext context, PsExecCommandOptions options)
        {
            context.AddParameter(ParameterId.Target, options.Target);
            context.AddParameter(ParameterId.Path, options.Path);
            if(!string.IsNullOrEmpty(options.Name))
                context.AddParameter(ParameterId.Name, options.Name);
            if (!string.IsNullOrEmpty(options.Service))
                context.AddParameter(ParameterId.Service, options.Service);
        }
    }
}
