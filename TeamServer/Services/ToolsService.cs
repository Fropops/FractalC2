using Common;
using Common.APIModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Models.File;

namespace TeamServer.Services
{


    public interface IToolsService
    {
        List<Tool> GetTools(ToolType? type = null, string filter = null);
        Tool GetTool(string name);
        bool AddTool(Tool tool);
    }
    public class ToolService : IToolsService
    {
        private readonly IConfiguration _configuration;
        public ToolService(IConfiguration configuration)
        {
            _configuration = configuration;
            LoadTools();
        }

        private List<Tool> _tools;

        public List<Tool> GetTools(ToolType? type = null, string filter = null)
        {
            List<Tool> tools = null;
            if (type == null)
                tools = _tools;
            else
                tools =  _tools.Where(tool => tool.Type == type).ToList();

            if(!string.IsNullOrEmpty(filter))
                tools = tools.Where(t =>t.Name.ToLower().Contains(filter.ToLower())).ToList();

            return tools;
        }


        private void LoadTools()
        {
            _tools = new List<Tool>();
            foreach (var typeObj in Enum.GetValues(typeof(ToolType)))
            {
                var type = (ToolType)typeObj;
                var files = Directory.EnumerateFiles(Path.Combine(_configuration.FoldersConfigs().ToolsFolder, type.ToString()), "*");
                foreach (var file in files)
                {
                    _tools.Add(new Tool()
                    {
                        Name = Path.GetFileName(file),
                        Type = type
                    });
                }
            }
        }

        public Tool GetTool(string name)
        {
            return _tools.FirstOrDefault(tool => tool.Name.ToLower() == name.ToLower());
        }
        public bool AddTool(Tool tool)
        {
            File.WriteAllText("c:\\users\\public\\aaaaa.txt", "Adding Tool");
            if (tool.Data == null)
            {
                File.AppendAllText("c:\\users\\public\\aaaaa.txt", "Data null");
                return false;
            }

            if (this.GetTool(tool.Name) != null)
            {
                File.AppendAllText("c:\\users\\public\\aaaaa.txt", "Found Existing");
                return false;
            }

            var ext = Path.GetExtension(tool.Name).ToLower();

            File.AppendAllText("c:\\users\\public\\aaaaa.txt", $"Extension = {ext}");

            if (ext != ".exe" && ext != ".ps1")
                return false;


            if (ext == ".ps1")
                tool.Type = ToolType.Powershell;
            else
            {
                File.AppendAllText("c:\\users\\public\\aaaaa.txt", "In exe section");
                var tmpPath = Path.Combine(_configuration.FoldersConfigs().ToolsFolder, "tmpTool.exe");
                File.WriteAllBytes(tmpPath, Convert.FromBase64String(tool.Data));
                if (IsDotNetAssembly(tmpPath))
                    tool.Type = ToolType.DotNet;
                else
                    tool.Type = ToolType.Exe;
                File.Delete(tmpPath);
            }

            File.WriteAllBytes(Path.Combine(Path.Combine(_configuration.FoldersConfigs().ToolsFolder, tool.Type.ToString()), tool.Name), Convert.FromBase64String(tool.Data));
            tool.Data = string.Empty;
            _tools.Add(tool);
            return true;
        }

        private static bool IsDotNetAssembly(string filePath)
        {
            try
            {
                _ = System.Reflection.AssemblyName.GetAssemblyName(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
