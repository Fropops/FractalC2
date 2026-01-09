using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.CommandLine.Binding;
using Common.CommandLine.Core;
using Common.CommandLine.Help;
using Common.CommandLine.Parsing;

namespace Common.CommandLine.Execution
{
    public class CommandExecutor
    {
        private readonly CommandLoader _loader;
        private readonly CommandLineParser _parser;
        private readonly CommandBinder _binder;

        private List<CommandDefinition> _commands = new List<CommandDefinition>();
        private Dictionary<Type, Func<CommandContext>> _contextFactories = new Dictionary<Type, Func<CommandContext>>();

        public List<CommandDefinition> RegisteredCommands { get { return _commands; } }

        public CommandExecutor()
        {
            _loader = new CommandLoader();
            _parser = new CommandLineParser();
            _binder = new CommandBinder();
        }

        public void LoadCommands(Assembly assembly)
        {
            _commands.AddRange(_loader.LoadCommands(assembly));
        }

        public void RegisterContextFactory<T>(Func<T> factory) where T : CommandContext
        {
            _contextFactories[typeof(T)] = factory;
        }


        public async Task<CommandDefinition> GetCommand(string commandeLine)
        {

            var parsed = _parser.Parse(commandeLine);
            if (parsed == null)
                return null;

            var commandDef = _commands.FirstOrDefault(c =>
                c.Metadata.Name.Equals(parsed.Name, StringComparison.OrdinalIgnoreCase) ||
                c.Metadata.Aliases.Contains(parsed.Name, StringComparer.OrdinalIgnoreCase));

            return commandDef;
        }

        public async Task<CommandResult> ExecuteAsync(string input)
        {
            try
            {
                var parsed = _parser.Parse(input);
                if (parsed == null)
                    return CommandResult.Failure("Parsing failed (empty input?)");

                var commandDef = _commands.FirstOrDefault(c =>
                    c.Metadata.Name.Equals(parsed.Name, StringComparison.OrdinalIgnoreCase) ||
                    c.Metadata.Aliases.Contains(parsed.Name, StringComparer.OrdinalIgnoreCase));

                if (commandDef == null)
                {
                    return CommandResult.Failure($"Command '{parsed.Name}' not found.");
                }

                // CHECK FOR HELP
                if (parsed.Options.ContainsKey("h") || parsed.Options.ContainsKey("help"))
                {
                    var helpGen = new HelpGenerator();
                    // We still print Help to console as it is "Output", not "Error". 
                    // Or should we return it? The user said "retourner les erreur".
                    // Help is usually normal output.
                    Console.WriteLine(helpGen.GenerateUsage(commandDef));
                    return CommandResult.Success();
                }

                // Get Context Factory
                if (!_contextFactories.TryGetValue(commandDef.ContextType, out var factory))
                {
                    return CommandResult.Failure($"Error: Context factory for type '{commandDef.ContextType.Name}' is not registered.");
                }

                // Create Fresh Context
                var context = factory.Invoke();

                // Create and Bind Options
                var options = (CommandOption)Activator.CreateInstance(commandDef.OptionsType);
                options.CommandLine = input;
                _binder.Bind(parsed, options);

                // Instantiate Command
                var commandInstance = Activator.CreateInstance(commandDef.CommandType);

                // Execute
                var executeMethod = commandDef.CommandType.GetMethod("Execute");
                var task = (Task<bool>)executeMethod.Invoke(commandInstance, new object[] { context, options });
                bool success = await task;

                return success ? CommandResult.Success(context) : CommandResult.Failure(context);
            }
            catch (ArgumentException ex)
            {
                return CommandResult.Failure($"Error: {ex.Message}");
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the exception for cleaner content
                var inner = ex.InnerException ?? ex;
                return CommandResult.Failure($"Error executing command: {inner.Message}");
            }
            catch (Exception ex)
            {
                return CommandResult.Failure($"Unexpected error: {ex.Message}");
            }
        }

    }
}
