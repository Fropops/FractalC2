using System.Net.Http.Json;
using WebCommander.Models;
using BinarySerializer;

namespace WebCommander.Services
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
                throw new Exception($"{response}");
            
            return agentTask.Id;
        }

        public async Task<List<Implant>> GetImplantsAsync()
        {
            var result = await _client.GetFromJsonAsync<List<Implant>>("/Implants");
            return result ?? new List<Implant>();
        }

        public async Task<Implant?> GetImplantAsync(string id)
        {
            var response = await _client.GetAsync($"/Implants/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Implant>();
        }

        public async Task<(bool success, string details)> CreateImplantAsync(ImplantConfig config)
        {
            var response = await _client.PostAsJsonAsync("/Implants", config);
            // We don't ensure success status code here because we want to return the logs even if it failed (if the API returns logs on failure)
            // But if the user wants to treat non-200 as failure in the UI, we might need to handle it.
            // The user said: "si le retour est succès (200), il faut afficher un toaster pour dire que la création est un succès. Sinon, il faut afficher un toaster pour montre que la création est un échec."

            return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> DeleteImplantAsync(string id)
        {
            var response = await _client.DeleteAsync($"/Implants/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Implant?> GetImplantWithDataAsync(string id)
        {
            var response = await _client.GetAsync($"/Implants/{id}?withData=true");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Implant>();
        }

        // WebHost methods
        public async Task<List<FileWebHost>> GetWebHostFilesAsync()
        {
            var result = await _client.GetFromJsonAsync<List<FileWebHost>>("/WebHost");
            return result ?? new List<FileWebHost>();
        }

        public async Task<bool> AddWebHostFileAsync(FileWebHost file)
        {
            var response = await _client.PostAsJsonAsync("/WebHost", file);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWebHostFileAsync(string path)
        {
            var response = await _client.DeleteAsync($"/WebHost?path={Uri.EscapeDataString(path)}");
            return response.IsSuccessStatusCode;
        }
    }
}
