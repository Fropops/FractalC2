using System.Net.Http.Json;
using WebCommander.Models;
using BinarySerializer;

namespace WebCommander.Services
{
    public class TeamServerClient
    {
        private readonly HttpClient _client;
        private readonly AuthService _authService;
        private bool _isConfigured = false;

        public TeamServerClient(HttpClient client, AuthService authService)
        {
            _client = client;
            _authService = authService;
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
            var result = await _client.GetFromJsonAsync<List<Agent>>("/Agents");
            return result ?? new List<Agent>();
        }

        public async Task<List<Listener>> GetListenersAsync()
        {
            await EnsureConfiguredAsync();
            var result = await _client.GetFromJsonAsync<List<Listener>>("/Listeners");
            return result ?? new List<Listener>();
        }

        public async Task<Listener?> GetListenerAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Listeners/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Listener>();
        }

        public async Task<Agent?> GetAgentAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Agents/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Agent>();
        }

        public async Task<AgentMetadata?> GetAgentMetadataAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Agents/{id}/metadata");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AgentMetadata>();
        }

        public async Task<AgentTaskResult?> GetTaskResultAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Results/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AgentTaskResult>();
        }

        public async Task<TeamServerAgentTask?> GetTaskAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Tasks/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TeamServerAgentTask>();
        }

        public async Task<List<Change>> GetChangesAsync(bool history)
        {
            await EnsureConfiguredAsync();
            var result = await _client.GetFromJsonAsync<List<Change>>($"/session/Changes?history={history}");
            return result ?? new List<Change>();
        }

        public async Task StartListenerAsync(StartHttpListenerRequest request)
        {
            await EnsureConfiguredAsync();
            var response = await _client.PostAsJsonAsync("/Listeners", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    if (errorContent.TryGetProperty("detail", out var detail))
                    {
                        errorMsg = detail.GetString();
                    }
                }
                catch
                {
                    // ignore
                }
                throw new Exception(errorMsg);
            }
        }

        public async Task<bool> StopListenerAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.DeleteAsync($"/Listeners?id={id}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    if (errorContent.TryGetProperty("detail", out var detail))
                    {
                        errorMsg = detail.GetString();
                    }
                }
                catch
                {
                    // ignore
                }
                throw new Exception(errorMsg);
            }
            
            return true;
        }

        public async Task StopAgentAsync(string id)
        {
            await EnsureConfiguredAsync();
            await _client.DeleteAsync($"/Agents/{id}");
        }

        public async Task<string> TaskAgent(string label, string agentId, CommandId commandId, ParameterDictionary parms)
        {
            await EnsureConfiguredAsync();
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

            var response = await _client.PostAsJsonAsync($"/Agents/{agentId}", taskrequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    if (errorContent.TryGetProperty("detail", out var detail))
                    {
                        errorMsg = detail.GetString();
                    }
                }
                catch
                {
                    // ignore
                }
                throw new Exception(errorMsg);
            }
            
            return agentTask.Id;
        }

        public async Task<List<Implant>> GetImplantsAsync()
        {   
            await EnsureConfiguredAsync();
            var result = await _client.GetFromJsonAsync<List<Implant>>("/Implants");
            return result ?? new List<Implant>();
        }

        public async Task<Implant?> GetImplantAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Implants/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Implant>();
        }

        public async Task<(bool success, string details)> CreateImplantAsync(ImplantConfig config)
        {
            await EnsureConfiguredAsync();
            var response = await _client.PostAsJsonAsync("/Implants", config);
            // We don't ensure success status code here because we want to return the logs even if it failed (if the API returns logs on failure)
            // But if the user wants to treat non-200 as failure in the UI, we might need to handle it.
            // The user said: "si le retour est succès (200), il faut afficher un toaster pour dire que la création est un succès. Sinon, il faut afficher un toaster pour montre que la création est un échec."

            return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> DeleteImplantAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.DeleteAsync($"/Implants/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Implant?> GetImplantWithDataAsync(string id)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Implants/{id}?withData=true");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Implant>();
        }

        // WebHost methods
        public async Task<List<FileWebHost>> GetWebHostFilesAsync()
        {
            await EnsureConfiguredAsync();
            var result = await _client.GetFromJsonAsync<List<FileWebHost>>("/WebHost");
            return result ?? new List<FileWebHost>();
        }

        public async Task<bool> AddWebHostFileAsync(FileWebHost file)
        {   
            await EnsureConfiguredAsync();
            var response = await _client.PostAsJsonAsync("/WebHost", file);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWebHostFileAsync(string path)
        {
            await EnsureConfiguredAsync();
            var response = await _client.DeleteAsync($"/WebHost?path={Uri.EscapeDataString(path)}");
            return response.IsSuccessStatusCode;
        }

        // Tools methods
        public async Task<List<Tool>> GetToolsAsync(ToolType? type = null, string? name = null)
        {
            await EnsureConfiguredAsync();
            var url = "/Tools";
            var queryParams = new List<string>();

            if (type.HasValue)
            {
                queryParams.Add($"type={type.Value}");
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                queryParams.Add($"name={Uri.EscapeDataString(name)}");
            }

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }
            
            var result = await _client.GetFromJsonAsync<List<Tool>>(url);
            return result ?? new List<Tool>();
        }

        public async Task<bool> CreateToolAsync(Tool tool)
        {
            await EnsureConfiguredAsync();
            var response = await _client.PostAsJsonAsync("/Tools", tool);
            return response.IsSuccessStatusCode;
        }

        // Loot methods
        public async Task<List<Loot>> GetLootsAsync(string agentId, bool includeThumbnail = true, bool includeData = false)
        {
            await EnsureConfiguredAsync();
            var url = $"/loot/{agentId}?includeThumbnail={includeThumbnail}&includeData={includeData}";
            var result = await _client.GetFromJsonAsync<List<Loot>>(url);
            return result ?? new List<Loot>();
        }

        public async Task<Loot?> GetLootAsync(string agentId, string fileName, bool includeData = true, bool includeThumbnail = true)
        {
            await EnsureConfiguredAsync();
            var url = $"/loot/{agentId}/{Uri.EscapeDataString(fileName)}?includeData={includeData}&includeThumbnail={includeThumbnail}";
            var response = await _client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Loot>();
        }

        public async Task<Loot?> GetLootThumbnailAsync(string agentId, string fileName)
        {
            await EnsureConfiguredAsync();
            var url = $"/loot/{agentId}/thumbnail/{Uri.EscapeDataString(fileName)}";
            var response = await _client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Loot>();
        }

        public async Task<bool> CreateLootAsync(string agentId, Loot loot)
        {
            await EnsureConfiguredAsync();
            var response = await _client.PostAsJsonAsync($"/loot/{agentId}/add", loot);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteLootAsync(string agentId, string fileName)
        {
            await EnsureConfiguredAsync();
            var response = await _client.DeleteAsync($"/loot/{agentId}/{Uri.EscapeDataString(fileName)}");
            return response.IsSuccessStatusCode;
        }

        // Proxy methods
        public async Task StartProxyAsync(string agentId, int port)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Agents/{agentId}/startproxy?port={port}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    errorMsg = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    // ignore
                }
                throw new Exception($"Failed to start proxy: {response.StatusCode} {errorMsg}");
            }
        }

        public async Task StopProxyAsync(string agentId)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Agents/{agentId}/stopproxy");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    errorMsg = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    // ignore
                }
                throw new Exception($"Failed to stop proxy: {response.StatusCode} {errorMsg}");
            }
        }

        public async Task StartReversePortForwardAsync(string agentId, int port, string destHost, int destPort)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Agents/{agentId}/rportfwd/start?port={port}&destHost={Uri.EscapeDataString(destHost)}&destPort={destPort}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    errorMsg = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    // ignore
                }
                throw new Exception($"Failed to start reverse port forward: {response.StatusCode} {errorMsg}");
            }
        }

        public async Task StopReversePortForwardAsync(string agentId, int port)
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync($"/Agents/{agentId}/rportfwd/stop?port={port}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = response.ReasonPhrase;
                try
                {
                    errorMsg = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    // ignore
                }
                throw new Exception($"Failed to stop reverse port forward: {response.StatusCode} {errorMsg}");
            }
        }

        public async Task<List<ProxyInfo>> GetProxiesAsync()
        {
            await EnsureConfiguredAsync();
            var response = await _client.GetAsync("/Agents/proxy");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get proxies: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<ProxyInfo>>();
            return result ?? new List<ProxyInfo>();
        }
    }
}
