using BinarySerializer;

namespace WebCommander.Models.ResultObjects
{
    public enum JobType : byte
    {
        ForkAndRun = 0,
        InlineAssembly,
        Shell,
        KeyLog
    }

    public class Job
    {
        [FieldOrder(0)]
        public JobType JobType { get; set; }
        
        [FieldOrder(1)]
        public string Name { get; set; } = string.Empty;
        
        [FieldOrder(2)]
        public int Id { get; set; }
        
        [FieldOrder(3)]
        public string TaskId { get; set; } = string.Empty;
        
        [FieldOrder(4)]
        public int? ProcessId { get; set; }
    }
}
