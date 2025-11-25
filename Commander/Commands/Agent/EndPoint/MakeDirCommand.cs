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
    public class MakeDirCommandOptions
    {
        public string path { get; set; }
    }
    public class MakeDirCommand : EndPointCommand<MakeDirCommandOptions>
    {
        public override string Description => "Create a folder on the agent.";
        public override string Name => "mkdir";

        public override CommandId CommandId => CommandId.MkDir;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", "Path of the folder to create"),
            };

        protected override void SpecifyParameters(CommandContext<MakeDirCommandOptions> context)
        {
            context.AddParameter(ParameterId.Path, context.Options.path);
        }
    }
}
