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
    public class LsCommandOptions
    {
        public string path { get; set; }
    }
    public class LsCommand : EndPointCommand<LsCommandOptions>
    {
        public override string Description => "List the content of the directory directopy.";
        public override string Name => "cd";

        public override CommandId CommandId => CommandId.Cd;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", () => string.Empty, "Path of the directory to list"),
            };

        protected override void SpecifyParameters(CommandContext<LsCommandOptions> context)
        {
            if (!string.IsNullOrEmpty(context.Options.path))
                context.AddParameter(ParameterId.Path, context.Options.path);
        }
    }
}
