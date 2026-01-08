using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinarySerializer;
using Commander.CommanderCommand;
using Commander.Commands;
using Commander.Commands.Agent;
using Commander.Commands.Agent.EndPoint;
using Commander.Commands.Custom;
using Commander.Communication;
using Commander.Helper;
using Commander.Models;
using Commander.Terminal;
using Common.AgentCommands;
using Common.CommandLine.Execution;
using Common.Models;
using Shared;

namespace Commander.Executor
{

    public class Executor : IExecutor
    {


        public ExecutorMode Mode { get; set; } = ExecutorMode.None;

        public bool IsRunning
        {
            get
            {
                return !this._tokenSource.IsCancellationRequested;
            }
        }


        private Agent _currentAgent = null;
        public Agent CurrentAgent
        {
            get => _currentAgent; set
            {
                _currentAgent = value;
                if (this._currentAgent != null)
                    this.UpdateAgentPrompt();
            }
        }

        private void UpdateAgentPrompt()
        {
            if (this._currentAgent.Metadata == null)
            {
                this.Terminal.Prompt = $"$({_currentAgent.Id})> ";
            }
            else
            {
                var star = _currentAgent.Metadata?.HasElevatePrivilege() == true ? "*" : string.Empty;
                this.Terminal.Prompt = $"$({_currentAgent.Metadata.Name}) {_currentAgent.Metadata.UserName}{star}@{_currentAgent.Metadata.Hostname}> ";
            }
        }
        private ICommModule CommModule { get; set; }
        public ITerminal Terminal { get; set; }

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public bool IsBusy { get; private set; }

        private CommandExecutor CommandExecutor = new CommandExecutor();

        public Executor(ITerminal terminal, ICommModule commModule)
        {
            this.CommModule = commModule;
            this.Terminal = terminal;

            // Register Context(s)
            var context = new CommanderCommandContext(this.CommModule, this.Terminal, this);
            this.CommandExecutor.RegisterContextFactory(() => new CommanderCommandContext(this.CommModule, this.Terminal, this));
            this.CommandExecutor.RegisterContextFactory(() => new AgentCommandContext(new AgentCommandAdapter(this, this.Terminal, this.CommModule)));

            //Force the assembly to be loaded.
            var whoami = new Common.AgentCommands.WhoamiCommand();
            // Load Commands
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                this.CommandExecutor.LoadCommands(assembly);
            }

            //suscribe to events
            this.Terminal.InputValidated += Instance_InputValidated;

            this.CommModule.ConnectionStatusChanged +=CommModule_ConnectionStatusChanged;
            this.CommModule.TaskResultUpdated += CommModule_TaskResultUpdated;
            this.CommModule.AgentMetaDataUpdated += CommModule_AgentMetadataUpdated;
            this.CommModule.AgentAdded +=CommModule_AgentAdded;
            //end events
        }

        public List<CommandDefinition> GetAllCommands()
        {
            return this.CommandExecutor.RegisteredCommands;
        }

        private void CommModule_AgentAdded(object sender, Agent e)
        {
            Terminal.Interrupt();

            var index = this.CommModule.GetAgents().OrderBy(a => a.FirstSeen).ToList().IndexOf(e);
            Terminal.WriteInfo($"New Agent Checking in : {e.Id} ({index})");
            Terminal.Restore();
        }

        private void CommModule_AgentMetadataUpdated(object sender, Agent e)
        {
            if (this.CurrentAgent != null && e.Id == this.CurrentAgent.Id)
            {
                this.UpdateAgentPrompt();
            }
        }

        private void CommModule_TaskResultUpdated(object sender, AgentTaskResult res)
        {
            var task = this.CommModule.GetTask(res.Id);
            if (task == null)
            {
                return;
                /*task =  new AgentTask()
                {
                    Id = res.Id,
                    AgentId = this.CurrentAgent.Metadata.Id,
                    Label = "unknown task",
                    Command = "unknown",
                };
                this.CommModule.AddTask(task);*/
            }
            if (this.CurrentAgent == null || task.AgentId != this.CurrentAgent.Id)
                return;

            this.Terminal.Interrupt();
            TaskPrinter.Print(task, res, this.Terminal);

            if(task.CommandId == CommandId.Capture)
            {
                if (res.Objects == null || res.Objects.Length == 0)
                    return;
                var list = res.Objects.BinaryDeserializeAsync<List<DownloadFile>>().Result;

                if (!Directory.Exists("media"))
                    Directory.CreateDirectory("media");

                var path = Path.Combine("media", task.AgentId);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                foreach (var file in list)
                {
                    File.WriteAllBytes(Path.Combine(path,file.FileName), file.Data);
                    this.Terminal.WriteInfo($"Screenshot saved : {file.FileName}.");
                }
            }
            /*foreach (var file in res.Files.Where(f => !f.IsDownloaded))
            {
                bool first = true;
                var bytes = this.CommModule.Download(file.FileId, a =>
                {
                    this.Terminal.ShowProgress("dowloading", a, first);
                    first = false;
                }).Result;

                using (FileStream fs = new FileStream(file.FileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                this.Terminal.WriteSuccess($"File {file.FileName} successfully downloaded");
            }*/
            this.Terminal.Restore();
        }


        private void CommModule_ConnectionStatusChanged(object sender, ConnectionStatus e)
        {
            string status = string.Empty;
            this.Terminal.Interrupt();
            switch (e)
            {
                case ConnectionStatus.Connected:
                    {
                        status = $"Connected to  TeamServer ({this.CommModule.Config.ApiConfig.EndPoint}).";
                        this.Terminal.WriteSuccess(status);
                    }
                    break;
                case ConnectionStatus.Unauthorized:
                    {
                        status = $"Not Authorized to connect to TeamServer ({this.CommModule.Config.ApiConfig.EndPoint}).";
                        this.Terminal.WriteError(status);
                    }
                    break;
                default:
                    {
                        status = $"Cannot connect to TeamServer ({this.CommModule.Config.ApiConfig.EndPoint}).";
                        this.Terminal.WriteError(status);
                    }
                    break;

            }

            this.Terminal.Restore();
        }

        private void Instance_InputValidated(object sender, string e)
        {
            this.HandleInput(e);
        }

        public void HandleInput(string input)
        {
            this.Terminal.CanHandleInput = false;
            string error = $"Command {input} is unknow.";

            try
            {
                var commandDef = this.CommandExecutor.GetCommand(input).Result;
                if(commandDef == null)
                {
                    this.Terminal.WriteLine(error);
                    return;
                }

                if(typeof(Common.AgentCommands.AgentCommandBase).IsAssignableFrom(commandDef.CommandType) && this.CurrentAgent == null)
                {
                    this.Terminal.WriteLine("No agent selected. Use 'interact' command to select an agent.");
                    return;
                }

                this.CommandExecutor.Execute(input).Wait();
            }
            catch (Exception ex)
            {
                this.Terminal.WriteError($"An Error occurred : {ex}");
            }
            finally
            {
                this.InputHandled(null, false);
            }
        }

        public void InputHandled(ExecutorCommand cmd, bool cmdResult)
        {
            this.Terminal.CanHandleInput = true;
            this.Terminal.NewLine();
        }

        public void Start()
        {
            this.Terminal.Start();
            this.CommModule.Start();
        }

        public void Stop()
        {
            this._tokenSource.Cancel();
            this.CommModule.Stop();
            this.Terminal.stop();
        }

    }
}
