using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels.WebHost;

namespace Common.APIClient.Clients
{
    public class WebHostClient : BaseApiClient
    {
        public WebHostClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<FileWebHost>?> GetAllAsync()
        {
            return await GetAsync<List<FileWebHost>>("/WebHost");
        }

        public async Task AddAsync(FileWebHost webHost)
        {
            await PostAsync("/WebHost", webHost);
        }

        public new async Task DeleteAsync(string path)
        {
            await base.DeleteAsync($"/WebHost?path={path}");
        }

        public async Task ClearAsync()
        {
            await GetAsync<object>("/WebHost/Clear");
        }

        public async Task<List<WebHostLog>?> GetLogsAsync()
        {
            return await GetAsync<List<WebHostLog>>("/WebHost/Logs");
        }
    }
}
