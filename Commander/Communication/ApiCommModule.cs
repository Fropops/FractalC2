using System;
using Shared;
using CommandId = Shared.CommandId;
using ParameterDictionary = Shared.ParameterDictionary;
using AgentTaskResult = Shared.AgentTaskResult;
using BinarySerializer;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Commander.Terminal;
using Common;
using Common.APIClient;
using Common.APIModels;
using Common.APIModels.WebHost;
using Common.Models;
using Common.Payload;
using Spectre.Console;

namespace Commander.Communication
{
    public class ApiCommModule : ICommModule
    {
        public event EventHandler<ConnectionStatus> ConnectionStatusChanged;

        public event EventHandler<List<TeamServerAgentTask>> RunningTaskChanged;
        public event EventHandler<Agent> AgentMetaDataUpdated;
        public event EventHandler<AgentTaskResult> TaskResultUpdated;
        public event EventHandler<Agent> AgentAdded;
        public event EventHandler<APIImplant> ImplantAdded;

        private FractalApiClient _apiClient;
        private FractalApiCache _apiCache;
        private StateSyncService _syncService;
        private HashSet<string> _knownAgents = new HashSet<string>();

        private ITerminal Terminal;

        public CommanderConfig Config { get; set; }

        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Unknown;

        public ApiCommModule(ITerminal terminal, CommanderConfig config)
        {
            this.Terminal = terminal;
            this.Config = config;
            this._apiCache = new FractalApiCache();
            
            // Wire up events
            this._apiCache.OnAgentUpdated += (agent) => 
            {
                bool isNew = _knownAgents.Add(agent.Id);
                
                if (isNew && !this._apiCache.IsInitialLoading)
                {
                    this.AgentAdded?.Invoke(this, agent);
                }
                else
                {
                    this.AgentMetaDataUpdated?.Invoke(this, agent);
                }
            };
            
            // We need to simulate AgentAdded event. 
            // FractalApiCache exposes OnAgentUpdated.
            // We can track locally if we've seen it? Or StateSyncService could handle.
            // For now let's just trigger updates.
            
            this._apiCache.OnTaskUpdated += (task) =>
            {
                 var running = _apiCache.Tasks.Values.Where(t => !_apiCache.Results.ContainsKey(t.Id) || (_apiCache.Results[t.Id].Status != AgentResultStatus.Completed && _apiCache.Results[t.Id].Status != AgentResultStatus.Error)).ToList();
                 this.RunningTaskChanged?.Invoke(this, running);
            };

            this._apiCache.OnResultUpdated += (result) =>
            {
                if (result.Status == AgentResultStatus.Completed || result.Status == AgentResultStatus.Error)
                {
                    this.TaskResultUpdated?.Invoke(this, result);
                }
                 var running = _apiCache.Tasks.Values.Where(t => !_apiCache.Results.ContainsKey(t.Id) || (_apiCache.Results[t.Id].Status != AgentResultStatus.Completed && _apiCache.Results[t.Id].Status != AgentResultStatus.Error)).ToList();
                 this.RunningTaskChanged?.Invoke(this, running);
            };

            this._apiCache.OnImplantUpdated += (implant) => this.ImplantAdded?.Invoke(this, implant);

            this.UpdateConfig();
        }

        public void UpdateConfig()
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 5);
            httpClient.BaseAddress = new Uri($"http://{this.Config.ApiConfig.EndPoint}");
            httpClient.DefaultRequestHeaders.Clear();
            // TODO: Token generation logic should be in BaseApiClient or handled here.
            // Current BaseApiClient takes HttpClient. So we configure HttpClient here.
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + GenerateToken());

            _apiClient = new FractalApiClient(httpClient);
            
            if (_syncService != null) _syncService.Dispose();
            _syncService = new StateSyncService(_apiClient, _apiCache);
            _syncService.OnConnectionStatusChanged += (isConnected) => 
            {
                 ConnectionStatus = isConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
                 ConnectionStatusChanged?.Invoke(this, ConnectionStatus);
            };

            this._apiCache.Clear();
            this._knownAgents.Clear();

            this.ConnectionStatus = ConnectionStatus.Unknown;
            this.ConnectionStatusChanged?.Invoke(this, this.ConnectionStatus);
        }

        private string GenerateToken()
        {
            // Reusing existing token generation logic
            // We need to reference System.IdentityModel.Tokens.Jwt and params
            // Assuming same dependencies as before
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(this.Config.ApiConfig.ApiKey);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("id", Config.ApiConfig.User), new System.Security.Claims.Claim("session", Config.Session) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task Start()
        {
            var tcs = new TaskCompletionSource();
            Action loadingHandler = null;

            // Define handler to detect when initial loading finishes
            loadingHandler = () =>
            {
                 if(!_apiCache.IsInitialLoading)
                 {
                     tcs.TrySetResult();
                 }
            };
            
            _apiCache.OnLoadingStateChanged += loadingHandler;
            
            // Check immediately in case it's already done (unlikely before Start)
            if(!_apiCache.IsInitialLoading) tcs.TrySetResult();

            try
            {
                // Initial sync with Spectre Console Progress
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
                        var task1 = ctx.AddTask($"[cyan]Syncing with TeamServer[/]"); 
                        task1.MaxValue = 100; 
                        task1.Value = 0;

                        Action<int, int> progressHandler = (current, total) => 
                        {
                            if(total > 0)
                            {
                                task1.Description = $"[cyan]Syncing with TeamServer ({total} items)[/]";
                                task1.MaxValue = total;
                                task1.Value = current;
                            }
                        };
                        
                        _syncService.OnInitialSyncProgress += progressHandler;
                        
                        // Start the service (begins polling)
                        _syncService.Start();
                        
                        // Wait for completion signal from Cache
                        await tcs.Task;
                        
                        // Finish up UI
                        task1.Value = task1.MaxValue; 
                        task1.StopTask();
                        
                        _syncService.OnInitialSyncProgress -= progressHandler;
                    });
            }
            catch (Exception ex)
            {
                this.Terminal.WriteError($"Initial sync failed: {ex.Message}");
                // Ensure service is started even if UI fails
                _syncService.Start(); 
            }
            finally
            {
                _apiCache.OnLoadingStateChanged -= loadingHandler;
            }
            
            // Force prompt refresh after AnsiConsole takes over
            this.Terminal.NewLine(false);
        }

        public void Stop()
        {
            _syncService.Stop();
        }

        public List<Agent> GetAgents()
        {
            return _apiCache.Agents.Values.OrderBy(a => a.FirstSeen).ToList();
        }

        public Agent GetAgent(int index)
        {
            var agents = GetAgents();
            if (index < 0 || index >= agents.Count) return null;
            return agents[index];
        }

        public Agent GetAgent(string id)
        {
             _apiCache.Agents.TryGetValue(id, out var agent);
             return agent;
        }

        public async Task<HttpResponseMessage> StopAgent(string id)
        {
            await _apiClient.Agents.DeleteAsync(id);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        public IEnumerable<TeamServerAgentTask> GetTasks(string id)
        {
            return _apiCache.Tasks.Values.Where(t => t.AgentId == id).OrderByDescending(t => t.RequestDate);
        }

        public TeamServerAgentTask GetTask(string taskId)
        {
            _apiCache.Tasks.TryGetValue(taskId, out var task);
            return task;
        }

        public AgentTaskResult GetTaskResult(string taskId)
        {
            _apiCache.Results.TryGetValue(taskId, out var result);
            return result;
        }

        public async Task<HttpResponseMessage> CreateListener(string name, int port, string address, bool secured)
        {
            await _apiClient.Listeners.CreateAsync(new StartHttpListenerRequest 
            {
                Name = name,
                BindPort = port,
                Ip = address,
                Secured = secured
            });
             return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        public async Task<HttpResponseMessage> StopListener(string id)
        {
            await _apiClient.Listeners.DeleteAsync(id);
             return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        public IEnumerable<TeamServerListener> GetListeners()
        {
            return _apiCache.Listeners.Values.ToList();
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
            
            await _apiClient.Tasks.CreateAsync(agentId, taskrequest);
        }

        public async Task<bool> StartProxy(string agentId, int port)
        {
            await _apiClient.Proxy.StartAsync(agentId, port);
            return true;
        }

        public async Task<bool> StopProxy(int port)
        {
            await _apiClient.Proxy.StopAsync(port);
            return true;
        }

        public async Task<List<ProxyInfo>> ShowProxy()
        {
            var result = await _apiClient.Proxy.GetAllAsync();
            return result ?? new List<ProxyInfo>();
        }

        public async Task WebHost(string path, byte[] fileContent, bool isPowerShell, string description)
        {
            await _apiClient.WebHost.AddAsync(new FileWebHost 
            {
                Path = path,
                Data = fileContent,
                IsPowershell = isPowerShell,
                Description = description
            });
        }

        public async Task<List<FileWebHost>> GetWebHosts()
        {
            var result = await _apiClient.WebHost.GetAllAsync();
            return result ?? new List<FileWebHost>();
        }

        public async Task<List<WebHostLog>> GetWebHostLogs()
        {
             var result = await _apiClient.WebHost.GetLogsAsync();
            return result ?? new List<WebHostLog>();
        }

        public async Task RemoveWebHost(string path)
        {
            await _apiClient.WebHost.DeleteAsync(path);
        }

        public async Task ClearWebHosts()
        {
            await _apiClient.WebHost.ClearAsync();
        }

        public List<APIImplant> GetImplants()
        {
            return _apiCache.Implants.Values.ToList();
        }

        public APIImplant GetImplant(string id)
        {
            _apiCache.Implants.TryGetValue(id, out var implant);
            return implant;
        }

        public async Task<APIImplantCreationResult> GenerateImplant(ImplantConfig config)
        {
            return await _apiClient.Implants.GenerateAsync(config);
        }

        public async Task<APIImplant> GetImplantBinary(string id)
        {
            return await _apiClient.Implants.GetWithDataAsync(id);
        }

        public async Task DeleteImplant(string id)
        {
            await _apiClient.Implants.DeleteAsync(id);
        }

        public async Task<List<Loot>> GetLoot(string agentId)
        {
            var result = await _apiClient.Loot.GetAllAsync(agentId);
            return result ?? new List<Loot>();
        }

        public async Task<Loot> GetLootFile(string agentId, string fileName)
        {
            return await _apiClient.Loot.GetFileAsync(agentId, fileName);
        }

        public async Task<bool> CreateLootAsync(string agentId, Loot loot)
        {
            await _apiClient.Loot.CreateAsync(agentId, loot);
            return true;
        }

        public async Task DeleteLoot(string agentId, string fileName)
        {
             await _apiClient.Loot.DeleteAsync(agentId, fileName);
        }

        public async Task<List<Tool>> GetTools(ToolType? type = null, string name = null)
        {
            var result = await _apiClient.Tools.GetAllAsync(type, name);
            return result ?? new List<Tool>();
        }

        public async Task AddTool(string path)
        {
            if(!System.IO.File.Exists(path))
                  throw new System.IO.FileNotFoundException("File not found", path);
 
             var data = await System.IO.File.ReadAllBytesAsync(path);
             var tool = new Tool()
             {
                 Name = System.IO.Path.GetFileName(path),
                 Data = Convert.ToBase64String(data),
             };
             await _apiClient.Tools.AddAsync(tool);
        }

        public async Task CloseSession()
        {
            await _apiClient.CloseSessionAsync();
        }

    }
}
