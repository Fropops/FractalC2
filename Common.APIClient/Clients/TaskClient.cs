using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels;
using Common.Models;
using Shared;

namespace Common.APIClient.Clients
{
    public class TaskClient : BaseApiClient
    {
        public TaskClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<TeamServerAgentTask?> GetAsync(string id)
        {
            return await GetAsync<TeamServerAgentTask>($"/Tasks/{id}");
        }

        public async Task<AgentTaskResult?> GetResultAsync(string id)
        {
            return await GetAsync<AgentTaskResult>($"/Results/{id}");
        }

        public async Task CreateAsync(string agentId, CreateTaskRequest request)
        {
            await PostAsync($"/Agents/{agentId}", request);
        }
    }
}
