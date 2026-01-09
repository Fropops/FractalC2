using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.AgentCommands;
using Common.APIModels;
using Common.Payload;
using Shared;
using WebCommander.Services;
using WebCommander.Models;

namespace WebCommander.Commands
{
    public class WebAgentCommandAdapter : IAgentCommandContext
    {
        private readonly TeamServerClient _client;
        private readonly Agent _agent;
        private readonly List<string> _output = new();
        private readonly List<string> _errors = new();
        
        public ParameterDictionary Parameters { get; private set; } = new ParameterDictionary();
        public List<AgentTask> Tasks { get; private set; } = new List<AgentTask>();

        public AgentMetadata Metadata => _agent.Metadata;

        // Valid only for this request
        public byte[]? CurrentFileBytes { get; private set; }

        public WebAgentCommandAdapter(TeamServerClient client, Agent agent, byte[]? fileBytes = null)
        {
            _client = client;
            _agent = agent;
            CurrentFileBytes = fileBytes;
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

        public void PsExec(string target, string path)
        {
            var task = RegisterTask(CommandId.PsExec);
            task.Parameters.AddParameter(ParameterId.Path, path);
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
            _errors.Add(message);
        }

        public void WriteSuccess(string message)
        {
             _output.Add(message);
        }

        public void WriteLine(string message)
        {
             _output.Add(message);
        }

        public void WriteInfo(string message)
        {
             _output.Add(message);
        }

        public Task<APIImplant> GeneratePayload(ImplantConfig options)
        {
            // Implementation for WebCommander payload generation if needed
            // Currently throwing NOT IMPLEMENTED or delegating to CommModule if it was available.
            // But TeamServerClient has CreateImplantAsync.
            // For now, let's assume we don't generate payloads via Agent Commands in WebCommander yet?
            // Or if we do, we need to implement it.
            // Commander uses CommModule.GenerateImplant.
            // WebCommander has TeamServerClient.CreateImplantAsync.
            throw new NotImplementedException("Payload generation not yet implemented in WebAgentCommandAdapter");
        }

        public void TaskAgent(string commandLine, CommandId commandId)
        {
            TaskAgent(commandLine, commandId, Parameters);
        }

        public void TaskAgent(string commandLine, CommandId commandId, ParameterDictionary parameters)
        {
             if (Tasks.Count > 0)
             {
                 // If tasks were queued via helper methods (Shell, Ls, etc.), send them.
                 foreach(var task in Tasks)
                 {
                     _ = _client.TaskAgent(commandLine, _agent.Id, task.CommandId, task.Parameters);
                 }
                 Tasks.Clear();
             }
             else
             {
                 // Otherwise use the parameters provided (usually from AddParameter calls)
                 _ = _client.TaskAgent(commandLine, _agent.Id, commandId, parameters);
             }
        }
        
        // Helper to get collected output
        public string GetOutput() => string.Join(Environment.NewLine, _output);
        public string GetErrors() => string.Join(Environment.NewLine, _errors);
    }
}
