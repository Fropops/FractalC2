using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Commander.Commands.Scripted
{
    public class ScriptingAgent<T>
    {
        private CommandContext<T> context;

        private List<AgentTask> tasks;

        public AgentMetadata Metadata
        {
            get { return this.context.Executor.CurrentAgent.Metadata; }
        }

        public ScriptingAgent(CommandContext<T> ctxt, List<AgentTask> taskList)
        {
            context = ctxt;
            tasks = taskList;
        }

        private AgentTask RegisterTask(CommandId command)
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

        
    }
}
