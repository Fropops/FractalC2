using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Config
{
    public class SpawnConfig
    {
        public string SpawnToX86 { get; set; }
        public string SpawnToX64 { get; set; }

        public void FromSection(IConfigurationSection section)
        {
            this.SpawnToX86 = section.GetValue<string>("SpawnToX86", "powershell.exe");
            this.SpawnToX64 = section.GetValue<string>("SpawnToX64", "powershell.exe");
        }
    }
}
