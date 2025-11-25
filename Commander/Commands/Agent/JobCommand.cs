using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Commander.Helper;
using Shared;

namespace Commander.Commands.Agent
{
    public class JobCommandOptions : VerbAwareCommandOptions
    {
        public int? id { get; set; }
    }
    public class JobCommand : VerbAwareCommand<JobCommandOptions>
    {
        public override string Description => "Manage Jobs";
        public override string Name => "job";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.Job;
        public override RootCommand Command => new RootCommand(Description)
        {
            new Argument<string>("verb", () => CommandVerbs.Show.Command()).FromAmong(CommandVerbs.Show.Command(), CommandVerbs.Kill.Command()),
            new Option<int?>(new[] { "--id", "-i" }, () => null, "Id of the job (Kill)"),
        };

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            Register(CommandVerbs.Kill, Kill);
            //this.Register(ServiceVerb.Show, this.Show);
        }

        protected async Task<bool> Kill(CommandContext<JobCommandOptions> context)
        {
            if (!context.Options.id.HasValue)
            {
                context.Terminal.WriteError($"Id is mandatory for kill verb");
                return false;
            }

            context.AddParameter(ParameterId.Id, context.Options.id.Value);

            return true;
        }


    }
}
