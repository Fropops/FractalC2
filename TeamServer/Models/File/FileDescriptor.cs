using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models.File
{
    public class FileDescriptor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
        public int ChunkSize { get; set; }
        public int ChunkCount { get; set; }

        public bool IsDownloaded
        {
            get
            {
                return this.Chunks.All(c => c.IsDownloaded);
            }
        }

        public bool IsUploaded
        {
            get
            {
                return this.Chunks.Count == this.ChunkCount;
                
            }
        }

        public List<FileChunk> Chunks { get; set; } = new List<FileChunk>();

    }
}
