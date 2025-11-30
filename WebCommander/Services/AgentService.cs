using WebCommander.Models;

namespace WebCommander.Services
{
    public class AgentService : IDisposable
    {
        private readonly TeamServerClient _client;
        private readonly Dictionary<string, Agent> _agents = new();
        private readonly Dictionary<string, Listener> _listeners = new();
        private readonly Dictionary<string, Implant> _implants = new();
        private readonly Dictionary<string, AgentTaskResult> _taskResults = new();
        private readonly Dictionary<string, TeamServerAgentTask> _tasks = new();
        private System.Threading.Timer? _timer;
        private bool _firstCall = true;
        private bool _isInitialLoading = false;
        private int _totalChanges = 0;
        private int _processedChanges = 0;
        private bool _hasConnectionError = false;
        private bool _isPolling = false;

        public event Action? OnAgentsUpdated;
        public event Action? OnListenersUpdated;
        public event Action? OnImplantsUpdated;
        public event Action? OnLoadingStateChanged;
        public event Action<Agent>? OnNewAgent;
        public event Action<AgentTaskResult>? OnAgentResult;
        public event Action? OnTasksUpdated;
        public event Action? OnProgressUpdated;
        public event Action? OnConnectionStatusChanged;
        public event Action? OnAuthorizationErrorChanged;

        public bool IsInitialLoading => _isInitialLoading;
        public int LoadingProgress => _totalChanges > 0 ? (_processedChanges * 100) / _totalChanges : 0;
        public bool HasConnectionError => _hasConnectionError;
        public bool HasAuthorizationError { get; private set; } = false;

        public AgentService(TeamServerClient client)
        {
            _client = client;
        }

        public async Task InitializeDataAsync()
        {
            ClearCache();
            _isInitialLoading = true;
            OnLoadingStateChanged?.Invoke();

            try
            {
                // Perform initial fetch
                await PollForChanges();
            }
            finally
            {
                _isInitialLoading = false;
                OnLoadingStateChanged?.Invoke();
            }
        }

        public void StartPolling()
        {
            if (_isPolling) return;
            
            _isPolling = true;
            _timer = new System.Threading.Timer(async _ =>
            {
                if (_isPolling)
                {
                    await PollForChanges();
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        public void StopPolling()
        {
            _isPolling = false;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
        }

        private async Task PollForChanges()
        {
            try
            {
                var changes = await _client.GetChangesAsync(_firstCall);
                _firstCall = false;

                // Connection successful - clear error state if it was set
                if (_hasConnectionError)
                {
                    _hasConnectionError = false;
                    OnConnectionStatusChanged?.Invoke();
                }

                if (HasAuthorizationError)
                {
                    HasAuthorizationError = false;
                    OnAuthorizationErrorChanged?.Invoke();
                }

                // Track progress during initial loading
                if (_isInitialLoading)
                {
                    _totalChanges = changes.Count;
                    _processedChanges = 0;
                    OnProgressUpdated?.Invoke();
                }

                bool agentsUpdated = false;
                bool listenersUpdated = false;
                bool implantsUpdated = false;
                bool tasksUpdated = false;

                foreach (var change in changes)
                {
                    // Console.WriteLine($"Processing change: {change.Type} for ID {change.Id}");
                    
                    if (change.Type == ChangingElement.Agent)
                    {
                        try
                        {
                            var agent = await _client.GetAgentAsync(change.Id);
                            if (agent != null)
                            {
                                bool isNewAgent = !_agents.ContainsKey(agent.Id);
                                
                                // Fetch metadata if this is the first time we see this agent
                                if (isNewAgent)
                                {
                                    agent.Metadata = await _client.GetAgentMetadataAsync(agent.Id);
                                    
                                    // Notify about new agent only if not in initial loading
                                    if (!_isInitialLoading)
                                    {
                                        OnNewAgent?.Invoke(agent);
                                    }
                                }
                                else
                                {
                                    // Preserve existing metadata when updating
                                    agent.Metadata = _agents[agent.Id].Metadata;
                                }
                                
                                _agents[agent.Id] = agent;
                                agentsUpdated = true;
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            if (_agents.Remove(change.Id))
                            {
                                agentsUpdated = true;
                            }
                        }
                    }
                    else if (change.Type == ChangingElement.Listener)
                    {
                        try
                        {
                            var listener = await _client.GetListenerAsync(change.Id);
                            if (listener != null)
                            {
                                _listeners[listener.Id] = listener;
                                listenersUpdated = true;
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            if (_listeners.Remove(change.Id))
                            {
                                listenersUpdated = true;
                            }
                        }
                    }
                    else if (change.Type == ChangingElement.Result)
                    {
                        try
                        {
                            var result = await _client.GetTaskResultAsync(change.Id);
                            if (result != null)
                            {
                                _taskResults[result.Id] = result;
                                
                                if ((result.Status == AgentResultStatus.Completed || result.Status == AgentResultStatus.Error) && !_isInitialLoading)
                                {
                                    OnAgentResult?.Invoke(result);
                                }
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _taskResults.Remove(change.Id);
                        }
                    }
                    else if (change.Type == ChangingElement.Task)
                    {
                        try
                        {
                            var task = await _client.GetTaskAsync(change.Id);
                            if (task != null)
                            {
                                _tasks[task.Id] = task;
                                tasksUpdated = true;
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            if (_tasks.Remove(change.Id))
                            {
                                tasksUpdated = true;
                            }
                        }
                    }
                    else if (change.Type == ChangingElement.Implant)
                    {
                        try
                        {
                            var implant = await _client.GetImplantAsync(change.Id);
                            if (implant != null)
                            {
                                _implants[implant.Id] = implant;
                                implantsUpdated = true;
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            if (_implants.Remove(change.Id))
                            {
                                implantsUpdated = true;
                            }
                        }
                    }

                    // Update progress during initial loading
                    if (_isInitialLoading)
                    {
                        _processedChanges++;
                        OnProgressUpdated?.Invoke();
                    }
                }

                // Notify subscribers
                if (agentsUpdated) OnAgentsUpdated?.Invoke();
                if (listenersUpdated) OnListenersUpdated?.Invoke();
                if (implantsUpdated) OnImplantsUpdated?.Invoke();
                if (tasksUpdated) OnTasksUpdated?.Invoke();

            }
            catch (HttpRequestException ex)
            {
                // Check if this is an authorization error (4XX)
                if (ex.StatusCode != null && ((int)ex.StatusCode >= 400 && (int)ex.StatusCode < 500))
                {
                    // Authorization error - Stop polling and show login
                    StopPolling();

                    if (_hasConnectionError)
                    {
                        _hasConnectionError = false;
                        OnConnectionStatusChanged?.Invoke();
                    }
                    
                    if (!HasAuthorizationError)
                    {
                        HasAuthorizationError = true;
                        OnAuthorizationErrorChanged?.Invoke();
                    }
                }
                else
                {
                    // Connection error - Continue polling (retry) but show error
                    if (HasAuthorizationError)
                    {
                        HasAuthorizationError = false;
                        OnAuthorizationErrorChanged?.Invoke();
                    }
                    
                    if (!_hasConnectionError)
                    {
                        _hasConnectionError = true;
                        OnConnectionStatusChanged?.Invoke();
                    }
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Not authenticated"))
            {
                 // Should not happen if we validate before starting polling, but if it does, stop polling
                 StopPolling();
            }
            catch (Exception ex)
            {
                // Generic error - treat as connection error
                Console.WriteLine($"Polling error: {ex.Message}");
                
                if (HasAuthorizationError)
                {
                    HasAuthorizationError = false;
                    OnAuthorizationErrorChanged?.Invoke();
                }
                
                if (!_hasConnectionError)
                {
                    _hasConnectionError = true;
                    OnConnectionStatusChanged?.Invoke();
                }
            }
        }

        public List<Agent> GetAgents()
        {
            return _agents.Values.ToList();
        }

        public Agent? GetAgent(string id)
        {
            return _agents.TryGetValue(id, out var agent) ? agent : null;
        }

        public List<Listener> GetListeners()
        {
            return _listeners.Values.ToList();
        }

        public List<Implant> GetImplants()
        {
            return _implants.Values.ToList();
        }

        public void ClearCache()
        {
            _agents.Clear();
            _listeners.Clear();
            _implants.Clear();
            _taskResults.Clear();
            _tasks.Clear();
            _firstCall = true;
            _isInitialLoading = true;
            _totalChanges = 0;
            _processedChanges = 0;
            
            // Clear error states
            _hasConnectionError = false;
            HasAuthorizationError = false;
            
            // Notify all subscribers
            OnAgentsUpdated?.Invoke();
            OnListenersUpdated?.Invoke();
            OnImplantsUpdated?.Invoke();
            OnTasksUpdated?.Invoke();
            OnLoadingStateChanged?.Invoke();
            OnConnectionStatusChanged?.Invoke();
            OnAuthorizationErrorChanged?.Invoke();
        }

        public List<TeamServerAgentTask> GetTasks()
        {
            return _tasks.Values.ToList();
        }

        public List<TeamServerAgentTask> GetTasksForAgent(string agentId)
        {
            return _tasks.Values.Where(t => t.AgentId == agentId).OrderByDescending(t => t.RequestDate).ToList();
        }

        public AgentTaskResult? GetTaskResult(string taskId)
        {
            return _taskResults.TryGetValue(taskId, out var result) ? result : null;
        }

        public async Task<bool> StopListenerAsync(string listenerId)
        {
            var success = await _client.StopListenerAsync(listenerId);
            return success;
        }

        public async Task StopAgentAsync(string agentId)
        {
            await _client.StopAgentAsync(agentId);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
