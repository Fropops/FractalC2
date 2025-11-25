using System.Collections.Generic;
using BinarySerializer;

namespace Shared
{
    public sealed class LinkInfo
    {
        [FieldOrder(0)]
        public string TaskId { get; set; }

        [FieldOrder(1)]
        public string ParentId { get; set; }

        [FieldOrder(2)]
        public string ChildId { get; set; }

        [FieldOrder(3)]
        public string Binding { get; set; }

        public LinkInfo(string taskId, string parentId)
        {
            TaskId = taskId;
            ParentId = parentId;
        }

        public LinkInfo()
        {

        }
    }
}
