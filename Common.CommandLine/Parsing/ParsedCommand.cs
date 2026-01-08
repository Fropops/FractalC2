using System.Collections.Generic;
using SharpCommandLine.Parsing;

namespace Common.CommandLine.Parsing
{
    public class ParsedCommand
    {
        public string Name { get; set; }
        public List<string> Arguments { get; set; } = new List<string>();
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
        public List<CommandLineToken> Tokens { get; } = new List<CommandLineToken>();
    }
}
