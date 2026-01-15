using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Common.APIModels;
using Common.APIClient.Clients;

namespace Common.APIClient
{
    public class FractalApiClient
    {
        private readonly HttpClient _httpClient; 
        
        public AgentClient Agents { get; }
        public ListenerClient Listeners { get; }
        public TaskClient Tasks { get; }
        public ImplantClient Implants { get; }
        public LootClient Loot { get; }
        public ToolClient Tools { get; }
        public ProxyClient Proxy { get; }
        public WebHostClient WebHost { get; }

        public FractalApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            this.Agents = new AgentClient(httpClient);
            this.Listeners = new ListenerClient(httpClient);
            this.Tasks = new TaskClient(httpClient);
            this.Implants = new ImplantClient(httpClient);
            this.Loot = new LootClient(httpClient);
            this.Tools = new ToolClient(httpClient);
            this.Proxy = new ProxyClient(httpClient);
            this.WebHost = new WebHostClient(httpClient);
        }

        public async Task<List<Change>> GetChangesAsync(bool history)
        {
            var response = await _httpClient.GetAsync($"/session/Changes?history={history}");
            if (!response.IsSuccessStatusCode)
                return new List<Change>();
                
            var content = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Change>>(content) ?? new List<Change>();
        }

        public async Task CloseSessionAsync()
        {
             await _httpClient.GetAsync($"/session/exit");
        }
    }
}
