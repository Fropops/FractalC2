using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;
using Shared;

namespace Commander.Commands
{
    public abstract class VerbCommandOption : CommandOption
    {
        public abstract string verb { get; set; }
    }

    public abstract class VerbCommand<TContext, TOptions> : ICommand<TContext, TOptions>
        where TContext : CommandContext
        where TOptions : VerbCommandOption
    {

        protected Dictionary<string, Func<TContext, TOptions, Task<bool>>> dico = new Dictionary<string, Func<TContext, TOptions, Task<bool>>>();
        
        public VerbCommand()
        {
            RegisterVerbs();
        }

        protected abstract void RegisterVerbs();

        public void Register(string verb, Func<TContext, TOptions, Task<bool>> action)
        {
            dico.Add(verb.ToLower(), action);
        }

        public virtual async Task<bool> Execute(TContext context, TOptions options)
        {
            var verb = options.verb.ToLower();

            if (dico.TryGetValue(verb, out var action))
                return await action(context, options);

            return false;
        }
    }
}
