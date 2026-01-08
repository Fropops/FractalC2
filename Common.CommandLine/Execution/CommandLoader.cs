using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.CommandLine.Core;

namespace Common.CommandLine.Execution
{
    public class CommandLoader
    {
        public List<CommandDefinition> LoadCommands(Assembly assembly)
        {
            var validCommands = new List<CommandDefinition>();
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<,>)));

            foreach (var type in types)
            {
                var commandAttr = type.GetCustomAttribute<CommandAttribute>();
                if (commandAttr == null)
                {
                    // Skip commands without attribute
                    continue;
                }

                // Identify Context and Options types from Interface
                var interfaceType = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<,>));
                var contextType = interfaceType.GetGenericArguments()[0];
                var optionsType = interfaceType.GetGenericArguments()[1];

                try
                {
                    ValidateOptions(optionsType);

                    validCommands.Add(new CommandDefinition
                    {
                        Metadata = commandAttr,
                        CommandType = type,
                        ContextType = contextType,
                        OptionsType = optionsType
                    });
                }
                catch (Exception ex)
                {
                    // Log or ignore invalid command
                    Console.WriteLine($"Command '{commandAttr.Name}' rejected: {ex.Message}");
                }
            }

            return validCommands;
        }

        private void ValidateOptions(Type optionsType)
        {
            var properties = optionsType.GetProperties();
            var arguments = properties
               .Select(p => p.GetCustomAttribute<ArgumentAttribute>())
               .Where(a => a != null)
               .OrderBy(a => a.Order)
               .ToList();

            // Rule: Required argument cannot be after an optional argument
            bool foundOptional = false;
            bool hasRemainder = false;
            var optionProperties = properties.Where(p => p.GetCustomAttribute<OptionAttribute>() != null).ToList();

            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];

                // Validate Remainder
                if (arg.IsRemainder)
                {
                    if (i != arguments.Count - 1)
                    {
                        throw new InvalidOperationException($"Remainder argument '{arg.Name}' must be the last argument.");
                    }
                    hasRemainder = true;
                }

                if (arg.IsRequired)
                {
                    if (foundOptional)
                    {
                        throw new InvalidOperationException($"Required argument '{arg.Name}' cannot appear after optional arguments.");
                    }
                }
                else
                {
                    foundOptional = true;
                }
            }

            // Rule: If Remainder is used, NO Options are allowed?
            // User Request: "incompatible with any options"
            if (hasRemainder && optionProperties.Any())
            {
                throw new InvalidOperationException($"Command uses a Remainder argument which is incompatible with options.");
            }

            // Additional validation if needed (e.g. duplicate Aliases in Options, etc.)
        }
    }
}
