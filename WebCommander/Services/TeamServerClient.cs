using System.Net.Http.Json;
using TeamServer.UI.Models;
using BinarySerializer;

namespace TeamServer.UI.Services
{
    public class TeamServerClient
    {
        private readonly HttpClient _client;

        public TeamServerClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Agent>> GetAgentsAsync()
        {
            var result = await _client.GetFromJsonAsync<List<Agent>>("/Agents");
            return result ?? new List<Agent>();
        }

        public async Task<List<Listener>> GetListenersAsync()
        {
            var result = await _client.GetFromJsonAsync<List<Listener>>("/Listeners");
            return result ?? new List<Listener>();
        }

        public async Task<Listener?> GetListenerAsync(string id)
        {
            var response = await _client.GetAsync($"/Listeners/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Listener>();
        }

        public async Task<Agent?> GetAgentAsync(string id)
        {
            var response = await _client.GetAsync($"/Agents/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Agent>();
        }

        public async Task<AgentMetadata?> GetAgentMetadataAsync(string id)
        {
            var response = await _client.GetAsync($"/Agents/{id}/metadata");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AgentMetadata>();
        }

        public async Task<AgentTaskResult?> GetTaskResultAsync(string id)
        {
            var response = await _client.GetAsync($"/Results/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AgentTaskResult>();
        }

        public async Task<TeamServerAgentTask?> GetTaskAsync(string id)
        {
            var response = await _client.GetAsync($"/Tasks/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TeamServerAgentTask>();
        }

        public async Task<List<Change>> GetChangesAsync(bool history)
        {
            var result = await _client.GetFromJsonAsync<List<Change>>($"/session/Changes?history={history}");
            return result ?? new List<Change>();
        }

        public async Task StartListenerAsync(StartHttpListenerRequest request)
        {
            await _client.PostAsJsonAsync("/Listeners", request);
        }

        public async Task<bool> StopListenerAsync(string id)
        {
            var response = await _client.DeleteAsync($"/Listeners?id={id}");
            return response.IsSuccessStatusCode;
        }

        public async Task StopAgentAsync(string id)
        {
            await _client.DeleteAsync($"/Agents/{id}");
        }

        public async Task<string> TaskAgent(string label, string agentId, CommandId commandId, ParameterDictionary parms)
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

            var response = await _client.PostAsJsonAsync($"/Agents/{agentId}", taskrequest);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response}");
            
            return agentTask.Id;
        }
    }
}
