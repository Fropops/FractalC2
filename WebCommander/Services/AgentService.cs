using TeamServer.UI.Models;

namespace TeamServer.UI.Services
{
    public class AgentService : IDisposable
    {
        private readonly TeamServerClient _client;
        private readonly Dictionary<string, Agent> _agents = new();
        private readonly Dictionary<string, Listener> _listeners = new();
        private readonly Dictionary<string, AgentTaskResult> _taskResults = new();
        private readonly Dictionary<string, TeamServerAgentTask> _tasks = new();
        private readonly System.Threading.Timer _timer;
        private bool _firstCall = true;
        private bool _isInitialLoading = true;
        private int _totalChanges = 0;
        private int _processedChanges = 0;
        private bool _hasConnectionError = false;

        public event Action? OnAgentsUpdated;
        public event Action? OnListenersUpdated;
        public event Action? OnLoadingStateChanged;
        public event Action<Agent>? OnNewAgent;
        public event Action<AgentTaskResult>? OnAgentResult;
        public event Action? OnTasksUpdated;
        public event Action? OnProgressUpdated;
        public event Action? OnConnectionStatusChanged;

        public bool IsInitialLoading => _isInitialLoading;
        public int LoadingProgress => _totalChanges > 0 ? (_processedChanges * 100) / _totalChanges : 0;
        public bool HasConnectionError => _hasConnectionError;

        public AgentService(TeamServerClient client)
        {
            _client = client;
            
            // Start polling immediately and then every 2 seconds
            _timer = new System.Threading.Timer(async _ =>
            {
                await PollForChanges();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
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

                Console.WriteLine($"Received {changes.Count} changes.");

            // Track progress during initial loading
            if (_isInitialLoading)
            {
                _totalChanges = changes.Count;
                _processedChanges = 0;
                OnProgressUpdated?.Invoke();
            }

            bool agentsUpdated = false;
            bool listenersUpdated = false;
            bool tasksUpdated = false;

            foreach (var change in changes)
            {
                Console.WriteLine($"Processing change: {change.Type} for ID {change.Id}");
                
                if (change.Type == ChangingElement.Agent)
                {
                    try
                    {
                        var agent = await _client.GetAgentAsync(change.Id);
                        if (agent != null)
                        {
                            Console.WriteLine($"Updated agent {agent.Metadata?.ImplantId} ({agent.Id})");
                            
                            bool isNewAgent = !_agents.ContainsKey(agent.Id);
                            
                            // Fetch metadata if this is the first time we see this agent
                            if (isNewAgent)
                            {
                                Console.WriteLine($"Fetching metadata for agent {agent.Id}");
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
                        // Resource was deleted, remove from cache
                        if (_agents.Remove(change.Id))
                        {
                            Console.WriteLine($"Agent {change.Id} was deleted, removed from cache");
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
                            Console.WriteLine($"Updated listener {listener.Name} ({listener.Id})");
                            _listeners[listener.Id] = listener;
                            listenersUpdated = true;
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Resource was deleted, remove from cache
                        if (_listeners.Remove(change.Id))
                        {
                            Console.WriteLine($"Listener {change.Id} was deleted, removed from cache");
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
                            Console.WriteLine($"Received result for task {change.Id} with status {result.Status} : {result.Output}");
                            
                            // Cache the result
                            _taskResults[result.Id] = result;
                            
                            // Only notify if completed and not in initial loading
                            if (result.Status == AgentResultStatus.Completed && !_isInitialLoading)
                            {
                                OnAgentResult?.Invoke(result);
                            }
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Resource was deleted, remove from cache
                        if (_taskResults.Remove(change.Id))
                        {
                            Console.WriteLine($"Task result {change.Id} was deleted, removed from cache");
                        }
                    }
                }
                else if (change.Type == ChangingElement.Task)
                {
                    try
                    {
                        var task = await _client.GetTaskAsync(change.Id);
                        if (task != null)
                        {
                            Console.WriteLine($"Received task {task.Id} for agent {task.AgentId}: {task.Command}");
                            _tasks[task.Id] = task;
                            tasksUpdated = true;
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Resource was deleted, remove from cache
                        if (_tasks.Remove(change.Id))
                        {
                            Console.WriteLine($"Task {change.Id} was deleted, removed from cache");
                            tasksUpdated = true;
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
            if (agentsUpdated)
            {
                OnAgentsUpdated?.Invoke();
            }
            if (listenersUpdated)
            {
                OnListenersUpdated?.Invoke();
            }
            if (tasksUpdated)
            {
                OnTasksUpdated?.Invoke();
            }

                // Mark initial loading as complete after first poll
                if (_isInitialLoading)
                {
                    // Add a small delay to let the user see the 100% progress
                    await Task.Delay(500);
                    _isInitialLoading = false;
                    OnLoadingStateChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                // Set error state and notify UI
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

        public List<Listener> GetListeners()
        {
            return _listeners.Values.ToList();
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
            
            // Remove from cache if API call succeeded
            if (success && _listeners.Remove(listenerId))
            {
                OnListenersUpdated?.Invoke();
            }
            
            return success;
        }

        public async Task StopAgentAsync(string agentId)
        {
            await _client.StopAgentAsync(agentId);
            
            // Remove from cache if API call succeeded
            if (_agents.Remove(agentId))
            {
                OnAgentsUpdated?.Invoke();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
