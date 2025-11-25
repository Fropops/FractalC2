using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Config
{
    public class PayloadConfig
    {
        public string PayloadTemplatesFolder { get; set; }
        public string ImplantsFolder { get; set; }
        public string WorkingFolder { get; set; }
        public string IncRustFolder { get; set; }
        public string DonutFolder { get; set; }
        public string PythonFolder { get; set; }

        public void FromSection(IConfigurationSection section, bool verbose = false)
        {
            this.PayloadTemplatesFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("PayloadTemplatesFolder"));
            this.ImplantsFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("ImplantsFolder", "/tmp"));
            this.WorkingFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("WorkingFolder", "/tmp"));
            this.DonutFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("DonutFolder", "/opt/donut"));
            this.PythonFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("PythonFolder", "/opt/pyenv"));
            this.IncRustFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("IncRustFolder", "/mnt/Share/Projects/Rust/incrust"));

            if (verbose)
            {
                Console.WriteLine("[CONFIG][PAYLOAD][SourceFolder] : " + this.PayloadTemplatesFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][ImplantsFolder] : " + this.ImplantsFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][WorkingFolder] : " + this.WorkingFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][DonutPath] : " + this.DonutFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][PythonFolder] : " + this.PythonFolder);
            }

            if(!Directory.Exists(this.ImplantsFolder)) { Directory.CreateDirectory(this.ImplantsFolder); }
            if (!Directory.Exists(this.WorkingFolder)) { Directory.CreateDirectory(this.WorkingFolder); }

            Directory.SetCurrentDirectory(this.WorkingFolder);
        }

       
    }
}
