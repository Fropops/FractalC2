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

    public class UploadCommandoptions
    {
        public string remotefile { get; set; }
        public string localfile { get; set; }
    }
    public class UploadCommand : EndPointCommand<UploadCommandoptions>
    {
        public override string Category => CommandCategory.Network;
        public override string Description => "Upload a file to the agent";
        public override string Name => "upload";
        public override CommandId CommandId => CommandId.Upload;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("localFile", "local file path to be uploaded to the agent."),
                new Argument<string>("remotefile", () => string.Empty, "path of the file to be saved on the agent"),
            };

        protected override async Task<bool> CheckParams(CommandContext<UploadCommandoptions> context)
        {
            if (!File.Exists(context.Options.localfile))
            {
                context.Terminal.WriteError($"File {context.Options.localfile} does not exists!");
                return false;
            }
            return await base.CheckParams(context);
        }


        protected override void SpecifyParameters(CommandContext<UploadCommandoptions> context)
        {
            var path = context.Options.localfile;
            var filename = Path.GetFileName(path);
            
            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            if (!string.IsNullOrEmpty(context.Options.remotefile))
            {
                filename = context.Options.remotefile;
            }

            context.AddParameter(ParameterId.Name, filename);
            context.AddParameter(ParameterId.File, fileBytes);

        }
    }
}
