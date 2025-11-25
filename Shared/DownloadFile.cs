using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public class DownloadFile
    {
        [FieldOrder(0)]
        public string Id { get; set; }
        [FieldOrder(1)]
        public string FileName { get; set; }
        [FieldOrder(2)]
        public byte[] Data { get; set; }
        [FieldOrder(3)]
        public string Source { get; set; }
        [FieldOrder(4)]
        public string Path { get; set; }

    }
}
