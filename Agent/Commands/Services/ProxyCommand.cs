using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Agent.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Agent.Helpers;
using Agent.Service;
using Agent.Commands.Services;

namespace Agent.Commands
{
    public class ProxyCommand : ServiceCommand<IProxyService>
    {
        public override string Name => "proxy";


        protected override void Start(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.Result.Result = "Proxy is already running!";
                return;
            }

            this.Service.Start();
            context.Result.Result = $"Proxy started";
        }

        protected override void Stop(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status != RunningService.RunningStatus.Running)
            {
                context.Result.Result = "Proxy is not running!";
                return;
            }

            this.Service.Stop();
            context.Result.Result = $"Proxy stopped";
        }

        protected override void Show(AgentTask task, AgentCommandContext context, string[] args)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
                context.Result.Result = "Proxy is running!";
            else
                context.Result.Result = "Proxy is stopped!";
            return;
        }
    }
}
