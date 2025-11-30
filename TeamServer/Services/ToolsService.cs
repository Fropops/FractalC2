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

namespace TeamServer.Services
{

    [InjectableService]
    public interface IToolsService
    {
        List<Tool> GetTools(ToolType? type = null, string filter = null);
        Tool GetTool(string name, bool withData = false);
        bool AddTool(Tool tool);
    }
    [InjectableServiceImplementation(typeof(IToolsService))]
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

        public Tool GetTool(string name, bool withData = false)
        {
            var tool = _tools.FirstOrDefault(tool => tool.Name.ToLower() == name.ToLower());
            if (tool != null && withData)
                tool.Data = Convert.ToBase64String(File.ReadAllBytes(GetToolPath(tool)));
            return tool;
        }

        private string GetToolPath(Tool tool)
        {
            return Path.Combine(Path.Combine(_configuration.FoldersConfigs().ToolsFolder, tool.Type.ToString()), tool.Name);
        }
        public bool AddTool(Tool tool)
        {
            if (tool.Data == null)
            {
                return false;
            }

            if (this.GetTool(tool.Name) != null)
            {
                return false;
            }

            var ext = Path.GetExtension(tool.Name).ToLower();


            if (ext != ".exe" && ext != ".ps1")
                return false;


            if (ext == ".ps1")
                tool.Type = ToolType.Powershell;
            else
            {
                var tmpPath = Path.Combine(_configuration.FoldersConfigs().ToolsFolder, "tmpTool.exe");
                File.WriteAllBytes(tmpPath, Convert.FromBase64String(tool.Data));
                if (IsDotNetAssembly(tmpPath))
                    tool.Type = ToolType.DotNet;
                else
                    tool.Type = ToolType.Exe;
                File.Delete(tmpPath);
            }

            File.WriteAllBytes(GetToolPath(tool), Convert.FromBase64String(tool.Data));
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
