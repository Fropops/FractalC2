using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CommandLine.Execution
{
    public class CommandResult
    {
        public bool Result { get; }
        public string Error { get; }

        public bool Succeed => Result;
        public bool Failed => !Result;

        public CommandResult(bool result, string error = null)
        {
            Result = result;
            Error = error;
        }

        public static CommandResult Success() => new CommandResult(true);
        public static CommandResult Failure(string error) => new CommandResult(false, error);
        public static CommandResult Failure() => new CommandResult(false);
    }
}
