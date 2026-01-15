using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.Models;
using Common.APIModels;
using Common.Payload;
using Shared;
using WebCommander.Services;
using WebCommander.Models;

namespace WebCommander.Commands
{
    public class WebAgentCommandAdapter : IAgentCommandContext
    {
        public enum OutputType
        {
            Info,
            Success,
            Error,
            Normal
        }

        private readonly TeamServerClient _client;
        private readonly Agent _agent;
        private readonly CommandService _commandService;
        private readonly List<Tuple<OutputType, string>> _output = new();
        public List<Tuple<OutputType, string>> Outputs { get { return _output; } }

        public ParameterDictionary Parameters { get; private set; } = new ParameterDictionary();
        public List<AgentTask> Tasks { get; private set; } = new List<AgentTask>();

        public AgentMetadata Metadata => _agent.Metadata;

        // Valid only for this request
        public byte[]? CurrentFileBytes { get; private set; }

        public WebAgentCommandAdapter(TeamServerClient client, Agent agent, CommandService commandService, byte[]? fileBytes = null)
        {
            _client = client;
            _agent = agent;
            _commandService = commandService;
            CurrentFileBytes = fileBytes;
        }

        public List<Common.CommandLine.Execution.CommandDefinition> GetAvailableCommands()
        {
            return _commandService.GetCommands();
        }

        public AgentTask RegisterTask(CommandId command)
        {
            var task = new AgentTask()
            {
                Id = Guid.NewGuid().ToString(),
                CommandId = command,
                Parameters = new ParameterDictionary()
            };
            Tasks.Add(task);
            return task;
        }

        public void Echo(string message)
        {
            var task = RegisterTask(CommandId.Echo);
            task.Parameters.AddParameter(ParameterId.Parameters, message);
        }

        public void Delay(int delayInSecond)
        {
            var task = RegisterTask(CommandId.Delay);
            task.Parameters.AddParameter(ParameterId.Delay, delayInSecond);
        }

        public void Shell(string cmd)
        {
            var task = RegisterTask(CommandId.Shell);
            task.Parameters.AddParameter(ParameterId.Command, cmd);
        }

        public void Powershell(string cmd)
        {
            var task = RegisterTask(CommandId.Powershell);
            task.Parameters.AddParameter(ParameterId.Command, cmd);
        }

        public void Upload(byte[] fileBytes, string path)
        {
            var task = RegisterTask(CommandId.Upload);
            task.Parameters.AddParameter(ParameterId.Name, path);
            task.Parameters.AddParameter(ParameterId.File, fileBytes);
        }

        public void Link(ConnexionUrl url)
        {
            var task = RegisterTask(CommandId.Link);
            task.Parameters.AddParameter(ParameterId.Verb, CommandVerbs.Start);
            task.Parameters.AddParameter(ParameterId.Bind, url.ToString());
        }

        public void PsExec(string target, string path, string service = null)
        {
            var task = RegisterTask(CommandId.PsExec);
            task.Parameters.AddParameter(ParameterId.Path, path);
            task.Parameters.AddParameter(ParameterId.Target, target);
            if (!string.IsNullOrEmpty(service))
                task.Parameters.AddParameter(ParameterId.Service, service);
        }

        public void WinRM(string target, string winRMCommand)
        {
            var task = this.RegisterTask(CommandId.Winrm);
            task.Parameters.AddParameter(ParameterId.Command, winRMCommand);
            task.Parameters.AddParameter(ParameterId.Target, target);
        }

        public void RegistryAdd(string path, string key, string value)
        {
            var task = RegisterTask(CommandId.Reg);
            task.Parameters.AddParameter(ParameterId.Verb, CommandVerbs.Add);
            task.Parameters.AddParameter(ParameterId.Path, path);
            task.Parameters.AddParameter(ParameterId.Key, key);
            task.Parameters.AddParameter(ParameterId.Value, value);
        }

        public void RegistryRemove(string path, string key)
        {
            var task = RegisterTask(CommandId.Reg);
            task.Parameters.AddParameter(ParameterId.Verb, CommandVerbs.Remove);
            task.Parameters.AddParameter(ParameterId.Path, path);
            task.Parameters.AddParameter(ParameterId.Key, key);
        }

        public void DeleteFile(string path)
        {
            var task = RegisterTask(CommandId.Del);
            task.Parameters.AddParameter(ParameterId.Path, path);
        }

        public List<AgentTask> GetTasks()
        {
            return Tasks;
        }

        public void AddParameter<T>(ParameterId id, T item)
        {
            Parameters.AddParameter(id, item);
        }

        public void AddParameter(ParameterId id, byte[] item)
        {
            Parameters.AddParameter(id, item);
        }

        public ParameterDictionary GetParameters()
        {
            return Parameters;
        }

        public void WriteError(string message)
        {
            // For WebCommander, we accumulate errors to return to the caller (CommandService/Terminal)
            _output.Add(new Tuple<OutputType, string>(OutputType.Error, message));
        }

        public void WriteSuccess(string message)
        {
            _output.Add(new Tuple<OutputType, string>(OutputType.Success, message));
        }

        public void WriteLine(string message)
        {
            _output.Add(new Tuple<OutputType, string>(OutputType.Normal, message));
        }

        public void WriteInfo(string message)
        {
            _output.Add(new Tuple<OutputType, string>(OutputType.Info, message));
        }

        public async Task<APIImplant> GeneratePayload(ImplantConfig options)
        {
            var (succeed, res) =  await _client.CreateImplantAsync(options);
            if(!succeed)
            {
                WriteError($"[X] Generation Failed: {res.Logs}");
                return null;
            }

            return res.Implant;
        }

        public void TaskAgent(string commandLine, CommandId commandId)
        {
            TaskAgent(commandLine, commandId, this.Parameters);
        }

        public void TaskAgent(string commandLine, CommandId commandId, ParameterDictionary parameters)
        {
            _ = _client.TaskAgent(commandLine, _agent.Id, commandId, parameters);
            this.WriteSuccess($"Command {commandLine} tasked to agent {this.Metadata?.Name}.");
        }
    }
}
