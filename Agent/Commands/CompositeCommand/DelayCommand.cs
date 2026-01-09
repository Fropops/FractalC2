using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DelayCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Delay;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Delay);

            int delay = task.GetParameter<int>(ParameterId.Delay);

            Thread.Sleep(delay*1000);
        }
    }
}
