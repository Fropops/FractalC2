using System;
using System.Collections.Generic;
using Common.CommandLine.Core;
using Shared;

namespace Common.AgentCommands
{
    public class JobCommandOptions : CommandOption
    {
        [Argument("Verb", "actions available", 0, DefaultValue = CommandVerbs.Show, AllowedValues = new object[] { CommandVerbs.Show, CommandVerbs.Kill }, IsRequired = true)]
        public string Verb { get; set; }

        [Option("i", "id", "Job ID")]
        public int? Id { get; set; }
    }

    [Command("job", "Manage Jobs", Category = AgentCommandCategories.System)]
    public class JobCommand : AgentCommand<JobCommandOptions>
    {
        public override CommandId CommandId => CommandId.Job;

        protected override bool CheckParams(AgentCommandContext context, JobCommandOptions options)
        {
            if (options.Verb.Equals(CommandVerbs.Kill.ToString(), StringComparison.OrdinalIgnoreCase) && !options.Id.HasValue)
            {
                context.WriteError("Job ID is required for kill command.");
                return false;
            }
            return base.CheckParams(context, options);
        }

        protected override void SpecifyParameters(AgentCommandContext context, JobCommandOptions options)
        {
            CommandVerbs verb = (CommandVerbs)Enum.Parse(typeof(CommandVerbs), options.Verb, true);
            context.AddParameter(ParameterId.Verb, verb);
            if (options.Id.HasValue)
            {
                context.AddParameter(ParameterId.Id, options.Id.Value);
            }
        }
    }
}
