using Common.AgentCommands;
using Common.CommandLine.Core;
using Shared;

namespace Commander.Commands.Agent
{

    public class DownloadCommandOptions : CommandOption
    {
        [Argument("agentFile", "Name of the file to load from the agent", 0, IsRequired = true)]
        public string agentFile { get; set; }
        [Argument("tsFile", "Name of the destination file on the TeamServer.", 1)]
        public string tsFile { get; set; }
    }

    [Command("download", "Download a file from the agent to the TeamServer", Category = AgentCommandCategories.System)]
    public class DownloadCommand : AgentCommand<DownloadCommandOptions>
    {
       
        public override CommandId CommandId => CommandId.Download;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };

        protected override void SpecifyParameters(AgentCommandContext context, DownloadCommandOptions options)
        {
            context.AddParameter(ParameterId.Path, options.agentFile);
            if (!string.IsNullOrEmpty(options.tsFile))
                context.AddParameter(ParameterId.Name, options.tsFile);
        }
    }
}
