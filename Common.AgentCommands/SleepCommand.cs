using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class SleepCommandOptions : CommandOption
    {
        [Argument("delay", "delay in seconds", 0)]
        public int? Delay { get; set; }

        [Argument("jitter", "jitter in percent", 1)]
        public int? Jitter { get; set; }
    }

    [Command("sleep", "Display or Change agent response time", Category = AgentCommandCategories.System)]
    public class SleepCommand : AgentCommand<SleepCommandOptions>
    {
        public override CommandId CommandId => CommandId.Sleep;

        protected override void SpecifyParameters(AgentCommandContext context, SleepCommandOptions options)
        {
            if (options.Delay.HasValue)
                context.AddParameter(ParameterId.Delay, options.Delay.Value);
            if (options.Jitter.HasValue)
                context.AddParameter(ParameterId.Jitter, options.Jitter.Value);
        }
    }
}
