using BinarySerializer;

namespace WebCommander.Models.ResultObjects
{
    public sealed class LinkInfo
    {
        [FieldOrder(0)]
        public string TaskId { get; set; } = string.Empty;

        [FieldOrder(1)]
        public string ParentId { get; set; } = string.Empty;

        [FieldOrder(2)]
        public string ChildId { get; set; } = string.Empty;

        [FieldOrder(3)]
        public string Binding { get; set; } = string.Empty;

        public LinkInfo(string taskId, string parentId)
        {
            TaskId = taskId;
            ParentId = parentId;
            ChildId = string.Empty;
            Binding = string.Empty;
        }

        public LinkInfo()
        {
        }
    }
}
