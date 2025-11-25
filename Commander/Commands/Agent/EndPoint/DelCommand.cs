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
    public class DelCommandOptions
    {
        public string path { get; set; }
    }
    public class DelCommand : EndPointCommand<DelCommandOptions>
    {
        public override string Description => "Delete a file on the agent.";
        public override string Name => "del";

        public override CommandId CommandId => CommandId.Del;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", "Path of the file to delete"),
            };

        protected override void SpecifyParameters(CommandContext<DelCommandOptions> context)
        {
            context.AddParameter(ParameterId.Path, context.Options.path);
        }
    }
}
