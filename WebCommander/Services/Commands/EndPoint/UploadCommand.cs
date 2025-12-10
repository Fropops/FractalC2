using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using WebCommander.Models;

namespace WebCommander.Services.Commands
{
    public class UploadCommand : EndPointCommand
    {
        public override string Name => "upload";
        public override string Description => "Upload a file to the agent";
        public override CommandId Id => CommandId.Upload;
        public override string Category => CommandCategory.Network;

        // Valid only for AgentInteraction (implied by CommandCategory but good to note)
        
        // These properties will be populated by the UI (Terminal.razor) before executing the command
        public byte[] FileBytes { get; set; }
        public string LocalFileName { get; set; }

        protected override void AddCommandParameters(RootCommand command)
        {
            // Optional argument for remote destination path/filename
            command.Arguments.Add(new Argument<string>("remotefile") { Arity = ArgumentArity.ZeroOrOne, Description = "Path of the file to be saved on the agent" });
        }

        public override async Task FillParametersAsync(ParseResult parseResult, ParameterDictionary parms)
        {
            var remoteFile = parseResult.GetValue<string>("remotefile");
            var filename = LocalFileName;

            if (!string.IsNullOrEmpty(remoteFile))
            {
                filename = remoteFile;
            }

            // If filename was somehow not set (should not happen if UI works correctly), default to unknown
            if (string.IsNullOrEmpty(filename))
            {
                 filename = "unknown_upload";
            }

            parms.AddParameter(ParameterId.Name, filename);
            
            if (FileBytes != null && FileBytes.Length > 0)
            {
                parms.AddParameter(ParameterId.File, FileBytes);
            }
        }
    }
}
