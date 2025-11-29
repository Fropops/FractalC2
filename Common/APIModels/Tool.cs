using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.APIModels
{
    public enum ToolType
    {
        Exe = 0,
        DotNet,
        Powershell
    }
    public class Tool
    {
        public string Name { get; set; }
        public ToolType Type { get; set; }
        public string Data { get; set; }
    }
}
