using Commander.Executor;
using Shared;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{

    public class DownloadCommandOptions
    {
        public string remotefile { get; set; }
        public string localfile { get; set; }
    }
    public class DownloadCommand : EndPointCommand<DownloadCommandOptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Download a file from the agent";
        public override string Name => "download";
        public override CommandId CommandId => CommandId.Download;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("remotefile",  "name of the file to load from the agent"),
                new Argument<string>("localFile",() => string.Empty, "local file name to downloaded to."),
            };

        protected override void SpecifyParameters(CommandContext<DownloadCommandOptions> context)
        {
            context.AddParameter(ParameterId.Path, context.Options.remotefile);
            if (!string.IsNullOrEmpty(context.Options.localfile))
                context.AddParameter(ParameterId.Name, context.Options.localfile);
        }
    }
}
