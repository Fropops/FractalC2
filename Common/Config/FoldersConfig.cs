using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Config
{
    public class FoldersConfig
    {
        public string DBFolder { get; set; }
        public string LootFolder { get; set; }
        public string AuditFolder { get; set; }
        public string ToolsFolder { get; set; }
        public string ImplantTemplatesFolder { get; set; }
        public string ImplantsFolder { get; set; }
        public string WorkingFolder { get; set; }
        public string IncRustFolder { get; set; }
        public string DonutFolder { get; set; }
        public string PythonFolder { get; set; }

        public string NewTempFile()
        {
            return Path.Combine(this.WorkingFolder, ShortGuid.NewGuid());
        }

        public void FromSection(IConfigurationSection section, bool verbose = false)
        {
            this.DBFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("DBFolder", "/tmp/DB"));
            this.LootFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("LootFolder", "/tmp/Files"));
            this.AuditFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("AuditFolder", "/tmp/Audit"));
            this.ToolsFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("ToolsFolder", "/tmp/Tools"));
            this.ImplantTemplatesFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("ImplantTemplatesFolder", "/tmp/ImplantTemplates"));
            this.ImplantsFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("ImplantsFolder", "/tmp/Implants"));
            this.WorkingFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("WorkingFolder", "/tmp"));
            this.DonutFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("DonutFolder", "/opt/donut"));
            this.PythonFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("PythonFolder", "/opt/pyenv"));
            this.IncRustFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("IncRustFolder", "/mnt/Share/Projects/Rust/incrust"));

            if (verbose)
            {
                Console.WriteLine("[CONFIG][PAYLOAD][DBFolder] : " + this.DBFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][FilesFolder] : " + this.LootFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][AuditFolder] : " + this.AuditFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][ToolsFolder] : " + this.ToolsFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][ImplantTemplateFolder] : " + this.ImplantTemplatesFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][ImplantsFolder] : " + this.ImplantsFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][WorkingFolder] : " + this.WorkingFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][DonutPath] : " + this.DonutFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][PythonFolder] : " + this.PythonFolder);
            }

            if (!Directory.Exists(this.DBFolder)) { Directory.CreateDirectory(this.DBFolder); }
            if (!Directory.Exists(this.ImplantsFolder)) { Directory.CreateDirectory(this.ImplantsFolder); }
            if (!Directory.Exists(this.WorkingFolder)) { Directory.CreateDirectory(this.WorkingFolder); }
            if (!Directory.Exists(this.LootFolder)) { Directory.CreateDirectory(this.LootFolder); }
            if (!Directory.Exists(this.AuditFolder)) { Directory.CreateDirectory(this.AuditFolder); }

            Directory.SetCurrentDirectory(this.WorkingFolder);
        }

       
    }
}
