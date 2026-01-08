using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.CommandLine.Core;
using Common.CommandLine.Execution;

namespace Common.CommandLine.Help
{
    public class HelpGenerator
    {
        public string GenerateUsage(CommandDefinition command)
        {
            var sb = new StringBuilder();

            // Usage Line
            sb.Append($"Usage: {command.Metadata.Name}");

            // Arguments in Usage
            var args = command.OptionsType.GetProperties()
                .Select(p => p.GetCustomAttribute<ArgumentAttribute>())
                .Where(a => a != null)
                .OrderBy(a => a.Order)
                .ToList();

            foreach (var arg in args)
            {
                if (arg.IsRequired)
                    sb.Append($" <{arg.Name}>");
                else
                    sb.Append($" [{arg.Name}]");
            }

            sb.Append(" [options]");
            sb.AppendLine();
            sb.AppendLine();

            // Description
            if (!string.IsNullOrWhiteSpace(command.Metadata.Description))
            {
                sb.AppendLine(command.Metadata.Description);
                sb.AppendLine();
            }

            // Arguments Details
            if (args.Any())
            {
                sb.AppendLine("Arguments:");
                foreach (var arg in args)
                {
                    string req = arg.IsRequired ? "(Required)" : "(Optional)";
                    
                    var extras = new StringBuilder();
                    if (arg.DefaultValue != null)
                    {
                        extras.Append($" [Default: {arg.DefaultValue}]");
                    }
                    if (arg.AllowedValues != null && arg.AllowedValues.Length > 0)
                    {
                        extras.Append($" [Allowed: {string.Join(", ", arg.AllowedValues)}]");
                    }

                    sb.AppendLine($"  {arg.Name,-15} {arg.Description} {req}{extras}");
                }
                sb.AppendLine();
            }

            // Options Details
            var options = command.OptionsType.GetProperties()
                .Select(p => p.GetCustomAttribute<OptionAttribute>())
                .Where(o => o != null)
                .OrderBy(o => o.LongName)
                .ToList();

            if (options.Any())
            {
                sb.AppendLine("Options:");
                foreach (var opt in options)
                {
                    string shortFlag = !string.IsNullOrEmpty(opt.ShortName) ? $"-{opt.ShortName}, " : "    ";
                    string longFlag = $"--{opt.LongName}";
                    string flags = $"{shortFlag}{longFlag}";
                    string req = opt.IsRequired ? "(Required)" : "";
                    string def = opt.DefaultValue != null ? $" [Default: {opt.DefaultValue}]" : "";
                    string allowed = (opt.AllowedValues != null && opt.AllowedValues.Length > 0) ? $" [Allowed: {string.Join(", ", opt.AllowedValues)}]" : "";

                    sb.AppendLine($"  {flags,-25} {opt.Description} {req}{def}{allowed}");
                }
            }

            return sb.ToString();
        }

        public string GenerateList(System.Collections.Generic.List<CommandDefinition> commands)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Available Commands:");
            
            var categories = commands.GroupBy(c => c.Metadata.Category).OrderBy(c => c.Key);
            
            foreach(var category in categories)
            {
                sb.AppendLine();
                sb.AppendLine($"[{category.Key}]");
                foreach (var cmd in category.OrderBy(c => c.Metadata.Name))
                {
                    sb.AppendLine($"  {cmd.Metadata.Name,-15} {cmd.Metadata.Description}");
                }
            }
            
            return sb.ToString();
        }
    }
}
