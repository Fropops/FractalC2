using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Command;

namespace Commander.Commands.Custom
{
    // Générateur automatique
    public static class CommandGenerator
    {
        public static RootCommand GenerateRootCommand<T>(string description) where T : class, new()
        {
            var rootCommand = new RootCommand(description);
            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
                if (attribute == null)
                    continue;

                var propertyType = property.PropertyType;
                Option option;

                // Gestion des types bool (flags sans valeur)
                if (propertyType == typeof(bool))
                {
                    option = new Option(attribute.Aliases, attribute.Description);
                }
                // Gestion des types avec valeur par défaut
                else if (attribute.HasDefaultValue)
                {
                    if (propertyType == typeof(string))
                    {
                        option = new Option<string>(
                            attribute.Aliases,
                            () => (string)attribute.DefaultValue,
                            attribute.Description
                        );
                    }
                    else if (propertyType == typeof(int))
                    {
                        option = new Option<int>(
                            attribute.Aliases,
                            () => (int)attribute.DefaultValue,
                            attribute.Description
                        );
                    }
                    else
                    {
                        // Ajouter d'autres types si nécessaire
                        option = CreateGenericOption(propertyType, attribute);
                    }
                }
                // Gestion des types sans valeur par défaut
                else
                {
                    if (propertyType == typeof(string))
                    {
                        option = new Option<string>(attribute.Aliases, attribute.Description);
                    }
                    else if (propertyType == typeof(int))
                    {
                        option = new Option<int>(attribute.Aliases, attribute.Description);
                    }
                    else
                    {
                        option = CreateGenericOption(propertyType, attribute);
                    }
                }

                rootCommand.Add(option);
            }

            return rootCommand;
        }

        private static Option CreateGenericOption(Type propertyType, CommandOptionAttribute attribute)
        {
            var optionType = typeof(Option<>).MakeGenericType(propertyType);

            if (attribute.HasDefaultValue)
            {
                var funcType = typeof(Func<>).MakeGenericType(propertyType);
                var defaultValueFunc = Delegate.CreateDelegate(funcType,
                    typeof(CommandGenerator).GetMethod(nameof(GetDefaultValue),
                    BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(propertyType),
                    false);

                return (Option)Activator.CreateInstance(
                    optionType,
                    attribute.Aliases,
                    defaultValueFunc,
                    attribute.Description
                );
            }
            else
            {
                return (Option)Activator.CreateInstance(
                    optionType,
                    attribute.Aliases,
                    attribute.Description
                );
            }
        }

        private static T GetDefaultValue<T>() => default(T);
    }

  
}
