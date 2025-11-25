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

   

    public class SleepCommandOptions
    {
        public int? delay { get; set; }
        public int? jitter { get; set; }
    }
    public class SleepCommand : EndPointCommand<SleepCommandOptions>
    {
        public override string Description => "Display or Change agent response time";
        public override string Name => "sleep";

        public override CommandId CommandId => CommandId.Sleep;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int?>("delay", () => null, "delay in seconds"),
                new Argument<int?>("jitter", () => null, "jitter in percent"),
            };

        protected override void SpecifyParameters(CommandContext<SleepCommandOptions> context)
        {
            base.SpecifyParameters(context);
            if(context.Options.delay.HasValue)
                context.AddParameter(ParameterId.Delay, context.Options.delay.Value);
            if (context.Options.jitter.HasValue)
                context.AddParameter(ParameterId.Jitter, context.Options.jitter.Value);
        }
    }
}
