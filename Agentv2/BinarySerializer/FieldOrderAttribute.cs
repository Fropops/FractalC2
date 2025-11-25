using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FieldOrderAttribute : Attribute
    {
        public int Order { get; set; }
        public FieldOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
