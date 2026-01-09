using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands;
using Commander.Commands.Agent;
using Commander.Executor;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Newtonsoft.Json;
using Shared;

namespace Commander.CommanderCommand.Agent
{

    public class UploadCommandoptions : CommandOption
    {
        
        [Argument("localfile", "Name of the source file on local computer.", 0, IsRequired = true)]
        public string localfile { get; set; }

        [Argument("remotefile", "Name of the file on the agent", 1)]
        public string remotefile { get; set; }
    }

    [Command("upload", "Upload a file to the agent", Category = AgentCommandCategories.System)]
    public class UploadCommand : AgentCommand<UploadCommandoptions>
    {
        public override CommandId CommandId => CommandId.Upload;
        public override OsType[] SupportedOs => new Shared.OsType[] { OsType.Windows, OsType.Linux };
        protected override bool CheckParams(AgentCommandContext context, UploadCommandoptions options)
        {
            if (!File.Exists(options.localfile))
            {
                context.WriteError($"File {options.localfile} does not exists!");
                return false;
            }
            return true;
        }


        protected override void SpecifyParameters(AgentCommandContext context, UploadCommandoptions options)
        {
            var path = options.localfile;
            var filename = Path.GetFileName(path);
            
            byte[] fileBytes = null;
            using (FileStream fs = File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            if (!string.IsNullOrEmpty(options.remotefile))
            {
                filename = options.remotefile;
            }

            context.AddParameter(ParameterId.Name, filename);
            context.AddParameter(ParameterId.File, fileBytes);

        }
    }
}
