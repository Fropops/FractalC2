using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.AgentCommands;
using Common.APIModels;
using Common.Payload;
using Shared;
using Spectre.Console;

namespace Commander.Commands.Agent
{
    internal class AgentCommandAdapter : IAgentCommandContext
    {
        private IExecutor Executor;
        private ITerminal Terminal;
        private ICommModule CommModule;

        private List<AgentTask> tasks = new List<AgentTask>();

        ParameterDictionary Parameters = new ParameterDictionary();

        public AgentMetadata Metadata
        {
            get { return this.Executor.CurrentAgent.Metadata; }
        }

        public AgentCommandAdapter(IExecutor exec, ITerminal terminal, ICommModule commModule)
        {
            this.Executor = exec;
            this.Terminal = terminal;
            this.CommModule = commModule;
        }

        public AgentTask RegisterTask(CommandId command)
        {
            var task = new AgentTask()
            {
                Id = Guid.NewGuid().ToString(),
                CommandId = command,
            };
            task.Parameters = new ParameterDictionary();
            this.tasks.Add(task);
            return task;
        }

        public void Echo(string message)
        {
            var task = this.RegisterTask(CommandId.Echo);
            task.Parameters.AddParameter(ParameterId.Parameters, message);
        }

        public void Delay(int delayInSecond)
        {
            var task = this.RegisterTask(CommandId.Delay);
            task.Parameters.AddParameter(ParameterId.Delay, delayInSecond);
        }

        public void Shell(string cmd)
        {
            var task = this.RegisterTask(CommandId.Shell);
            task.Parameters.AddParameter(ParameterId.Command, cmd);
        }

        public void Powershell(string cmd)
        {
            var task = this.RegisterTask(CommandId.Powershell);
            task.Parameters.AddParameter(ParameterId.Command, cmd);
        }

        public void Upload(byte[] fileBytes, string path)
        {
            var task = this.RegisterTask(CommandId.Upload);
            task.Parameters.AddParameter(ParameterId.Name, path);
            task.Parameters.AddParameter(ParameterId.File, fileBytes);
        }

        public void Link(ConnexionUrl url)
        {
            var task = this.RegisterTask(CommandId.Link);
            task.Parameters.AddParameter(ParameterId.Verb, CommandVerbs.Start);
            task.Parameters.AddParameter(ParameterId.Bind, url.ToString());
        }

        public void PsExec(string target, string path)
        {
            var task = this.RegisterTask(CommandId.PsExec);
            task.Parameters.AddParameter(ParameterId.Path, path);
            task.Parameters.AddParameter(ParameterId.Target, target);
        }

        public void RegistryAdd(string path, string key, string value)
        {
            var task = this.RegisterTask(CommandId.Reg);
            task.Parameters.AddParameter(ParameterId.Verb, CommandVerbs.Add);
            task.Parameters.AddParameter(ParameterId.Path, path);
            task.Parameters.AddParameter(ParameterId.Key, key);
            task.Parameters.AddParameter(ParameterId.Value, value);
        }

        public void RegistryRemove(string path, string key)
        {
            var task = this.RegisterTask(CommandId.Reg);
            task.Parameters.AddParameter(ParameterId.Verb, CommandVerbs.Remove);
            task.Parameters.AddParameter(ParameterId.Path, path);
            task.Parameters.AddParameter(ParameterId.Key, key);
        }

        public void DeleteFile(string path)
        {
            var task = this.RegisterTask(CommandId.Del);
            task.Parameters.AddParameter(ParameterId.Path, path);
        }

        public List<AgentTask> GetTasks()
        {
            return this.tasks;
        }

        public void AddParameter<T>(ParameterId id, T item)
        {
            this.Parameters.AddParameter(id, item);
        }

        public void AddParameter(ParameterId id, byte[] item)
        {
            this.Parameters.AddParameter(id, item);
        }

        public ParameterDictionary GetParameters()
        {
            return this.Parameters;
        }



        public void WriteError(string message)
        {
            this.Terminal.WriteError(message);
        }
        public void WriteSuccess(string message)
        {
            this.Terminal.WriteSuccess(message);
        }
        public void WriteLine(string message)
        {
            this.Terminal.WriteLine(message);
        }
        public void WriteInfo(string message)
        {
            this.Terminal.WriteInfo(message);
        }

        public Task<APIImplant> GeneratePayload(ImplantConfig options)
        {
            return Task.FromResult(GeneratePayloadAndDisplay(options));
        }

        public void TaskAgent(string commandLine, CommandId commandId)
        {
            TaskAgent(commandLine, commandId, this.Parameters);
        }

        public void TaskAgent(string commandLine, CommandId commandId, ParameterDictionary parameters)
        {
            this.CommModule.TaskAgent(commandLine, this.Executor.CurrentAgent.Id, commandId, parameters).Wait();
            this.Terminal.WriteSuccess($"Command {commandLine} tasked to agent {this.Executor.CurrentAgent?.Metadata?.Name}.");
        }

        internal APIImplant GeneratePayloadAndDisplay(ImplantConfig options)
        {
            APIImplant implant = null;
            AnsiConsole.Status()
                    .Start($"[olive]Generating Payload {options.Type} for Endpoint {options.Endpoint} (arch = {options.Architecture}).[/]", ctx =>
                    {
                        if (string.IsNullOrEmpty(options.ImplantName))
                            options.ImplantName = PayloadGenerator.GenerateImplantName();

                        try
                        {
                            this.Terminal.WriteInfo("Triggering server-side generation...");
                            var result = this.CommModule.GenerateImplant(options).GetAwaiter().GetResult();
                            implant = result.Implant;
                        }
                        catch (Exception ex)
                        {
                            this.Terminal.WriteError($"[X] Generation Failed: {ex.Message}");
                            if (options.IsVerbose) this.Terminal.WriteError(ex.ToString());
                        }
                    });



            return implant;
        }
    }
}
