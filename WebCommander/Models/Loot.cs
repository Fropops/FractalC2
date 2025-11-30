namespace WebCommander.Models
{
    public class Loot
    {
        public string FileName { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string? ThumbnailData { get; set; }
        public bool IsImage { get; set; }
        public string? Data { get; set; }
    }
}
