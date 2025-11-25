using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared.ResultObjects
{
    public class ListProcessResult
    {
        [FieldOrder(0)]
        public string Name { get; set; }
        [FieldOrder(1)]
        public int Id { get; set; }
        [FieldOrder(2)]
        public int ParentId { get; set; }
        [FieldOrder(3)]
        public int SessionId { get; set; }
        [FieldOrder(4)]
        public string ProcessPath { get; set; }
        [FieldOrder(5)]
        public string Owner { get; set; }
        [FieldOrder(6)]
        public string Arch { get; set; }
    }
}
