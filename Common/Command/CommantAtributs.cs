using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Command
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CommandOptionAttribute : Attribute
    {
        public string[] Aliases { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }
        public bool HasDefaultValue { get; set; }

        public CommandOptionAttribute(string shortAlias, string longAlias, string description)
        {
            Aliases = new[] { longAlias, shortAlias };
            Description = description;
            HasDefaultValue = false;
        }

        public CommandOptionAttribute(string shortAlias, string longAlias, string description, object defaultValue)
        {
            Aliases = new[] { longAlias, shortAlias };
            Description = description;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }
    }
}
