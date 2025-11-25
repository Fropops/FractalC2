using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.File;

namespace TeamServer.Services
{
    public interface IFileService
    {
        FileDescriptor GetFile(string id);

        void AddFile(FileDescriptor desc);

        void CleanDownloaded();


        string GetFullPath(string fileName);
        string GetAgentPath(string agentId, string fileName);

        string GetAgentPath(string agentId);

        public string GetListenerPath(string listenerName, string fileName);

        public string GetListenerPath(string listenerName);

        public string GetWebHostPath(string fileName);

        public List<AgentFileChunck> GetFileChunksForAgent(string id);

        public void AddAgentFileChunk(AgentFileChunck chunk);
    }
    public class FileService : IFileService
    {
        public static int ChunkSize = 500000;
        private readonly IConfiguration _configuration;
        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static Dictionary<string, FileDescriptor> Cache = new Dictionary<string, FileDescriptor>();

        public string GetFullPath(string fileName)
        {
            var root = PathHelper.GetAbsolutePath(_configuration.GetValue<string>("FilesFolder"));

            var actualPath = Path.Combine(root, fileName.Replace("..", string.Empty));
            if (!actualPath.StartsWith(root))
                actualPath = Path.Combine(root, Path.GetFileName(fileName));
            return actualPath;
        }



        public FileDescriptor GetFile(string id)
        {
            if (!Cache.ContainsKey(id))
                return null;

            return Cache[id];
        }


        public void AddFile(FileDescriptor desc)
        {
            Cache.Add(desc.Id, desc);
        }
        public FileDescriptor CreateFileDescriptor(string filePath)
        {
            var desc = new FileDescriptor()
            {
                Name = filePath,
                Id = Guid.NewGuid().ToString(),
                ChunkSize = FileService.ChunkSize,
            };

            return desc;
        }

        public void CleanDownloaded()
        {
            foreach (var id in Cache.Keys.ToList())
            {
                if (Cache[id].IsDownloaded)
                {
                    Cache.Remove(id);
                }
            }
        }


        //public void SaveUploadedFile(FileDescriptor desc, string path)
        //{

        //    byte[] fileBytes = null;
        //    using (var ms = new MemoryStream())
        //    {
        //        foreach (var chunk in desc.Chunks.OrderBy(c => c.Index))
        //        {
        //            var bytes = Convert.FromBase64String(chunk.Data);
        //            ms.Write(bytes, 0, bytes.Length);
        //        }

        //        fileBytes =  ms.ToArray();

        //    }

        //    var dirName = Path.GetDirectoryName(path);
        //    if (!Directory.Exists(dirName))
        //        Directory.CreateDirectory(dirName);

        //    if (Directory.Exists(path))
        //        throw new Exception("Destination is a directory !");

        //    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        //    {
        //        fs.Write(fileBytes, 0, fileBytes.Length);
        //    }
        //}

        public string GetAgentPath(string agentId, string fileName)
        {
            return this.GetFullPath(Path.Combine("Agent", agentId, fileName));
        }

        public string GetAgentPath(string agentId)
        {
            return this.GetFullPath(Path.Combine("Agent", agentId));
        }

        public string GetListenerPath(string listenerName, string fileName)
        {
            return this.GetFullPath(Path.Combine("WebHost", listenerName.Replace(" ", "_"), fileName));
        }

        public string GetListenerPath(string listenerName)
        {
            return this.GetFullPath(Path.Combine("WebHost", listenerName.Replace(" ", "_")));
        }

        public string GetWebHostPath(string fileName)
        {
            return this.GetFullPath(Path.Combine("WebHost", fileName));
        }




        #region Agent
        public List<AgentFileChunck> GetFileChunksForAgent(string id)
        {
            var file = this.GetFile(id);

            List<AgentFileChunck> chunks = new List<AgentFileChunck>();
            foreach(var ch in file.Chunks)
            {
                var chk = new AgentFileChunck();
                chk.Count = file.ChunkCount;
                chk.FileId = file.Id;
                chk.FileName = file.Name;
                chk.Index = ch.Index;
                chk.Data = ch.Data;
                chunks.Add(chk);
            }
            return chunks;
        }

        public void AddAgentFileChunk(AgentFileChunck chunk)
        {
            var file = this.GetFile(chunk.FileId);
            if(file == null)
            {
                file = new FileDescriptor()
                {
                    ChunkCount = chunk.Count,
                    Id = chunk.FileId,
                    Name = chunk.FileName
                };
                Cache.Add(chunk.FileId, file);
            }

            file.Chunks.Add(new FileChunk()
            {
                Data = chunk.Data,
                FileId = chunk.FileId,
                Index = chunk.Index,
            });
        }
        #endregion

    }
}
