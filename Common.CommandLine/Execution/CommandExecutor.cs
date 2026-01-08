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

        public CommandExecutor()
        {
            _loader = new CommandLoader();
            _parser = new CommandLineParser();
            _binder = new CommandBinder();
        }

        public List<CommandDefinition> RegisteredCommands { get { return _commands; } }

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

        public async Task<bool> Execute(string commandeLine)
        {
            try
            {
                var parsed = _parser.Parse(commandeLine);
                if (parsed == null)
                    return false;

                var commandDef = _commands.FirstOrDefault(c =>
                    c.Metadata.Name.Equals(parsed.Name, StringComparison.OrdinalIgnoreCase) ||
                    c.Metadata.Aliases.Contains(parsed.Name, StringComparer.OrdinalIgnoreCase));

                if (commandDef == null)
                {
                    Console.WriteLine($"Command '{parsed.Name}' not found.");
                    return false;
                }

                // CHECK FOR HELP
                if (parsed.Options.ContainsKey("h") || parsed.Options.ContainsKey("help"))
                {
                    var helpGen = new HelpGenerator();
                    Console.WriteLine(helpGen.GenerateUsage(commandDef));
                    return false;
                }

                // Get Context Factory
                if (!_contextFactories.TryGetValue(commandDef.ContextType, out var factory))
                {
                    Console.WriteLine($"Error: Context factory for type '{commandDef.ContextType.Name}' is not registered.");
                    return false;
                }

                // Create Fresh Context
                var context = factory.Invoke();

                // Create and Bind Options
                var options = (CommandOption)Activator.CreateInstance(commandDef.OptionsType);
                options.CommandLine = commandeLine;
                _binder.Bind(parsed, options);

                // Instantiate Command
                var commandInstance = Activator.CreateInstance(commandDef.CommandType);

                // Execute
                var executeMethod = commandDef.CommandType.GetMethod("Execute");
                var task = (Task<bool>)executeMethod.Invoke(commandInstance, new object[] { context, options });
                await task;
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine($"Error executing command: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

    }
}
