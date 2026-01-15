using Common.APIModels;
using Common.Command;
using Common.CommandLine.Core;
using Common.Payload;
using Shared;

namespace Common.AgentCommands
{

    public interface IAgentCommandContext
    {
        AgentTask RegisterTask(CommandId command);
        void Echo(string message);
        void Delay(int delayInSecond);
        void Shell(string cmd);
        void Powershell(string cmd);
        void Upload(byte[] fileBytes, string path);
        void Link(ConnexionUrl url);
        void PsExec(string target, string path, string service = null);

        void WinRM(string target, string winRMCommand);
        void RegistryAdd(string path, string key, string value);
        void RegistryRemove(string path, string key);
        void DeleteFile(string path);
        List<AgentTask> GetTasks();
        void AddParameter<T>(ParameterId id, T item);
        void AddParameter(ParameterId id, byte[] item);
        ParameterDictionary GetParameters();



        void WriteError(string message);
        void WriteSuccess(string message);
        void WriteLine(string message);
        void WriteInfo(string message);

        Task<APIImplant> GeneratePayload(ImplantConfig options);
        void TaskAgent(string commandLine, CommandId commandId);

        void TaskAgent(string commandLine, CommandId commandId, ParameterDictionary parameters);
        
        AgentMetadata Metadata { get; }
    }
    public class AgentCommandContext : CommandContext, IAgentCommandContext
    {
        public IAgentCommandContext Adapter { get; private set; }
        public AgentCommandContext(IAgentCommandContext adapter)
        {
            this.Adapter = adapter;
        }

        public void AddParameter<T>(ParameterId id, T item)
        {
            this.Adapter.AddParameter<T>(id, item);
        }

        public void AddParameter(ParameterId id, byte[] item)
        {
            this.Adapter.AddParameter(id, item);
        }

        public void Delay(int delayInSecond)
        {
            this.Adapter.Delay(delayInSecond);
        }

        public void DeleteFile(string path)
        {
            this.Adapter.DeleteFile(path);
        }

        public void Echo(string message)
        {
            this.Adapter.Echo(message);
        }

        public void WinRM(string target, string winRMCommand)
        {
            this.Adapter.WinRM(target, winRMCommand);
        }
        public Task<APIImplant> GeneratePayload(ImplantConfig options)
        {
            return this.Adapter.GeneratePayload(options);
        }

        public ParameterDictionary GetParameters()
        {
            return this.Adapter.GetParameters();
        }

        public List<AgentTask> GetTasks()
        {
            return this.Adapter.GetTasks();
        }

        public void Link(ConnexionUrl url)
        {
            this.Adapter.Link(url);
        }

        public void Powershell(string cmd)
        {
            this.Adapter.Powershell(cmd);
        }

        public void PsExec(string target, string path, string service = null)
        {
            this.Adapter.PsExec(target, path, service);
        }

        public AgentTask RegisterTask(CommandId command)
        {
            return this.Adapter.RegisterTask(command);
        }

        public void RegistryAdd(string path, string key, string value)
        {
            this.Adapter.RegistryAdd(path, key, value);
        }

        public void RegistryRemove(string path, string key)
        {
            this.Adapter.RegistryRemove(path, key);
        }

        public void Shell(string cmd)
        {
            this.Adapter.Shell(cmd);
        }

        public void TaskAgent(string commandLine, CommandId commandId)
        {
            this.Adapter.TaskAgent(commandLine, commandId);
        }

        public void TaskAgent(string commandLine, CommandId commandId, ParameterDictionary parameters)
        {
            this.Adapter.TaskAgent(commandLine, commandId, parameters);
        }

        public void Upload(byte[] fileBytes, string path)
        {
            this.Adapter.Upload(fileBytes, path);
        }

        public void WriteError(string message)
        {
            this.Adapter.WriteError(message);
        }

        public void WriteInfo(string message)
        {
            this.Adapter.WriteInfo(message);
        }

        public void WriteLine(string message)
        {
            this.Adapter.WriteLine(message);
        }

        public void WriteSuccess(string message)
        {
            this.Adapter.WriteSuccess(message);
        }

        public AgentMetadata Metadata => this.Adapter.Metadata;
    }
}
