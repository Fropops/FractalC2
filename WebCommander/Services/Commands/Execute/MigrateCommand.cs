using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;
using WebCommander.Helpers;

namespace WebCommander.Services.Commands
{
    public class MigrateCommand : EndPointCommand
    {
        public override string Name => "migrate";
        public override string Description => "Migrate to a new process";
        public override CommandId Id => CommandId.Inject;
        public override string Category => CommandCategory.Execution;

        private string processIdParam = "ProcessId";
        private string x86Param = "-x86";
        private string endpointParam = "--endpoint";

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Arguments.Add(new Argument<string>(processIdParam) { Arity = ArgumentArity.ExactlyOne, Description = "Process ID to migrate to" });
            command.Options.Add(new Option<bool?>(x86Param)  { Arity = ArgumentArity.ZeroOrOne, Description = "Force x86 architecture" });
            command.Options.Add(new Option<string>(endpointParam, "-b") { Arity = ArgumentArity.ZeroOrOne, Description = "Endpoint to bind to" });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            parms.AddParameter(ParameterId.Id, int.Parse(parseResult.GetValue<string>(processIdParam)));
            var x86 = parseResult.GetValue<bool?>(x86Param);
            if(!x86.HasValue)
                x86 = _agent.Metadata.Architecture == "x86";
            var endpoint = parseResult.GetValue<string>(endpointParam);
            if(string.IsNullOrWhiteSpace(endpoint))
                endpoint = _agent.Metadata.EndPoint;
            parms.AddParameter(ParameterId.Target, x86.Value ? "x86" : "x64");
            parms.AddParameter(ParameterId.Bind, endpoint);
        }
    }

    
}
