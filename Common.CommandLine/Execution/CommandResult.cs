using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommandLine.Core;

namespace Common.CommandLine.Execution
{
    public class CommandResult
    {
        public bool Result { get; }
        public string Error { get; }

        public bool Succeed => Result;
        public bool Failed => !Result;

        public CommandContext Context { get; }

        public CommandResult(bool result, CommandContext context = null, string error = null)
        {
            Result = result;
            Error = error;
            Context = context;
        }

        public static CommandResult Success(CommandContext context) => new CommandResult(true, context);
        public static CommandResult Failure(CommandContext context, string error) => new CommandResult(false, context, error);
        public static CommandResult Failure(CommandContext context) => new CommandResult(false, context);

        public static CommandResult Success() => new CommandResult(true);
        public static CommandResult Failure(string error) => new CommandResult(false, null, error);
        public static CommandResult Failure() => new CommandResult(false);
    }
}
