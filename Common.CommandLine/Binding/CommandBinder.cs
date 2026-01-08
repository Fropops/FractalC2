using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Common.CommandLine.Core;
using Common.CommandLine.Parsing;

namespace Common.CommandLine.Binding
{
    public class CommandBinder
    {
        public void Bind<TOptions>(ParsedCommand parsed, TOptions options) where TOptions : CommandOption
        {
            var properties = options.GetType().GetProperties();
            
            // 1. Bind Arguments
            BindArguments(parsed, options, properties);

            // 2. Bind Options
            BindOptions(parsed, options, properties);
        }

        private void BindArguments<TOptions>(ParsedCommand parsed, TOptions options, PropertyInfo[] properties)
        {
            var argumentProps = properties
                .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<ArgumentAttribute>() })
                .OrderBy(x => x.Attribute.Order)
                .ToList();

            // Check if we have more arguments than properties
            if (parsed.Arguments.Count > argumentProps.Count)
            {
                throw new ArgumentException($"Too many arguments provided. Expected {argumentProps.Count}, got {parsed.Arguments.Count}.");
            }

            for (int i = 0; i < argumentProps.Count; i++)
            {
                var target = argumentProps[i];
                string value = null;

                if (i < parsed.Arguments.Count)
                {
                    value = parsed.Arguments[i];
                }
                else
                {
                    // No input provided for this argument
                    if (target.Attribute.DefaultValue != null)
                    {
                        // Use default value
                         try 
                         {
                             // If DefaultValue is provided, set it. 
                             // We might need conversion if the DefaultValue type doesn't match Property type exactly (e.g. integer 5 vs "5")
                             // But usually attribute values match basic types.
                             // For simplicity, we assume compatible types or simple conversion.
                             if (target.Attribute.DefaultValue.GetType() == target.Property.PropertyType)
                             {
                                  target.Property.SetValue(options, target.Attribute.DefaultValue);
                             }
                             else
                             {
                                  target.Property.SetValue(options, Convert.ChangeType(target.Attribute.DefaultValue, target.Property.PropertyType));
                             }
                             
                             continue; // Proceed to next argument
                         }
                         catch(Exception ex)
                         {
                             throw new ArgumentException($"Failed to apply default value for argument '{target.Attribute.Name}'.", ex);
                         }
                    }

                    if (target.Attribute.IsRequired)
                    {
                         throw new ArgumentException($"Missing required argument: {target.Attribute.Name}");
                    }
                    
                    // If not required and no default, just skip (stays null/default)
                    continue;
                }

                // If we have a value (from connection), validate AllowedValues
                if (target.Attribute.AllowedValues != null && target.Attribute.AllowedValues.Length > 0)
                {
                    bool isValid = false;
                    foreach(var allowed in target.Attribute.AllowedValues)
                    {
                        if (allowed.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (!isValid)
                    {
                         throw new ArgumentException($"Invalid value '{value}' for argument '{target.Attribute.Name}'. Allowed values are: {string.Join(", ", target.Attribute.AllowedValues)}");
                    }
                }

                SetPropertyValue(options, target.Property, value);
            }
        }

        private void BindOptions<TOptions>(ParsedCommand parsed, TOptions options, PropertyInfo[] properties)
        {
            var optionProps = properties
                .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<OptionAttribute>() })
                .ToList();

            // Validate Unknown Options
            var knownOptions = new HashSet<string>();
            foreach(var opt in optionProps)
            {
                if(!string.IsNullOrEmpty(opt.Attribute.ShortName)) knownOptions.Add(opt.Attribute.ShortName);
                if(!string.IsNullOrEmpty(opt.Attribute.LongName)) knownOptions.Add(opt.Attribute.LongName);
            }

            foreach(var parsedOpt in parsed.Options.Keys)
            {
                if (!knownOptions.Contains(parsedOpt))
                {
                    throw new ArgumentException($"Unknown option: --{parsedOpt} (or -{parsedOpt})");
                }
            }

            foreach (var opt in optionProps)
            {
                string key = null;
                // Check if option is present by ShortName or LongName
                if (parsed.Options.ContainsKey(opt.Attribute.ShortName ?? ""))
                {
                    key = opt.Attribute.ShortName;
                }
                else if (parsed.Options.ContainsKey(opt.Attribute.LongName ?? ""))
                {
                    key = opt.Attribute.LongName;
                }

                if (key != null)
                {
                    string value = parsed.Options[key];
                    // Validate AllowedValues
                    if (opt.Attribute.AllowedValues != null && opt.Attribute.AllowedValues.Length > 0)
                    {
                        bool isValid = false;
                        foreach (var allowed in opt.Attribute.AllowedValues)
                        {
                            if (allowed.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                            {
                                isValid = true;
                                break;
                            }
                        }

                        if (!isValid)
                        {
                            var name = !string.IsNullOrEmpty(opt.Attribute.LongName) ? $"--{opt.Attribute.LongName}" : $"-{opt.Attribute.ShortName}";
                            throw new ArgumentException($"Invalid value '{value}' for option '{name}'. Allowed values are: {string.Join(", ", opt.Attribute.AllowedValues)}");
                        }
                    }
                    SetPropertyValue(options, opt.Property, value);
                }
                else
                {
                    // Option not provided
                    if (opt.Attribute.IsRequired)
                    {
                        var name = !string.IsNullOrEmpty(opt.Attribute.LongName) ? $"--{opt.Attribute.LongName}" : $"-{opt.Attribute.ShortName}";
                        throw new ArgumentException($"Missing required option: {name}");
                    }

                    // Apply default value if present
                    if (opt.Attribute.DefaultValue != null)
                    {
                         opt.Property.SetValue(options, opt.Attribute.DefaultValue);
                    }
                }
            }
        }

        private void SetPropertyValue(object target, PropertyInfo property, string value)
        {
            try
            {
                Type targetType = property.PropertyType;

                // Handle Nullable Types
                if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        property.SetValue(target, null);
                        return;
                    }
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                if (targetType == typeof(string))
                {
                    property.SetValue(target, value);
                }
                else if (targetType == typeof(int))
                {
                    property.SetValue(target, int.Parse(value));
                }
                else if (targetType == typeof(bool))
                {
                    property.SetValue(target, bool.Parse(value));
                }
                // Add more types as needed (enum, etc.)
                else
                {
                    // Fallback to ChangeType
                    property.SetValue(target, Convert.ChangeType(value, targetType));
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to bind value '{value}' to property '{property.Name}' of type '{property.PropertyType.Name}'.", ex);
            }
        }
    }
}
