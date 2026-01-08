using System;

namespace Common.CommandLine.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string Category { get; set; } = "General";
        public string[] Aliases { get; set; } = Array.Empty<string>();

        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ArgumentAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public bool IsRequired { get; set; } = false;
        public int Order { get; }
        public object DefaultValue { get; set; }
        public object[] AllowedValues { get; set; }

        public ArgumentAttribute(string name, string description, int order)
        {
            Name = name;
            Description = description;
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public string ShortName { get; }
        public string LongName { get; }
        public string Description { get; }
        public object DefaultValue { get; set; }
        public bool IsRequired { get; set; } = false;
        public object[] AllowedValues { get; set; }

        public OptionAttribute(string shortName, string longName, string description)
        {
            ShortName = shortName;
            LongName = longName;
            Description = description;
        }
    }
}
