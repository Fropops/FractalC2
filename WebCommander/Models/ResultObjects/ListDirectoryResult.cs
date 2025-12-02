using BinarySerializer;

namespace WebCommander.Models.ResultObjects
{
    public class ListDirectoryResult
    {
        [FieldOrder(0)]
        public long Length { get; set; }
        [FieldOrder(1)]
        public string Name { get; set; } = string.Empty;
        [FieldOrder(2)]
        public bool IsFile { get; set; }
    }
}
