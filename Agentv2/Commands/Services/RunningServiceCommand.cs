using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands.Services
{
    public abstract class RunningServiceCommand<T> : ServiceCommand<T> where T : IRunningService
    {
        public RunningServiceCommand() : base()
        {
            Service = ServiceProvider.GetService<T>();
            this.Register(CommandVerbs.Start, this.Start);
            this.Register(CommandVerbs.Stop, this.Stop);
        }
       
        protected abstract Task Start(AgentTask task, AgentCommandContext context);

        protected abstract Task Stop(AgentTask task, AgentCommandContext context);


    }
}
