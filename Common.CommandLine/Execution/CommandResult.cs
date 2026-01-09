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
        public string Message { get; }

        public bool Succeed => Result;
        public bool Failed => !Result;

        public CommandContext Context { get; }

        public CommandResult(bool result, CommandContext context = null, string message = null)
        {
            Result = result;
            Message = message;
            Context = context;
        }

        public static CommandResult Success(CommandContext context) => new CommandResult(true, context);
        public static CommandResult Failure(CommandContext context, string message) => new CommandResult(false, context, message);
        public static CommandResult Failure(CommandContext context) => new CommandResult(false, context);

        public static CommandResult Success() => new CommandResult(true);
        public static CommandResult Failure(string message) => new CommandResult(false, null, message);

        public static CommandResult Success(string message) => new CommandResult(true, null, message);
        public static CommandResult Failure() => new CommandResult(false);
    }
}
