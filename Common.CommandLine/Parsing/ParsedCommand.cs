using System.Collections.Generic;

namespace Common.CommandLine.Parsing
{
    public class ParsedCommand
    {
        public string Name { get; set; }
        public List<string> Arguments { get; set; } = new List<string>();
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }
}
