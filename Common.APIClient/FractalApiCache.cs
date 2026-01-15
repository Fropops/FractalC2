using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.APIModels;
using Common.Models;
using Shared;

namespace Common.APIClient
{
    public class FractalApiCache
    {
        public ConcurrentDictionary<string, Agent> Agents { get; } = new();
        public ConcurrentDictionary<string, TeamServerListener> Listeners { get; } = new();
        public ConcurrentDictionary<string, TeamServerAgentTask> Tasks { get; } = new();
        public ConcurrentDictionary<string, AgentTaskResult> Results { get; } = new();
        public ConcurrentDictionary<string, APIImplant> Implants { get; } = new();

        public event Action<Agent>? OnAgentUpdated;
        public event Action<string>? OnAgentRemoved;
        
        public event Action<TeamServerListener>? OnListenerUpdated;
        public event Action<string>? OnListenerRemoved;
        
        public event Action<TeamServerAgentTask>? OnTaskUpdated;
        public event Action<string>? OnTaskRemoved;
        
        public event Action<AgentTaskResult>? OnResultUpdated;
        public event Action<string>? OnResultRemoved;

        public event Action<APIImplant>? OnImplantUpdated;
        public event Action<string>? OnImplantRemoved;

        private bool _isInitialLoading = true;
        public bool IsInitialLoading
        {
            get => _isInitialLoading;
            set
            {
                if (_isInitialLoading != value)
                {
                    _isInitialLoading = value;
                    OnLoadingStateChanged?.Invoke();
                }
            }
        }
        public event Action? OnLoadingStateChanged;

        public void Clear()
        {
            Agents.Clear();
            Listeners.Clear();
            Tasks.Clear();
            Results.Clear();
            Implants.Clear();
            
            IsInitialLoading = true;
            OnLoadingStateChanged?.Invoke();
        }

        public void UpdateAgent(Agent agent)
        {
            Agents.AddOrUpdate(agent.Id, agent, (k, v) => agent);
            OnAgentUpdated?.Invoke(agent);
        }

        public void RemoveAgent(string id)
        {
            if (Agents.TryRemove(id, out _))
            {
                OnAgentRemoved?.Invoke(id);
            }
        }

        public void UpdateListener(TeamServerListener listener)
        {
            Listeners.AddOrUpdate(listener.Id, listener, (k, v) => listener);
            OnListenerUpdated?.Invoke(listener);
        }

        public void RemoveListener(string id)
        {
            if (Listeners.TryRemove(id, out _))
            {
                OnListenerRemoved?.Invoke(id);
            }
        }

        public void UpdateTask(TeamServerAgentTask task)
        {
            Tasks.AddOrUpdate(task.Id, task, (k, v) => task);
            OnTaskUpdated?.Invoke(task);
        }

        public void RemoveTask(string id)
        {
            if (Tasks.TryRemove(id, out _))
            {
                OnTaskRemoved?.Invoke(id);
            }
        }

        public void UpdateResult(AgentTaskResult result)
        {
            Results.AddOrUpdate(result.Id, result, (k, v) => result);
            OnResultUpdated?.Invoke(result);
        }

        public void RemoveResult(string id)
        {
            if (Results.TryRemove(id, out _))
            {
                OnResultRemoved?.Invoke(id);
            }
        }

        public void UpdateImplant(APIImplant implant)
        {
            Implants.AddOrUpdate(implant.Id, implant, (k, v) => implant);
            OnImplantUpdated?.Invoke(implant);
        }

        public void RemoveImplant(string id)
        {
            if (Implants.TryRemove(id, out _))
            {
                OnImplantRemoved?.Invoke(id);
            }
        }
    }
}
