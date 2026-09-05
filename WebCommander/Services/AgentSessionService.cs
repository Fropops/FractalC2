using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCommander.Services
{
    public class AgentSessionService
    {
        public List<string> OpenAgentIds { get; } = new();
        public string? ActiveAgentId { get; private set; }
        public Dictionary<string, string> ActiveViews { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> UnreadAgents { get; } = new(StringComparer.OrdinalIgnoreCase);

        public event Action? OnSessionsChanged;

        public string GetActiveView(string agentId)
        {
            if (ActiveViews.TryGetValue(agentId, out var view))
            {
                return view;
            }
            return "terminal";
        }

        public void OpenSession(string agentId, string view = "terminal")
        {
            if (string.IsNullOrWhiteSpace(agentId))
                return;

            if (!OpenAgentIds.Contains(agentId))
            {
                OpenAgentIds.Add(agentId);
            }

            ActiveAgentId = agentId;
            ActiveViews[agentId] = view.ToLowerInvariant();
            UnreadAgents.Remove(agentId);

            OnSessionsChanged?.Invoke();
        }

        public void SelectSession(string agentId)
        {
            if (OpenAgentIds.Contains(agentId))
            {
                ActiveAgentId = agentId;
                UnreadAgents.Remove(agentId);
                OnSessionsChanged?.Invoke();
            }
        }

        public void SetSessionView(string agentId, string view)
        {
            if (!string.IsNullOrWhiteSpace(agentId))
            {
                ActiveViews[agentId] = view.ToLowerInvariant();
                OnSessionsChanged?.Invoke();
            }
        }

        public void CloseSession(string agentId)
        {
            if (OpenAgentIds.Contains(agentId))
            {
                int index = OpenAgentIds.IndexOf(agentId);
                OpenAgentIds.Remove(agentId);
                ActiveViews.Remove(agentId);
                UnreadAgents.Remove(agentId);

                if (ActiveAgentId == agentId)
                {
                    if (OpenAgentIds.Count > 0)
                    {
                        // Select adjacent tab
                        int newIndex = Math.Min(index, OpenAgentIds.Count - 1);
                        ActiveAgentId = OpenAgentIds[newIndex];
                        UnreadAgents.Remove(ActiveAgentId);
                    }
                    else
                    {
                        ActiveAgentId = null;
                    }
                }

                OnSessionsChanged?.Invoke();
            }
        }

        public void CloseAllSessions()
        {
            OpenAgentIds.Clear();
            ActiveViews.Clear();
            UnreadAgents.Clear();
            ActiveAgentId = null;
            OnSessionsChanged?.Invoke();
        }

        public void CloseOtherSessions(string keepAgentId)
        {
            if (OpenAgentIds.Contains(keepAgentId))
            {
                OpenAgentIds.RemoveAll(id => id != keepAgentId);
                var viewsToRemove = ActiveViews.Keys.Where(id => id != keepAgentId).ToList();
                foreach (var id in viewsToRemove)
                {
                    ActiveViews.Remove(id);
                    UnreadAgents.Remove(id);
                }
                ActiveAgentId = keepAgentId;
                UnreadAgents.Remove(keepAgentId);
                OnSessionsChanged?.Invoke();
            }
        }

        public void MarkUnread(string agentId)
        {
            if (ActiveAgentId != agentId && OpenAgentIds.Contains(agentId))
            {
                if (UnreadAgents.Add(agentId))
                {
                    OnSessionsChanged?.Invoke();
                }
            }
        }

        public void MarkRead(string agentId)
        {
            if (UnreadAgents.Remove(agentId))
            {
                OnSessionsChanged?.Invoke();
            }
        }
    }
}
