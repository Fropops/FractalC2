using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Commander.Commands.Agent.EndPoint
{
    public class PSCommandOptions
    {
        public string process { get; set; }
    }
    public class PSCommand : EndPointCommand<PSCommandOptions>
    {
        public override string Description => "Change the current working directopy.";
        public override string Name => "ps";

        public override CommandId CommandId => CommandId.ListProcess;
        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("process", () => "", "process name to search for"),
            };

        protected override void SpecifyParameters(CommandContext<PSCommandOptions> context)
        {
            if (!string.IsNullOrEmpty(context.Options.process))
                context.AddParameter(ParameterId.Path, context.Options.process);
        }
    }
}
