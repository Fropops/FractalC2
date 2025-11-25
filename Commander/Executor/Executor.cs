using BinarySerializer;
using Commander.Commands;
using Commander.Communication;
using Commander.Helper;
using Commander.Models;
using Commander.Terminal;
using Common.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                this.Terminal.Prompt = $"${ExecutorMode.Agent}({_currentAgent.Id})> ";
            }
            else
            {
                var star = _currentAgent.Metadata?.HasElevatePrivilege() == true ? "*" : string.Empty;
                this.Terminal.Prompt = $"${ExecutorMode.Agent}({_currentAgent.Id}) {_currentAgent.Metadata.UserName}{star}@{_currentAgent.Metadata.Hostname}> ";
            }
        }
        private ICommModule CommModule { get; set; }
        public ITerminal Terminal { get; set; }

        private Dictionary<ExecutorMode, List<ExecutorCommand>> _commands = new Dictionary<ExecutorMode, List<ExecutorCommand>>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public bool IsBusy { get; private set; }

        public Executor(ITerminal terminal, ICommModule commModule)
        {
            this.CommModule = commModule;
            this.Terminal = terminal;

            this.LoadCommands();

            //suscribe to events
            this.Terminal.InputValidated += Instance_InputValidated;

            this.CommModule.ConnectionStatusChanged +=CommModule_ConnectionStatusChanged;
            this.CommModule.RunningTaskChanged += CommModule_RunningTaskChanged;
            this.CommModule.TaskResultUpdated += CommModule_TaskResultUpdated;
            this.CommModule.AgentMetaDataUpdated += CommModule_AgentMetadataUpdated;
            this.CommModule.AgentAdded +=CommModule_AgentAdded;
            //end events
        }

        private void CommModule_AgentAdded(object sender, Agent e)
        {
            Terminal.Interrupt();
            //string userName = e.Metadata.UserName;
            //if (e.Metadata.Integrity == "High")
            //    userName += "*";

            var index = this.CommModule.GetAgents().OrderBy(a => a.FirstSeen).ToList().IndexOf(e);
            Terminal.WriteInfo($"New Agent Checking in : {e.Id} ({index})");
            Terminal.Restore();
        }

        private void CommModule_AgentMetadataUpdated(object sender, Agent e)
        {
            if (this.CurrentAgent != null && e.Id == this.CurrentAgent.Id)
            {
                //this.CurrentAgent = this.CommModule.GetAgent(this.CurrentAgent.Id);
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

        private void CommModule_RunningTaskChanged(object sender, List<TeamServerAgentTask> tasks)
        {
            //if(this.CurrentAgent == null)
            //{
            //    if(lastRunningCount != 0)
            //    {
            //        Terminal.CanHandleInput = false;
            //        Terminal.SaveCursorPosition();
            //        Terminal.SetCursorPosition(0, 0);
            //        Terminal.DrawBackGround(TerminalConstants.DefaultBackGroundColor, lastRunningCount + 1);
            //        Terminal.ResetCursorPosition();
            //        Terminal.CanHandleInput = true;
            //    }
            //    return;
            //}
            //tasks = tasks.Where(t => t.AgentId == this.CurrentAgent.Metadata.Id).ToList();
            //if (tasks.Count == 0 && lastRunningCount == 0)
            //    return;

            //Terminal.CanHandleInput = false;

            //Terminal.SaveCursorPosition();
            //Terminal.SetCursorPosition(0, 0);
            //Terminal.DrawBackGround(TerminalConstants.DefaultBackGroundColor, lastRunningCount + 1);

            //lastRunningCount = tasks.Count;
            //if (tasks.Any())
            //{
            //    Terminal.SetCursorPosition(0, 0);
            //    Terminal.DrawBackGround(ConsoleColor.Cyan, tasks.Count + 1);

            //    Terminal.SetBackGroundColor(ConsoleColor.Cyan);
            //    Terminal.SetForeGroundColor(ConsoleColor.Black);

            //    Terminal.SetCursorPosition(0, 0);
            //    Terminal.WriteLine("Running Commands :");
            //    int index = 0;
            //    foreach (var task in tasks.OrderBy(t => t.RequestDate))
            //    {
            //        index++;
            //        int completion = 0;
            //        var res = this.CommModule.GetTaskResult(task.Id);
            //        if (res != null)
            //            completion = res.Completion;

            //        Terminal.Write($" #{index} {task.FullCommand} - {completion}%");
            //        Terminal.WriteLine();
            //    }
            //}

            //Terminal.SetForeGroundColor(TerminalConstants.DefaultForeGroundColor);
            //Terminal.SetBackGroundColor(TerminalConstants.DefaultBackGroundColor);
            //Terminal.ResetCursorPosition();

            //Terminal.CanHandleInput = true;
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
            string command = string.Empty;
            string parms = string.Empty;

            int limitIndex = e.IndexOf(' ');
            if (limitIndex == -1)
                command = e;
            else
            {
                command = e.Substring(0, limitIndex);
                parms = e.Substring(limitIndex+1, e.Length - limitIndex - 1);
            }

            this.HandleInput(command, parms);
        }

        public void LoadCommands()
        {
            var self = Assembly.GetExecutingAssembly();
            foreach (var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(ExecutorCommand)) && !type.IsAbstract)
                {
                    var instance = Activator.CreateInstance(type) as ExecutorCommand;

                    if (!this._commands.ContainsKey(instance.AvaliableIn))
                    {
                        var list = new List<ExecutorCommand>() { instance };
                        this._commands.Add(instance.AvaliableIn, list);
                    }
                    else
                    {
                        this._commands[instance.AvaliableIn].Add(instance);
                    }
                }
            }
        }

        public IEnumerable<ExecutorCommand> GetCommandsInMode(ExecutorMode mode)
        {
            if (!this._commands.ContainsKey(mode))
                return new List<ExecutorCommand>();

            return this._commands[mode];
        }

        public ExecutorCommand GetCommandInMode(ExecutorMode mode, string commandName)
        {
            if (!this._commands.ContainsKey(mode))
            {
                return null;
            }

            var list = this._commands[mode];
            if (list == null || list.Count == 0)
            {
                return null;
            }

            var command = list.FirstOrDefault(c => c.Name == commandName || (c.Alternate != null && c.Alternate.Contains(commandName)));

            if (command is null)
            {
                return null;
            }

            return command;
        }

        public void HandleInput(string input, string parms)
        {
            this.Terminal.CanHandleInput = false;
            string error = $"Command {input} is unknow.";

            var cmd = this.GetCommandInMode(this.Mode, input);
            if (cmd == null)
                cmd = this.GetCommandInMode(ExecutorMode.All, input);

            if (cmd is null)
            {
                this.Terminal.WriteError(error);
                this.InputHandled(null, false);
                return;
            }

            try
            {
                cmd.Execute(parms);
            }
            catch(Exception ex)
            {
                this.Terminal.WriteError($"An Error occurred : {ex}");
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
