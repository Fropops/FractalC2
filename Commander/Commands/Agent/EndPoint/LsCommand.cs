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
    public class AgentCdCommandOptions
    {
        public string path { get; set; }
    }
    public class CdCommandOptions : EndPointCommand<AgentCdCommandOptions>
    {
        public override string Description => "Change the current working directopy.";
        public override string Name => "ls";

        public override CommandId CommandId => CommandId.Ls;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", () => string.Empty, "Path of the new directory"),
            };

        protected override void SpecifyParameters(CommandContext<AgentCdCommandOptions> context)
        {
            if (!string.IsNullOrEmpty(context.Options.path))
                context.AddParameter(ParameterId.Path, context.Options.path);
        }
    }
}
