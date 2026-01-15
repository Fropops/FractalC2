using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.APIModels;

namespace Common.APIClient.Clients
{
    public class ProxyClient : BaseApiClient
    {
        public ProxyClient(HttpClient httpClient) : base(httpClient) { }

        public async Task StartAsync(string agentId, int port)
        {
            // Note: Endpoints were using GET in ApiCommModule, checking if that's standard.
            // Keeping as GET per existing implementation but wrapped in Task
            await GetAsync<object>($"/proxy/start?agentId={agentId}&port={port}");
        }

        public async Task StopAsync(int port)
        {
             await GetAsync<object>($"/proxy/stop?port={port}");
        }

        public async Task<List<ProxyInfo>?> GetAllAsync()
        {
            return await GetAsync<List<ProxyInfo>>("/proxy");
        }
    }
}
