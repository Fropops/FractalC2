using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class JobCommand : VerbAwareCommand
    {
        public override string Name => "job";
        public override string Description => "Manage Jobs";
        public override CommandId Id => CommandId.Job;
        override protected List<CommandVerbs> AllowedVerbs => new List<CommandVerbs> { CommandVerbs.Show, CommandVerbs.Kill };
        private string jobIdParam = "--id";

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Options.Add(new Option<int?>(jobIdParam, "-i") { Arity = ArgumentArity.ZeroOrOne, Description = "Job ID" });
            base.AddCommandParameters(command);
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            await base.FillParametersAsync(parseResult, parms);
            var jobId = parseResult.GetValue<int?>(jobIdParam);
            if(jobId.HasValue)
                parms.AddParameter(ParameterId.Id, jobId.Value);
        }
    }
}
