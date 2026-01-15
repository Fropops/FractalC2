using System.Net.Http.Json;
using WebCommander.Models;
using BinarySerializer;
using Shared;
using Common.Payload;
using Common.APIModels;
using Common;
using Common.Models;
using Common.APIClient;
using Listener = Common.Models.TeamServerListener;
using FileWebHost = Common.APIModels.WebHost.FileWebHost;

namespace WebCommander.Services
{
    public class TeamServerClient
    {
        private readonly HttpClient _client;
        private readonly AuthService _authService;
        private bool _isConfigured = false;
        private FractalApiClient _apiClient;

        public TeamServerClient(HttpClient client, AuthService authService)
        {
            _client = client;
            _authService = authService;
            _apiClient = new FractalApiClient(_client);
        }

        private async Task EnsureConfiguredAsync()
        {
            if (_isConfigured)
                return;

            var auth = await _authService.GetAuthConfigAsync();
            if (auth == null || string.IsNullOrWhiteSpace(auth.ServerUrl))
            {
                throw new InvalidOperationException("Not authenticated. Please configure authentication first.");
            }

            _client.BaseAddress = new Uri(auth.ServerUrl);
            var token = await _authService.GenerateTokenAsync();
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            
            // Re-initialize or update client if needed, but FractalApiClient uses the reference
             _apiClient = new FractalApiClient(_client);
            
            _isConfigured = true;
        }

        public async Task ReconfigureAsync()
        {
            _isConfigured = false;
            await EnsureConfiguredAsync();
        }

        public async Task ValidateAuthAsync()
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync("/Session/Auth");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Agent>> GetAgentsAsync()
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Agents.GetAllAsync() ?? new List<Agent>();
        }

        public async Task<List<Listener>> GetListenersAsync()
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Listeners.GetAllAsync() ?? new List<Listener>();
        }

        public async Task<Listener?> GetListenerAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Listeners.GetAsync(id);
        }

        public async Task<Agent?> GetAgentAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Agents.GetAsync(id);
        }

        public async Task<AgentMetadata?> GetAgentMetadataAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Agents.GetMetadataAsync(id);
        }

        public async Task<AgentTaskResult?> GetTaskResultAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Tasks.GetResultAsync(id);
        }

        public async Task<TeamServerAgentTask?> GetTaskAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Tasks.GetAsync(id);
        }

        public async Task<List<Change>> GetChangesAsync(bool history)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.GetChangesAsync(history);
        }

        public async Task StartListenerAsync(StartHttpListenerRequest request)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Listeners.CreateAsync(request);
        }

        public async Task<bool> StopListenerAsync(string id)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Listeners.DeleteAsync(id);
            return true;
        }

        public async Task StopAgentAsync(string id)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Agents.DeleteAsync(id);
        }

        public async Task<string> TaskAgent(string label, string agentId, CommandId commandId, ParameterDictionary parms)
        {
            await EnsureConfiguredAsync();
            
            // Original logic from TeamServerClient.TaskAgent AND ApiCommModule.TaskAgent
            // We need to serialize task here.
            
            var agentTask = new AgentTask()
            {
                Id = ShortGuid.NewGuid(),
                CommandId = commandId,
                Parameters = parms,
            };
            Console.WriteLine($"Into TaskAgent {parms.Count}");
            var ser = await agentTask.BinarySerializeAsync();

            var taskrequest = new CreateTaskRequest()
            {
                Command = label,
                Id = agentTask.Id,
                TaskBin = Convert.ToBase64String(ser),
            };

            await _apiClient.Tasks.CreateAsync(agentId, taskrequest);
            
            return agentTask.Id;
        }

        public async Task<List<APIImplant>> GetImplantsAsync()
        {   
            await EnsureConfiguredAsync();
            return await _apiClient.Implants.GetAllAsync();
        }

        public async Task<APIImplant?> GetImplantAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Implants.GetWithDataAsync(id); 
        }

        public async Task<(bool success, APIImplantCreationResult result)> CreateImplantAsync(ImplantConfig config)
        {
            await EnsureConfiguredAsync();
            try {
                var res = await _apiClient.Implants.GenerateAsync(config);
                return (true, res);
            } catch (Exception ex) {
                return (false, new APIImplantCreationResult { Logs = ex.Message });
            }
        }

        public async Task<bool> DeleteImplantAsync(string id)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Implants.DeleteAsync(id);
            return true;
        }

        public async Task<APIImplant?> GetImplantWithDataAsync(string id)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Implants.GetWithDataAsync(id);
        }

        // WebHost methods
        public async Task<List<FileWebHost>> GetWebHostFilesAsync()
        {
            await EnsureConfiguredAsync();
            return await _apiClient.WebHost.GetAllAsync();
        }

        public async Task<bool> AddWebHostFileAsync(FileWebHost file)
        {   
            await EnsureConfiguredAsync();
            await _apiClient.WebHost.AddAsync(file);
            return true;
        }

        public async Task<bool> DeleteWebHostFileAsync(string path)
        {
            await EnsureConfiguredAsync();
            await _apiClient.WebHost.DeleteAsync(path);
            return true;
        }

        // Tools methods
        public async Task<List<Tool>> GetToolsAsync(ToolType? type = null, string? name = null)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Tools.GetAllAsync(type, name);
        }

        public async Task<bool> CreateToolAsync(Tool tool)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Tools.AddAsync(tool); 
            return true;
        }

        // Loot methods
        public async Task<List<Loot>> GetLootsAsync(string agentId, bool includeThumbnail = true, bool includeData = false)
        {
            await EnsureConfiguredAsync();
             return await _apiClient.Loot.GetAllAsync(agentId);
        }

        public async Task<Loot?> GetLootAsync(string agentId, string fileName, bool includeData = true, bool includeThumbnail = true)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Loot.GetFileAsync(agentId, fileName);
        }

        public async Task<Loot?> GetLootThumbnailAsync(string agentId, string fileName)
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Loot.GetFileAsync(agentId, fileName);
        }

        public async Task<bool> CreateLootAsync(string agentId, Loot loot)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Loot.CreateAsync(agentId, loot);
            return true;
        }

        public async Task<bool> DeleteLootAsync(string agentId, string fileName)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Loot.DeleteAsync(agentId, fileName);
            return true;
        }

        // Proxy methods
        public async Task StartProxyAsync(string agentId, int port)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Proxy.StartAsync(agentId, port);
        }

        public async Task StopProxyAsync(int port)
        {
            await EnsureConfiguredAsync();
            await _apiClient.Proxy.StopAsync(port);
        }

        public async Task StartReversePortForwardAsync(string agentId, int port, string destHost, int destPort)
        {
            await EnsureConfiguredAsync();
            await _client.GetAsync($"/Agents/{agentId}/rportfwd/start?port={port}&destHost={Uri.EscapeDataString(destHost)}&destPort={destPort}");
        }

        public async Task StopReversePortForwardAsync(string agentId, int port)
        {
            await EnsureConfiguredAsync();
             await _client.GetAsync($"/Agents/{agentId}/rportfwd/stop?port={port}");
        }

        public async Task<List<ProxyInfo>> GetProxiesAsync()
        {
            await EnsureConfiguredAsync();
            return await _apiClient.Proxy.GetAllAsync();
        }
    }
}
