namespace WebCommander.Models
{
    public enum TerminalLineType
    {
        Normal,
        Error,
        Warning,
        Info,
        Command
    }

    public class TerminalLine
    {
        public string Text { get; set; } = string.Empty;
        public TerminalLineType Type { get; set; } = TerminalLineType.Normal;
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class TerminalHistory
    {
        public List<TerminalLine> OutputLines { get; set; } = new();
        public List<string> CommandHistory { get; set; } = new();
        public HashSet<string> SentTaskIds { get; set; } = new();
        public Dictionary<string, string> TaskCommands { get; set; } = new();
    }
}
