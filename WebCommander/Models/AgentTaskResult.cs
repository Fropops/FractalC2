namespace WebCommander.Models
{
    public enum AgentResultStatus : byte
    {
        Queued = 0x00,
        Running = 0x01,
        Completed = 0x02,
        Error = 0x03
    }

    public class AgentTaskResult
    {
        public string Id { get; set; }
        public string Output { get; set; }
        public byte[] Objects { get; set; }
        public string Error { get; set; }
        public string Info { get; set; }
        public AgentResultStatus Status { get; set; }
    }
}
