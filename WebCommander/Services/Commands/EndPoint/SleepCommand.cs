using System.CommandLine;
using System.CommandLine.Parsing;
using WebCommander.Models;

namespace WebCommander.Services.Commands.EndPoint
{
    public class SleepCommand : EndPointCommand
    {
        public override string Name => "sleep";
        public override string Description => "Display or Change agent response time";
        public override CommandId Id => CommandId.Sleep;
        public override string Category => CommandCategory.Core;

        protected override void AddCommandParameters(RootCommand command)
        {
            command.Add(new Argument<int?>("delay") { Description = "Delay in seconds", Arity = ArgumentArity.ZeroOrOne });
            command.Add(new Argument<int?>("jitter") { Description = "Jitter in percent", Arity = ArgumentArity.ZeroOrOne });
        }

        public override Task FillParametersAsync(ParseResult result, ParameterDictionary parms)
        {
            var delay = result.GetValue<int?>("delay");
            var jitter = result.GetValue<int?>("jitter");
            
            if (delay.HasValue)
                parms.AddParameter(ParameterId.Delay, delay.Value);
            if (jitter.HasValue)
                parms.AddParameter(ParameterId.Jitter, jitter.Value);
            
            return Task.CompletedTask;
        }
    }
}
