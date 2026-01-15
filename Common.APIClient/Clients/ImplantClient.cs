using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels;
using Common.Payload;

namespace Common.APIClient.Clients
{
    public class ImplantClient : BaseApiClient
    {
        public ImplantClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<APIImplant>?> GetAllAsync()
        {
            return await GetAsync<List<APIImplant>>("/Implants");
        }

        public async Task<APIImplant?> GetAsync(string id)
        {
            return await GetAsync<APIImplant>($"/Implants/{id}");
        }

        public async Task<APIImplant?> GetWithDataAsync(string id)
        {
            return await GetAsync<APIImplant>($"/Implants/{id}?withData=true");
        }

        public async Task<APIImplantCreationResult?> GenerateAsync(ImplantConfig config)
        {
            return await PostAsync<APIImplantCreationResult, ImplantConfig>("/Implants", config);
        }

        public new async Task DeleteAsync(string id)
        {
            await base.DeleteAsync($"/Implants/{id}");
        }
    }
}
