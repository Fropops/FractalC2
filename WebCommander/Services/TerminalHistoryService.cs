using Microsoft.JSInterop;
using System.Text.Json;
using WebCommander.Models;

namespace WebCommander.Services
{
    public class TerminalHistoryService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string HISTORY_PREFIX = "terminal_history_";

        public TerminalHistoryService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SaveHistoryAsync(string agentId, TerminalHistory history)
        {
            try
            {
                var json = JsonSerializer.Serialize(history);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{HISTORY_PREFIX}{agentId}", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving terminal history for agent {agentId}: {ex.Message}");
            }
        }

        public async Task<TerminalHistory?> LoadHistoryAsync(string agentId)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", $"{HISTORY_PREFIX}{agentId}");
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<TerminalHistory>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading terminal history for agent {agentId}: {ex.Message}");
            }

            return null;
        }

        public async Task ClearHistoryAsync(string agentId)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"{HISTORY_PREFIX}{agentId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing terminal history for agent {agentId}: {ex.Message}");
            }
        }
    }
}
