using BinarySerializer;

namespace WebCommander.Models
{
    public class ReversePortForwardResult
    {
        [FieldOrder(0)]
        public int Port { get; set; }
        [FieldOrder(1)]
        public string DestHost { get; set; }
        [FieldOrder(2)]
        public int DestPort { get; set; }
    }
}
