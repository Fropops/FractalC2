using System;
using System.Collections.Generic;
using Shared;

namespace TeamServer.Services
{

    [InjectableService]
    public interface ITaskInterceptionService
    {
        InterceptionResult Intercept(AgentTask task);
    }

    [InjectableServiceImplementation(typeof(ITaskInterceptionService))]
    public class TaskInterceptionService : ITaskInterceptionService
    {
        public List<TaskInterceptor> Interceptors { get; set; } = new List<TaskInterceptor>();

        private readonly IToolsService _toolsService;
        public TaskInterceptionService(IToolsService toolService)
        {
            this._toolsService = toolService;
            this.Interceptors.Add(new InlineAssemblyInterceptor(this._toolsService));
        }

        public InterceptionResult Intercept(AgentTask task)
        {
            foreach(var interceptor in Interceptors)
            {
                if(task.CommandId == interceptor.CommandId)
                {
                    var result = interceptor.Intercept(task);
                    if (!result.Success)
                        return result;
                }
            }
            return new InterceptionResult().Succeed();
        }
    }

    public class InterceptionResult
    {
        public bool Success { get; set;}
        public string Error { get; set;}
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
        public abstract CommandId CommandId { get;}
        public abstract InterceptionResult Intercept(AgentTask task);

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

        public override InterceptionResult Intercept(AgentTask task)
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

    
}
