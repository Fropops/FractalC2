namespace WebCommander.Models
{
    public class Agent
    {
        public string Id { get; set; }
        public string Hostname { get; set; }
        public string Ip { get; set; }
        public string? RelayId { get; set; }
        public DateTime LastSeen { get; set; }
        public AgentMetadata? Metadata { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Agent agent && Id == agent.Id;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}
