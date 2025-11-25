using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.APIModels.WebHost
{
    public class FileWebHost
    {
        public string Path { get; set; }
        public string Description { get; set; }

        public bool IsPowershell { get; set; }
        public byte[] Data { get; set; }
    }
}
