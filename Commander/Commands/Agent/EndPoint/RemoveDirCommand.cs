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
    public class RemoveDirCommandOptions
    {
        public string path { get; set; }
    }
    public class RemoveDirCommand : EndPointCommand<RemoveDirCommandOptions>
    {
        public override string Description => "Delete a folder on the agent.";
        public override string Name => "rmdir";

        public override CommandId CommandId => CommandId.RmDir;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", "Path of the folder to delete"),
            };

        protected override void SpecifyParameters(CommandContext<RemoveDirCommandOptions> context)
        {
            context.AddParameter(ParameterId.Path, context.Options.path);
        }
    }
}
