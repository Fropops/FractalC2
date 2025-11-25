using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands.Services
{
    public abstract class ServiceCommand<T> : AgentCommand
    {
        protected T Service;
        protected Dictionary<CommandVerbs, Func<AgentTask, AgentCommandContext, Task>> dico = new Dictionary<CommandVerbs, Func<AgentTask, AgentCommandContext, Task>>();
        public ServiceCommand()
        {
            Service = ServiceProvider.GetService<T>();
            this.RegisterVerbs();
        }

        protected virtual void RegisterVerbs()
        {
            this.Register(CommandVerbs.Show, this.Show);
        }

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var verb = task.GetParameter<CommandVerbs>(ParameterId.Verb);
            if (dico.TryGetValue(verb, out var action))
                await action(task, context);
            else
                context.Error($"Verb {verb} not found !");
        }

        public void Register(CommandVerbs verb, Func<AgentTask, AgentCommandContext, Task> action)
        {
            dico.Add(verb, action);
        }

        protected abstract Task Show(AgentTask task, AgentCommandContext context);
    }


    public abstract class ServiceCommand : AgentCommand
    {
        protected Dictionary<CommandVerbs, Func<AgentTask, AgentCommandContext, Task>> dico = new Dictionary<CommandVerbs, Func<AgentTask, AgentCommandContext, Task>>();
        public ServiceCommand()
        {
            this.RegisterVerbs();
        }

        protected virtual void RegisterVerbs()
        {
            this.Register(CommandVerbs.Show, this.Show);
        }

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var verb = task.GetParameter<CommandVerbs>(ParameterId.Verb);
            if (dico.TryGetValue(verb, out var action))
                await action(task, context);
            else
                context.Error($"Verb {verb} not found !");
        }

        public void Register(CommandVerbs verb, Func<AgentTask, AgentCommandContext, Task> action)
        {
            dico.Add(verb, action);
        }

        protected abstract Task Show(AgentTask task, AgentCommandContext context);
    }
}
