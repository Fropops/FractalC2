using Agent.Service;
using Shared;
using System.Diagnostics;

namespace Agent.Commands
{
    public class ShellCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Shell;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Command);

            var arg = task.GetParameter<string>(ParameterId.Command);
            var cmd = $@"c:\windows\system32\cmd.exe /c {arg}";

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{arg.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            var jobService = ServiceProvider.GetService<IJobService>();
            this.JobId = jobService.RegisterJob(JobType.Shell, process.Id, arg, task.Id).Id;

            
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            context.AppendResult(output, false);
            if (!string.IsNullOrEmpty(error))
                context.Error(error);
        }
    }
}
