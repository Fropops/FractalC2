using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands.Agent;
using Commander.Executor;
using Shared;

namespace Commander.Commands.Core
{
    public class VerbAwareCommandOptions
    {
        public string verb { get; set; }

    }

    public abstract class VerbAwareCommand<T> : EnhancedCommand<T> where T : VerbAwareCommandOptions
    {
        protected Dictionary<string, Func<CommandContext<T>, Task<bool>>> dico = new Dictionary<string, Func<CommandContext<T>, Task<bool>>>();
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public VerbAwareCommand()
        {
            RegisterVerbs();
        }

        protected virtual void RegisterVerbs()
        {
        }

        public void Register(string verb, Func<CommandContext<T>, Task<bool>> action)
        {
            dico.Add(verb.ToLower(), action);
        }

        protected override async Task<bool> HandleCommand(CommandContext<T> context)
        {
            var verb = context.Options.verb.ToLower();

            if (dico.TryGetValue(verb, out var action))
                if (!await action(context))
                    return false;
            return true;
        }

    }
}
