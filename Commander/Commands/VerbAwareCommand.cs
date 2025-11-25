using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Executor;
using Shared;

namespace Commander.Commands
{
    public class VerbAwareCommandOptions
    {
        public string verb { get; set; }

        public CommandVerbs CommandVerb { get { return (CommandVerbs)Enum.Parse(typeof(CommandVerbs), verb, true); } }
    }
    public abstract class VerbAwareCommand<T> : EndPointCommand<T> where T : VerbAwareCommandOptions
    {
        protected Dictionary<CommandVerbs, Func<CommandContext<T>, Task<bool>>> dico = new Dictionary<CommandVerbs, Func<CommandContext<T>, Task<bool>>>();
        public override string Category => CommandCategory.Services;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
        public override CommandId CommandId => CommandId.Job;

        public VerbAwareCommand()
        {
            RegisterVerbs();
        }

        protected virtual void RegisterVerbs()
        {
        }

        public void Register(CommandVerbs verb, Func<CommandContext<T>, Task<bool>> action)
        {
            dico.Add(verb, action);
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            if (!await CheckParams(context))
                return false;

            var verb = context.Options.CommandVerb;
            context.AddParameter(ParameterId.Verb, verb);

            if (dico.TryGetValue(verb, out var action))
                if (!await action(context))
                    return false;

            SpecifyParameters(context);

            await CallEndPointCommand(context);
            return true;
        }

    }
}
