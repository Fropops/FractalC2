using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commander.Models;
using Commander.Terminal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Spectre.Console;
using Shared;
using Common.Models;
using Common.APIModels;
using BinarySerializer;
using Common;
using Common.APIModels.WebHost;

namespace Commander.Communication
{
    public class ApiCommModule : ICommModule
    {
        public event EventHandler<ConnectionStatus> ConnectionStatusChanged;

        public event EventHandler<List<TeamServerAgentTask>> RunningTaskChanged;
        public event EventHandler<Agent> AgentMetaDataUpdated;
        public event EventHandler<AgentTaskResult> TaskResultUpdated;
        public event EventHandler<Agent> AgentAdded;

        private CancellationTokenSource _tokenSource;

        private HttpClient _client;

        protected ConcurrentDictionary<string, TeamServerListener> _listeners = new ConcurrentDictionary<string, TeamServerListener>();
        protected ConcurrentDictionary<string, Agent> _agents = new ConcurrentDictionary<string, Agent>();
        protected ConcurrentDictionary<string, TeamServerAgentTask> _tasks = new ConcurrentDictionary<string, TeamServerAgentTask>();
        protected ConcurrentDictionary<string, AgentTaskResult> _results = new ConcurrentDictionary<string, AgentTaskResult>();


        private ITerminal Terminal;

        public CommanderConfig Config { get; set; }
        public ApiCommModule(ITerminal terminal, CommanderConfig config)
        {
            this.Terminal = terminal;
            this.Config = config;

            this.UpdateConfig();
        }

        public void UpdateConfig()
        {
            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 5);
            _client.BaseAddress = new Uri($"http://{this.Config.ApiConfig.EndPoint}");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ GenerateToken());

            this._agents.Clear();
            this._tasks.Clear();
            this._results.Clear();
            this._listeners.Clear();

            this.ConnectionStatus = ConnectionStatus.Unknown;
            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
        }

        private string GenerateToken()
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.Config.ApiConfig.ApiKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", Config.ApiConfig.User), new Claim("session", Config.Session) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Unknown;

        bool isSyncing = true;
        public async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    var changes = await this.GetChanges(this.isSyncing);

                    if (this.ConnectionStatus != ConnectionStatus.Connected)
                    {
                        this.ConnectionStatus = ConnectionStatus.Connected;
                        this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
                    }

                    if (isSyncing)
                    {
                        var serverConfig = await this.ServerConfig();
                        this.Config.ServerConfig = serverConfig;
                        

                        this._listeners.Clear();
                        this._agents.Clear();
                        this._tasks.Clear();
                        this._results.Clear();

                        Terminal.Interrupt();
                        Terminal.CanHandleInput = false;

                        await AnsiConsole.Progress()
                            .Columns(new ProgressColumn[]
                            {
                                new TaskDescriptionColumn(),    // Task description
                                new ProgressBarColumn(),        // Progress bar
                                new PercentageColumn(),         // Percentage
                                new SpinnerColumn(Spinner.Known.Default).Style(Style.Parse("cyan")),            // Spinner
                            })
                        .StartAsync(async ctx =>
                        {
                            var task1 = ctx.AddTask($"[cyan]Syncing whith TeamServer ({changes.Count} items)[/]");

                            task1.MaxValue = changes.Count + 1;
                            task1.Value = 0;
                            task1.Increment(1);
                            //// Simulate some work
                            //AnsiConsole.MarkupLine("Doing some more work...");
                            //Thread.Sleep(2000);
                            foreach (var change in changes)
                            {
                                //await Task.Delay(10000);
                                await this.HandleChange(change);
                                task1.Increment(1);
                            }
                        });


                        //Terminal.WriteInfo($"Syncing whith TeamServer ({changes.Count}) :");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Listener)} Listeners to load.");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Agent)} Agents to load.");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Task)} Tasks to load.");
                        //Terminal.WriteInfo($"{changes.Count(c => c.Element == ChangingElement.Result)} Results to load.");

                        Terminal.WriteSuccess($"Syncing done.");
                        //Terminal.WriteInfo($"ServerKey is {this.Config.ServerConfig.Key}");
                        Terminal.Restore();
                        Terminal.CanHandleInput = true;
                    }
                    else
                    {
                        foreach (var change in changes)
                            await this.HandleChange(change);
                    }

                    isSyncing = false;
                }
                catch (Exception e)
                {
                    if ((e.InnerException != null && e.InnerException is TimeoutException) || e is HttpRequestException)
                    {
                        var newStatus = ConnectionStatus.Disconnected;
                        if (e is HttpRequestException)
                        {
                            var htEx = e as HttpRequestException;
                            if (htEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                newStatus = ConnectionStatus.Unauthorized;
                        }


                        if (this.ConnectionStatus != newStatus)
                        {
                            this.ConnectionStatus = newStatus;
                            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
                        }
                    }
                    else
                        this.Terminal.WriteError(e.ToString());
                }

                await Task.Delay(this.Config.ApiConfig.Delay);
            }
        }

        private async Task HandleChange(Change change)
        {
            switch (change.Element)
            {
                case ChangingElement.Listener:
                    await this.UpdateListener(change.Id);
                    break;
                case ChangingElement.Agent:
                    await this.UpdateAgent(change.Id);
                    break;
                case ChangingElement.Task:
                    await this.UpdateTask(change.Id);
                    break;
                case ChangingElement.Result:
                    await this.UpdateResult(change.Id);
                    break;
                case ChangingElement.Metadata:
                    await this.UpdateMetadata(change.Id);
                    break;
            }
        }

        private async Task<List<Change>> GetChanges(bool first)
        {
            var response = await _client.GetStringAsync($"/session/Changes?history={first}");
            var tasksResponse = JsonConvert.DeserializeObject<List<Change>>(response);
            return tasksResponse;
        }

        public async Task CloseSession()
        {
            var response = await _client.GetAsync($"/session/exit");
        }

        private async Task UpdateListener(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"/Listeners/{id}");
                var listener = JsonConvert.DeserializeObject<TeamServerListener>(response);

                this._listeners.AddOrUpdate(listener.Id, listener, (key, current) =>
                {
                    current.Name = listener.Name;
                    current.BindPort = listener.BindPort;
                    current.Secured = listener.Secured;
                    current.Ip = listener.Ip;
                    return current;
                });
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._listeners.ContainsKey(id))
                        this._listeners.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateAgent(string id)
        {
            try
            {
                //Terminal.WriteInfo(_client.BaseAddress.ToString());
                var response = await _client.GetStringAsync($"Agents/{id}");
                var ar = JsonConvert.DeserializeObject<TeamServerAgent>(response);

                var agent = new Agent()
                {
                    Id = ar.Id,
                    FirstSeen = ar.FirstSeen,
                    LastSeen = ar.LastSeen,
                    RelayId = ar.RelayId,
                    Links = ar.Links,
                };

                bool isNew = !this._agents.ContainsKey(agent.Id);
                this._agents.AddOrUpdate(ar.Id, agent, (key, current) =>
                {
                    current.LastSeen = agent.LastSeen;
                    current.RelayId = agent.RelayId;
                    current.Links = agent.Links;
                    return current;
                });

                if (isNew && !this.isSyncing)
                    this.AgentAdded?.Invoke(this, agent);

            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._agents.ContainsKey(id))
                        this._agents.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateTask(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"Tasks/{id}");

                var task = JsonConvert.DeserializeObject<TeamServerAgentTask>(response);

                this._tasks.AddOrUpdate(task.Id, task, (key, current) =>
                {
                    current.RequestDate = task.RequestDate;
                    current.AgentId = task.AgentId;
                    current.Id = task.Id;
                    current.Command = task.Command;
                    return current;
                });
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._tasks.ContainsKey(id))
                        this._tasks.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateResult(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"Results/{id}");
                var result = JsonConvert.DeserializeObject<AgentTaskResult>(response);

                //foreach (var file in tr.Files)
                //    res.Files.Add(new Models.TaskFileResult() { FileId = file.FileId, FileName = file.FileName, IsDownloaded = file.IsDownloaded });

                //new respone or response change detected

                if (!_results.ContainsKey(result.Id)) // new response
                {
                    if ((result.Status == AgentResultStatus.Completed || result.Status == AgentResultStatus.Error) && !this.isSyncing)
                        this.TaskResultUpdated?.Invoke(this, result);
                }
                else
                {
                    //Change detected :
                    var existing = this._results[result.Id];
                    if (result.Output != existing.Output
                        || result.Error != existing.Error
                        || result.Objects != existing.Objects
                        || result.Status  != existing.Status
                        || result.Info != existing.Info
                        )
                    {
                        if ((result.Status == AgentResultStatus.Completed || result.Status == AgentResultStatus.Error) && !this.isSyncing)
                            this.TaskResultUpdated?.Invoke(this, result);
                    }
                }

                this._results.AddOrUpdate(result.Id, result, (key, current) =>
                {
                    current.Output = result.Output;
                    current.Error = result.Error;
                    current.Objects = result.Objects;
                    current.Info = result.Info;
                    current.Status = result.Status;
                    //current.Files.Clear();
                    //foreach (var file in res.Files)
                    //{
                    //    current.Files.Add(file);
                    //}
                    return current;
                });

                var running = this._tasks.Values.Where(t => !this._results.ContainsKey(t.Id) || (this._results[t.Id].Status != AgentResultStatus.Completed && this._results[t.Id].Status != AgentResultStatus.Error)).ToList();
                this.RunningTaskChanged?.Invoke(this, running);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._results.ContainsKey(id))
                        this._results.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        private async Task UpdateMetadata(string id)
        {
            try
            {
                var response = await _client.GetStringAsync($"agents/{id}/metadata");

                var metadata = JsonConvert.DeserializeObject<AgentMetadata>(response);

                if (metadata == null)
                    return;

                var agent = new Agent()
                {
                    Id = metadata.Id,
                    Metadata = metadata,
                };

                bool isNew = !this._agents.ContainsKey(agent.Id);
                this._agents.AddOrUpdate(agent.Id, agent, (key, current) =>
                {
                    current.Metadata = metadata;
                    return current;
                });

                if(!isSyncing)
                    this.AgentMetaDataUpdated?.Invoke(this, agent);
                if (isNew && !this.isSyncing)
                    this.AgentAdded?.Invoke(this, agent);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (this._tasks.ContainsKey(id))
                        this._tasks.Remove(id, out _);
                }
                else
                    throw e;
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        public List<Agent> GetAgents()
        {
            return this._agents.Values.ToList();
        }

        public Agent GetAgent(int index)
        {
            var agents = GetAgents().OrderBy(a => a.FirstSeen).ToList();
            if (index < 0)
                return null;
            if (index > agents.Count() -1)
                return null;

            return agents[index];
        }

        public Agent GetAgent(string id)
        {
            if (!this._agents.ContainsKey(id))
                return null;
            return this._agents[id];
        }

        public void DeleteAgent(string id)
        {
            if (GetAgent(id) != null)
                this._agents.Remove(id, out var ret);
        }

        public IEnumerable<TeamServerAgentTask> GetTasks(string id)
        {
            return this._tasks.Values.Where(t => t.AgentId == id).OrderByDescending(t => t.RequestDate);
        }

        public async Task<HttpResponseMessage> StopAgent(string id)
        {
            this.DeleteAgent(id);
            return await _client.GetAsync($"/Agents/{id}/stop");
        }

        public TeamServerAgentTask GetTask(string taskId)
        {
            if (!this._tasks.ContainsKey(taskId))
                return null;
            return this._tasks[taskId];
        }

        public AgentTaskResult GetTaskResult(string taskId)
        {
            if (!this._results.ContainsKey(taskId))
                return null;
            return this._results[taskId];
        }



        public async Task<HttpResponseMessage> CreateListener(string name, int port, string address, bool secured)
        {
            var requestObj = new StartHttpListenerRequest();
            requestObj.Name = name;
            requestObj.BindPort  = port;
            requestObj.Ip = address;
            requestObj.Secured = secured;
            var requestContent = JsonConvert.SerializeObject(requestObj);

            return await _client.PostAsync("/Listeners/", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> StopListener(string id, bool clean)
        {
            return await _client.DeleteAsync($"/Listeners/?id={id}&clean={clean}");
        }

        public IEnumerable<TeamServerListener> GetListeners()
        {
            return this._listeners.Values.ToList();
        }

        
        public async Task TaskAgent(string label, string agentId, CommandId commandId, ParameterDictionary parms)
        {
            var agentTask = new AgentTask()
            {
                Id = ShortGuid.NewGuid(),
                CommandId = commandId,
                Parameters = parms,
            };
            var ser = await agentTask.BinarySerializeAsync();

            var taskrequest = new CreateTaskRequest()
            {
                Command = label,
                Id = agentTask.Id,
                TaskBin = Convert.ToBase64String(ser),
            };

            var requestContent = JsonConvert.SerializeObject(taskrequest);

            var response = await _client.PostAsync($"/Agents/{agentId}", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task<ServerConfig> ServerConfig()
        {
            var response = await _client.GetAsync($"/Config");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var conf = JsonConvert.DeserializeObject<ServerConfig>(json);

            return conf;
        }

        #region Proxy
        public async Task<bool> StartProxy(string agentId, int port)
        {
            var resp = await _client.GetAsync($"/Agents/{agentId}/startproxy?port={port}");
            if (resp.IsSuccessStatusCode)
                return true;
            else
            {
                Terminal.WriteError(resp.StatusCode + " " + resp.Content.ReadAsStringAsync().Result);
                return false;
            }
        }
        public async Task<bool> StopProxy(string agentId)
        {
            var resp = await _client.GetAsync($"/Agents/{agentId}/stopproxy");
            if (resp.IsSuccessStatusCode)
                return true;
            else
            {
                Terminal.WriteError(resp.StatusCode + " " + resp.Content.ReadAsStringAsync().Result);
                return false;
            }
        }

        public async Task<List<ProxyInfo>> ShowProxy()
        {
            var response = await _client.GetAsync($"/Agents/proxy");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var proxies = JsonConvert.DeserializeObject<List<ProxyInfo>>(json);
            return proxies;
        }
        #endregion

        #region WebHost
        public async Task WebHost(string path, byte[] fileContent, bool isPowerShell, string description)
        {
            var wh = new FileWebHost()
            {
                Path = path,
                Data = fileContent,
                IsPowershell = isPowerShell,
                Description = description
            };
            var requestContent = JsonConvert.SerializeObject(wh);

            var response = await _client.PostAsync($"/WebHost", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task<List<FileWebHost>> GetWebHosts()
        {
            var response = await _client.GetAsync($"/WebHost");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<FileWebHost>>(json);
            return list;

        }

        public async Task<List<WebHostLog>> GetWebHostLogs()
        {
            var response = await _client.GetAsync($"/WebHost/Logs");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<WebHostLog>>(json);
            return list;

        }

        public async Task RemoveWebHost(string path)
        {
            var wh = new FileWebHost()
            {
                Path = path,
            };
            var requestContent = JsonConvert.SerializeObject(wh);
            var response = await _client.PostAsync($"/WebHost/Remove", new StringContent(requestContent, UnicodeEncoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }

        public async Task ClearWebHosts()
        {
            var response = await _client.GetAsync($"/WebHost/Clear");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
        }


        #endregion

        #region files
        public async Task<List<TeamServerDownloadFile>> GetFiles()
        {
            var response = await _client.GetAsync($"/DownloadFile/");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<TeamServerDownloadFile>>(json);
            return list;
        }

        public async Task<TeamServerDownloadFile> GetFile(string id)
        {
            var response = await _client.GetAsync($"/DownloadFile/{id}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");

            var json = await response.Content.ReadAsStringAsync();
            var file = JsonConvert.DeserializeObject<TeamServerDownloadFile>(json);
            return file;
        }
        #endregion
    }
}
