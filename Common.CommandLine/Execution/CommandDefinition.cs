using System;
using Common.CommandLine.Core;

namespace Common.CommandLine.Execution
{
    public class CommandDefinition
    {
        public CommandAttribute Metadata { get; set; }
        public Type CommandType { get; set; }
        public Type OptionsType { get; set; }
        public Type ContextType { get; set; }
    }
}
