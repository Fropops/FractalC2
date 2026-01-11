using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Shared;
using WebCommander.Services;

namespace WebCommander.Commands
{
    public class UploadCommandComplement
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
    }
    public class UploadCommandOptions : CommandOption
    {
        [Argument("remotefile", "Path of the file to be saved on the agent", 0)]
        public string remoteFile { get; set; }
    }

    [Command("upload", "Upload a file to the agent", Category = "Network", Aliases = new string[] { "put" })]
    public class UploadCommand : AgentCommand<UploadCommandOptions>
    {
        public override CommandId CommandId => CommandId.Upload;

        public override async Task<bool> Execute(AgentCommandContext context, UploadCommandOptions options)
        {
            if (context.Complement == null)
            {
                context.WriteError("No file content provided for upload. Please use the UI to upload a file.");
                return false;
            }

            var fileInfo = context.Complement as UploadCommandComplement;

            if (fileInfo == null || fileInfo.FileBytes == null || fileInfo.FileBytes.Length == 0 || string.IsNullOrEmpty(fileInfo.FileName))
            {
                context.WriteError("No file content provided for upload. Please use the UI to upload a file.");
                return false;
            }

            context.AddParameter(ParameterId.File, fileInfo.FileBytes);
            

            string filename = options.remoteFile ?? fileInfo.FileName;
            context.AddParameter(ParameterId.Name, filename);

            context.TaskAgent(options.CommandLine, CommandId.Upload);

            return true;
        }
    }
}
