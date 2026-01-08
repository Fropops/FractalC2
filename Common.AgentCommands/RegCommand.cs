using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class RegCommandOptions : CommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Show, CommandVerbs.Start, CommandVerbs.Stop }, IsRequired = true)]
        public string Verb { get; set; }

        [Argument("path", "Path of the Key", 1, IsRequired = true)]
        public string Path { get; set; }

        [Argument("key", "Name of the Key", 2)]
        public string Key { get; set; } = string.Empty;

        [Argument("value", "Value of the Key (Add)", 3)]
        public string Value { get; set; }
    }

    [Command("reg", "Manage registry keys on the agent", Category = AgentCommandCategories.System)]
    public class RegCommand : AgentCommand<RegCommandOptions>
    {
        public override CommandId CommandId => CommandId.Reg;

        protected override bool CheckParams(AgentCommandContext context, RegCommandOptions options)
        {
             if (options.Verb.Equals("add", StringComparison.OrdinalIgnoreCase))
             {
                if (string.IsNullOrEmpty(options.Value))
                {
                    context.WriteError($"[X] Value is required!");
                    return false;
                }
             }
            return base.CheckParams(context, options);
        }

        protected override void SpecifyParameters(AgentCommandContext context, RegCommandOptions options)
        {
             context.AddParameter(ParameterId.Verb, options.Verb); // Hope it serializes correctly.
            
             context.AddParameter(ParameterId.Path, options.Path);
            if (!string.IsNullOrEmpty(options.Key))
                context.AddParameter(ParameterId.Key, options.Key);
             if (!string.IsNullOrEmpty(options.Value))
                context.AddParameter(ParameterId.Value, options.Value);
        }
    }
}
