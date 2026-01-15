using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.APIClient
{
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _httpClient;

        protected BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<T?> GetAsync<T>(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                // TODO: specific error handling?
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected async Task<string> GetStringAsync(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                 throw new HttpRequestException($"Error fetching {uri}: {response.StatusCode}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<T?> PostAsync<T, TRequest>(string uri, TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, request);
             if (!response.IsSuccessStatusCode)
            {
                 throw new HttpRequestException($"Error fetching {uri}: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            if(string.IsNullOrEmpty(json)) return default;
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        protected async Task PostAsync<TRequest>(string uri, TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, request);
            if (!response.IsSuccessStatusCode)
            {
                 throw new HttpRequestException($"Error posting {uri}: {response.StatusCode}");
            }
        }

        protected async Task DeleteAsync(string uri)
        {
            var response = await _httpClient.DeleteAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error deleting {uri}: {response.StatusCode}");
            }
        }
    }
}
