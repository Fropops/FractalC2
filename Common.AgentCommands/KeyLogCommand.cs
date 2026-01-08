using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class KeyLogCommandOptions : CommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Show, CommandVerbs.Start, CommandVerbs.Stop }, IsRequired = true)]
        public string Verb { get; set; }
    }

    [Command("keylog", "Log keys on the agent", Category = AgentCommandCategories.System)]
    public class KeyLogCommand : AgentCommand<KeyLogCommandOptions>
    {
        public override CommandId CommandId => CommandId.KeyLog;

        protected override void SpecifyParameters(AgentCommandContext context, KeyLogCommandOptions options)
        {
             if (!string.IsNullOrEmpty(options.Verb))
            {
                 context.AddParameter(ParameterId.Verb, options.Verb);
            }
        }
    }
}
