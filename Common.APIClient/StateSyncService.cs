using System;
using System.Threading;
using System.Threading.Tasks;
using Common.APIModels;
using Common.Models;

namespace Common.APIClient
{
    public class StateSyncService : IDisposable
    {
        private readonly FractalApiClient _client;
        private readonly FractalApiCache _cache;
        private CancellationTokenSource? _cts;
        private bool _isPolling;
        private bool _firstCall = true;

        public event Action<bool>? OnConnectionStatusChanged;
        public event Action<int, int>? OnInitialSyncProgress;
        public bool IsConnected { get; private set; } = false;

        public StateSyncService(FractalApiClient client, FractalApiCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public void Start()
        {
            if (_isPolling) return;
            _isPolling = true;
            _cts = new CancellationTokenSource();
            
            Task.Run(() => PollLoop(_cts.Token));
        }

        public void Stop()
        {
            _isPolling = false;
            _cts?.Cancel();
        }

        private async Task PollLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var changes = await _client.GetChangesAsync(_firstCall);
                    _firstCall = false;

                    if (!IsConnected)
                    {
                        IsConnected = true;
                        OnConnectionStatusChanged?.Invoke(true);
                    }

                    if (_cache.IsInitialLoading && changes.Count > 0)
                    {
                         OnInitialSyncProgress?.Invoke(0, changes.Count);
                    }

                    int current = 0;
                    int total = changes.Count;

                    foreach (var change in changes)
                    {
                        await HandleChange(change);
                        current++;
                        if (_cache.IsInitialLoading)
                        {
                            OnInitialSyncProgress?.Invoke(current, total);
                        }
                    }

                    if (_cache.IsInitialLoading)
                    {
                        _cache.IsInitialLoading = false;
                    }
                }
                catch (Exception)
                {
                    // Basic error handling
                    if (IsConnected)
                    {
                        IsConnected = false;
                        OnConnectionStatusChanged?.Invoke(false);
                    }
                }

                await Task.Delay(2000, token);
            }
        }

        private async Task HandleChange(Change change)
        {
            switch (change.Element)
            {
                case ChangingElement.Agent:
                    var tsAgent = await _client.Agents.GetAsync(change.Id);
                    if (tsAgent != null)
                    {
                        var agent = new Agent
                        {
                            Id = tsAgent.Id,
                            FirstSeen = tsAgent.FirstSeen,
                            LastSeen = tsAgent.LastSeen,
                            Links = tsAgent.Links,
                            RelayId = tsAgent.RelayId,
                        };

                        if (_cache.Agents.TryGetValue(tsAgent.Id, out var existingAgent))
                        {
                            agent.Metadata = existingAgent.Metadata;
                            agent.Pings = existingAgent.Pings;
                        }

                        if (agent.Metadata == null)
                        {
                            agent.Metadata = await _client.Agents.GetMetadataAsync(agent.Id);
                        }

                        _cache.UpdateAgent(agent);
                    }
                    else
                    {
                        _cache.RemoveAgent(change.Id);
                    }
                    break;
                case ChangingElement.Listener:
                    var listener = await _client.Listeners.GetAsync(change.Id);
                    if (listener != null) _cache.UpdateListener(listener);
                    else _cache.RemoveListener(change.Id);
                    break;
                case ChangingElement.Task:
                    var task = await _client.Tasks.GetAsync(change.Id);
                    if (task != null) _cache.UpdateTask(task);
                    else _cache.RemoveTask(change.Id);
                    break;
                case ChangingElement.Result:
                    var result = await _client.Tasks.GetResultAsync(change.Id);
                    if (result != null) _cache.UpdateResult(result);
                    else _cache.RemoveResult(change.Id);
                    break;
                case ChangingElement.Implant:
                    var implant = await _client.Implants.GetAsync(change.Id);
                     if (implant != null) _cache.UpdateImplant(implant);
                    else _cache.RemoveImplant(change.Id);
                    break;
                 case ChangingElement.Metadata:
                    // Handled inside Agent update or specific call?
                    // In ApiCommModule it fetches Metadata separate
                    var meta = await _client.Agents.GetMetadataAsync(change.Id);
                    if (meta != null && _cache.Agents.TryGetValue(meta.Id, out var agentToUpdate))
                    {
                        agentToUpdate.Metadata = meta;
                         _cache.UpdateAgent(agentToUpdate);
                    }
                    break;
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
