using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Common.APIModels;
using Common.Models;

namespace Common.APIClient.Clients
{
    public class ToolClient : BaseApiClient
    {
        public ToolClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<Tool>?> GetAllAsync(ToolType? type = null, string? name = null)
        {
            var query = new List<string>();
            if (type.HasValue) query.Add($"type={(int)type.Value}");
            if (!string.IsNullOrEmpty(name)) query.Add($"name={name}");
            
            var queryString = query.Any() ? "?" + string.Join("&", query) : string.Empty;

            return await GetAsync<List<Tool>>($"/Tools{queryString}");
        }

        public async Task AddAsync(Tool tool)
        {
            await PostAsync("/Tools", tool);
        }
    }
}
