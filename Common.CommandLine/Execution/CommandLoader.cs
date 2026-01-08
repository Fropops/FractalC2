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
             foreach (var arg in arguments)
             {
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

             // Additional validation if needed (e.g. duplicate Aliases in Options, etc.)
        }
    }
}
