using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared.ResultObjects
{
    public class ListDirectoryResult
    {
        [FieldOrder(0)]
        public long Length { get; set; }
        [FieldOrder(1)]
        public string Name { get; set; }
        [FieldOrder(2)]
        public bool IsFile { get; set; }
    }
}
