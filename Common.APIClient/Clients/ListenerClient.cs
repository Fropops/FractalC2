using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels;
using Common.Models;

namespace Common.APIClient.Clients
{
    public class ListenerClient : BaseApiClient
    {
        public ListenerClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<TeamServerListener>?> GetAllAsync()
        {
            return await GetAsync<List<TeamServerListener>>("/Listeners");
        }

        public async Task<TeamServerListener?> GetAsync(string id)
        {
            return await GetAsync<TeamServerListener>($"/Listeners/{id}");
        }

        public async Task CreateAsync(StartHttpListenerRequest request)
        {
            await PostAsync("/Listeners/", request);
        }

        public new async Task DeleteAsync(string id)
        {
             await base.DeleteAsync($"/Listeners/{id}");
        }
    }
}
