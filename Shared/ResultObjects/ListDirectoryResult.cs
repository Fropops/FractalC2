using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared.ResultObjects
{
    public class ListDirectoryResultLine
    {
        [FieldOrder(0)]
        public long Length { get; set; }
        [FieldOrder(1)]
        public string Name { get; set; } = string.Empty;
        [FieldOrder(2)]
        public bool IsFile { get; set; }
    }

    public class ListDirectoryResult
    {
        [FieldOrder(0)]
        public List<ListDirectoryResultLine> Lines { get; set; } = new List<ListDirectoryResultLine>();

        [FieldOrder(1)]
        public string Directory { get; set; } = string.Empty;
    }
}
