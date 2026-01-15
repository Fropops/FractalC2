using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels;

namespace Common.APIClient.Clients
{
    public class LootClient : BaseApiClient
    {
        public LootClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<Loot>?> GetAllAsync(string agentId)
        {
            return await GetAsync<List<Loot>>($"/loot/{agentId}");
        }

        public async Task<Loot?> GetFileAsync(string agentId, string fileName)
        {
            return await GetAsync<Loot>($"/loot/{agentId}/{fileName}");
        }

        public async Task CreateAsync(string agentId, Loot loot)
        {
            await PostAsync($"/loot/{agentId}/add", loot);
        }

        public async Task DeleteAsync(string agentId, string fileName)
        {
            await base.DeleteAsync($"/loot/{agentId}/{fileName}");
        }
    }
}
