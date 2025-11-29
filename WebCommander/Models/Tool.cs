namespace WebCommander.Models
{
    public enum ToolType
    {
        Exe = 0,
        DotNet,
        Powershell
    }

    public class Tool
    {
        public string Name { get; set; }
        public ToolType Type { get; set; }
        public string Data { get; set; }
    }
}
