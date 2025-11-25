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
    public class PsExecCommandOptions
    {
        public string path { get; set; }
        public string target { get; set; }
    }
    public class PsExecCommand : EndPointCommand<PsExecCommandOptions>
    {
        public override string Description => "Send a path to be run as remote service";
        public override string Name => "psexec";

        public override CommandId CommandId => CommandId.PsExec;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("target", "Target computer."),
                 new Argument<string>("path", "path of the service to start"),
            };

        protected override void SpecifyParameters(CommandContext<PsExecCommandOptions> context)
        {
            context.AddParameter(ParameterId.Target, context.Options.target);
            context.AddParameter(ParameterId.Path, context.Options.path);
        }
    }
}
