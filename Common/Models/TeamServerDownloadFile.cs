using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Common.Models
{
    public class TeamServerDownloadFile
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string Data { get; set; }
        public string Source { get; set; }
        public string Path { get; set; }
    }
}
