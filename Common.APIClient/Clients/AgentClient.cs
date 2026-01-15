using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels;
using Common.Models;
using Shared;

namespace Common.APIClient.Clients
{
    public class AgentClient : BaseApiClient
    {
        public AgentClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<Agent>?> GetAllAsync()
        {
            return await GetAsync<List<Agent>>("/Agents");
        }

        public async Task<Agent?> GetAsync(string id)
        {
            return await GetAsync<Agent>($"/Agents/{id}");
        }

        public new async Task DeleteAsync(string id)
        {
            await base.DeleteAsync($"/Agents/{id}");
        }
        
        public async Task<AgentMetadata?> GetMetadataAsync(string id)
        {
            return await GetAsync<AgentMetadata>($"/agents/{id}/metadata");
        }
    }
}
