using System;

namespace Common.CommandLine.Core
{
    public abstract class CommandContext
    {
        // Base class for context, can be extended to add specific properties
        public Object Complement { get; set; }
    }

    public class CommandOption
    {
        // Base class for options, will contain properties decorated with attributes
        public string CommandLine { get; set; }
    }

    public interface ICommand<TContext, TOptions>
        where TContext : CommandContext
        where TOptions : CommandOption
    {
        Task<bool> Execute(TContext context, TOptions options);
    }

}
