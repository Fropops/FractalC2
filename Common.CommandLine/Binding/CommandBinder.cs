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

        private void BindArguments<TOptions>(ParsedCommand parsed, TOptions options, PropertyInfo[] properties) where TOptions : CommandOption
        {
            var argumentProps = properties
                .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<ArgumentAttribute>() })
                .OrderBy(x => x.Attribute.Order)
                .ToList();

            // Check if we have more arguments than properties
            // If the last argument is a Remainder, we allow more parsed arguments (they will be consumed by remainder logic)
            bool hasRemainder = argumentProps.Any() && argumentProps.Last().Attribute.IsRemainder;

            if (!hasRemainder && parsed.Arguments.Count > argumentProps.Count)
            {
                throw new ArgumentException($"Too many arguments provided. Expected {argumentProps.Count}, got {parsed.Arguments.Count}.");
            }

            for (int i = 0; i < argumentProps.Count; i++)
            {
                var target = argumentProps[i];
                // Check if this argument is 'IsRemainder'
                if (target.Attribute.IsRemainder)
                {
                    // Remainder Logic
                    // 1. It must be the last argument property (validated implicitly by being here, if we validated attributes correctly)
                    // 2. We take everything from the end of the *previous* token until the end of the string.
                    // But wait, "previous token" depends on where we are.
                    // We are at index 'i' of arguments.

                    // We need to find the CommandLineToken corresponding to the start of this remainder.
                    // The normal arguments consumed 'i' slots in parsed.Arguments.
                    // But parsed.Arguments only contains non-option tokens.
                    // Remainder effectively consumes *everything* remaining, including thinks that look like options.

                    // Actually, if IsRemainder is set, the parser might have already tokenized things as options if they started with '-'.
                    // This Binder logic runs AFTER parsing.
                    // The requirement is: "take all the rest of the command line".

                    // Challenge: The parser separates Options and Arguments. If the user typed "cmd arg1 -opt val remainder...", 
                    // 'remainder...' might have been parsed as options or arguments depending on format.
                    // BUT, the requirement says "incompatible with any options". 
                    // This means if IsRemainder is present, we shouldn't have any Options bound (or maybe even parsed as options? No, parser is generic).

                    // If IsRemainder is true, we ignore parsed.Options and parsed.Arguments logic for this field
                    // and instead go to the raw tokens.

                    // We need to identify WHERE the remainder starts.
                    // It starts after the last successfully bound positional argument.
                    // 'i' is the index of the current argument we are binding.
                    // So we bound 'i' arguments so far (indices 0 to i-1).

                    // We need to find the token corresponding to the (i-1)th argument.
                    // Actually, we can just look at the last token consumed.

                    // Let's find the end index of the Command Name (if i=0) or the last bound argument.
                    int startOffset = 0;

                    if (i == 0)
                    {
                        // No previous args, start after Command Name
                        startOffset = parsed.Tokens[0].EndIndex;
                    }
                    else
                    {
                        // Start after the (i-1)th parsed argument.
                        // But wait, parsed.Arguments indices don't map 1:1 to parsed.Tokens if there are options interleaved (though we said no options allowed).
                        // If no options allowed, parsed.Arguments map to tokens [1..N].
                        // So the (i-1)th argument corresponds to parsed.Arguments[i-1].
                        // We need to find which token that was.
                        // This is getting tricky to match back.

                        // Simpler approach:
                        // If IsRemainder is used, and "Incompatible with options", we assume the user didn't provide standard options
                        // OR we just treat them as part of the remainder.

                        // We iterate through tokens. Token 0 is Command Name.
                        // Tokens 1..i are the positional arguments we just bound.
                        // So the Remainder starts at Token[i+1].StartIndex.

                        if (i + 1 < parsed.Tokens.Count)
                        {
                            startOffset = parsed.Tokens[i+1].StartIndex;
                        }
                        else
                        {
                            // No tokens left
                            startOffset = options.CommandLine.Length;
                        }
                    }

                    if (startOffset < options.CommandLine.Length)
                    {
                        string remainder = options.CommandLine.Substring(startOffset).TrimStart(); // Trim leading separators
                        SetPropertyValue(options, target.Property, remainder);
                        // We consumed everything. Stop.
                        break;
                    }
                    else
                    {
                        if (target.Attribute.IsRequired)
                        {
                            throw new ArgumentException($"Missing required argument: {target.Attribute.Name}");
                        }

                        // Empty remainder
                        SetPropertyValue(options, target.Property, ""); // or null?
                        break;
                    }
                }

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
                        catch (Exception ex)
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
                    foreach (var allowed in target.Attribute.AllowedValues)
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

        private void BindOptions<TOptions>(ParsedCommand parsed, TOptions options, PropertyInfo[] properties) where TOptions : CommandOption
        {
            var optionProps = properties
                .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<OptionAttribute>() })
                .ToList();

            // Validate Unknown Options
            var knownOptions = new HashSet<string>();
            foreach (var opt in optionProps)
            {
                if (!string.IsNullOrEmpty(opt.Attribute.ShortName)) knownOptions.Add(opt.Attribute.ShortName);
                if (!string.IsNullOrEmpty(opt.Attribute.LongName)) knownOptions.Add(opt.Attribute.LongName);
            }

            // Check for Remainder Argument to handle "options" inside remainder
            var argumentProps = properties
                .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<ArgumentAttribute>() })
                .OrderBy(x => x.Attribute.Order)
                .ToList();

            var remainderArg = argumentProps.FirstOrDefault(a => a.Attribute.IsRemainder);
            int remainderStartOffset = -1;

            if (remainderArg != null)
            {
                // Calculate Remainder Start Offset logic (Robust)
                // Find the token corresponding to the argument BEFORE the remainder.
                // Remainder is at index 'remainderArg.Attribute.Order' (assuming strict 0..N ordering)
                // Actually easier: remainderArg is at index 'argumentProps.IndexOf(remainderArg)'.
                int remainderIndex = argumentProps.IndexOf(remainderArg);

                // We need the (remainderIndex - 1)-th positional argument token.
                int targetArgIndex = remainderIndex - 1;

                if (targetArgIndex < 0)
                {
                    // Remainder is the first argument, starts after Command Name
                    if (parsed.Tokens.Count > 0)
                        remainderStartOffset = parsed.Tokens[0].StartIndex + parsed.Tokens[0].Length;
                }
                else
                {
                    int currentArgCount = -1; // 0-based index of args found
                    for (int i = 1; i < parsed.Tokens.Count; i++) // Skip command name
                    {
                        var t = parsed.Tokens[i];
                        if (!t.Value.StartsWith("-"))
                        {
                            currentArgCount++;
                            if (currentArgCount == targetArgIndex)
                            {
                                remainderStartOffset = t.StartIndex + t.Length;
                                break;
                            }
                        }
                    }
                }

                // Fallback if not found (e.g. not enough args provided? then remainder starts at end?)
                if (remainderStartOffset == -1) remainderStartOffset = int.MaxValue;
            }

            foreach (var parsedOpt in parsed.Options.Keys)
            {
                if (!knownOptions.Contains(parsedOpt))
                {
                    bool isIgnorable = false;
                    if (remainderArg != null)
                    {
                        // Check if this option instance is appearing ONLY after remainderStartOffset
                        bool allInRemainder = true;
                        bool foundAny = false;

                        foreach (var token in parsed.Tokens)
                        {
                            if (token.Value.StartsWith("-"))
                            {
                                string name = token.Value.TrimStart('-');
                                // Simple match. Note: parsed.Options keys are simple names.
                                // If token is --foo, name=foo.
                                if (name == parsedOpt)
                                {
                                    foundAny = true;
                                    if (token.StartIndex < remainderStartOffset)
                                    {
                                        allInRemainder = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (foundAny && allInRemainder)
                        {
                            isIgnorable = true;
                        }
                    }

                    if (!isIgnorable)
                    {
                        throw new ArgumentException($"Unknown option: --{parsedOpt} (or -{parsedOpt})");
                    }
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
