namespace WebCommander.Models
{
    public class FileWebHost
    {
        public string Path { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPowershell { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
