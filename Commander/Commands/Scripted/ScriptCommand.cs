using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using Commander.Commands.Agent;
using Commander.Commands.Scripted;
using Commander.Executor;
using Common;
using Newtonsoft.Json;
using Shared;

namespace Commander.Commands.Composite
{
    public abstract class ScriptCommand<T> : EndPointCommand<T>
    {

        public override CommandId CommandId => CommandId.Script;
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        protected List<AgentTask> Tasks { get; private set; } = new List<AgentTask>();

        protected abstract void Run(ScriptingAgent<T> agent, ScriptingCommander<T> commander, ScriptingTeamServer<T> teamServer, T options, CommanderConfig config);


        //protected void RegisterTask(string command, string arguments = null, string fileName = null, string fileId = null)
        //{
        //    this.Tasks.Add(new AgentTask()
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        Command = command,
        //        Arguments = arguments,
        //        FileName = fileName,
        //        FileId = fileId
        //    });
        //}

 
        //protected abstract Task<bool> CreateComposition(CommandContext<T> context);


        //protected void Delay(int seconds)
        //{
        //    this.RegisterTask(EndPointCommand.DELAY, seconds.ToString());
        //}

        //protected void Echo(string message)
        //{
        //    this.RegisterTask(EndPointCommand.ECHO, message);
        //}

        //protected void Step(string message)
        //{
        //    this.RegisterTask(EndPointCommand.STEP, message);
        //}

        //protected void Shell(string cmd)
        //{
        //    this.RegisterTask(EndPointCommand.SHELL, cmd);
        //}

        //protected void Powershell(string cmd)
        //{
        //    this.RegisterTask(EndPointCommand.POWERSHELL, cmd);
        //}

        //protected void StartPivot(ConnexionUrl endpoint)
        //{
        //    this.RegisterTask(EndPointCommand.PIVOT, "start " + endpoint.ToString());
        //}

        //protected void StartPivot(string endpoint)
        //{
        //    ConnexionUrl pt = ConnexionUrl.FromString(endpoint);
        //    this.StartPivot(pt);
        //}

        //protected void Dowload(string fileName, string fileId, string outFile = null)
        //{
        //    this.RegisterTask(EndPointCommand.DOWNLOAD, outFile, fileName, fileId);
        //}

        protected override void SpecifyParameters(CommandContext<T> context)
        {
            this.Tasks.Clear();

            var agent = new ScriptingAgent<T>(context, this.Tasks);
            var commander = new ScriptingCommander<T>(context);
            var teamServer = new ScriptingTeamServer<T>(context);

            this.Run(agent, commander, teamServer, context.Options , context.Config);

            context.AddParameter(ParameterId.Parameters, this.Tasks);
            base.SpecifyParameters(context);
        }
    }

   
}
