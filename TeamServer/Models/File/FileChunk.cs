using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer.Models.File
{
    public class FileChunk
    {
        public string FileId { get; set; }
        public int Index { get; set; }
        public string Data { get; set; }

        public bool IsDownloaded { get; set; }
    }
}
