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

        private const int MAX_STORED_LINES = 1000;
        private const int FALLBACK_STORED_LINES = 500;

        public async Task SaveHistoryAsync(string agentId, TerminalHistory history)
        {
            try
            {
                if (history.OutputLines.Count > MAX_STORED_LINES)
                {
                    history.OutputLines = history.OutputLines.TakeLast(MAX_STORED_LINES).ToList();
                }

                var json = JsonSerializer.Serialize(history);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{HISTORY_PREFIX}{agentId}", json);
            }
            catch (JSException jsEx) when (jsEx.Message.Contains("QuotaExceededError", StringComparison.OrdinalIgnoreCase) || 
                                           jsEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"LocalStorage quota exceeded for agent {agentId}, trimming history to {FALLBACK_STORED_LINES} lines...");
                try
                {
                    if (history.OutputLines.Count > FALLBACK_STORED_LINES)
                    {
                        history.OutputLines = history.OutputLines.TakeLast(FALLBACK_STORED_LINES).ToList();
                    }
                    var fallbackJson = JsonSerializer.Serialize(history);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{HISTORY_PREFIX}{agentId}", fallbackJson);
                }
                catch (Exception retryEx)
                {
                    Console.WriteLine($"Failed to save trimmed terminal history for agent {agentId}: {retryEx.Message}");
                }
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
