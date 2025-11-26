using System.Text.Json.Serialization;

namespace WebCommander.Models
{
    public enum ChangingElement
    {
        Listener = 0,
        Agent,
        Task,
        Result,
        Metadata,
    }

    public class Change
    {
        public string Id { get; set; }
        [JsonPropertyName("element")]
        public ChangingElement Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
